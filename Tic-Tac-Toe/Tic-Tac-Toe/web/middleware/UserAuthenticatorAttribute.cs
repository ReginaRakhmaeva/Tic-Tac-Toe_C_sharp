using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Tic_Tac_Toe.web.model;
using Tic_Tac_Toe.web.service;

namespace Tic_Tac_Toe.web.middleware;

/// Атрибут для авторизации пользователей через Bearer Authentication
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class UserAuthenticatorAttribute : Attribute, IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (context.ActionDescriptor is Microsoft.AspNetCore.Mvc.RazorPages.CompiledPageActionDescriptor)
        {
            await Task.CompletedTask;
            return;
        }

        var endpoint = context.HttpContext.GetEndpoint();
        if (endpoint != null)
        {
            if (endpoint.Metadata.GetMetadata<IAllowAnonymous>() != null)
            {
                await Task.CompletedTask;
                return;
            }
        }

        var actionDescriptor = context.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor;
        if (actionDescriptor != null)
        {
            var hasAllowAnonymousOnMethod = actionDescriptor.MethodInfo
                .GetCustomAttributes(typeof(AllowAnonymousAttribute), inherit: true)
                .Any();

            if (hasAllowAnonymousOnMethod)
            {
                await Task.CompletedTask;
                return;
            }

            var hasAllowAnonymousOnController = actionDescriptor.ControllerTypeInfo
                .GetCustomAttributes(typeof(AllowAnonymousAttribute), inherit: true)
                .Any();

            if (hasAllowAnonymousOnController)
            {
                await Task.CompletedTask;
                return;
            }
        }

        var jwtProvider = context.HttpContext.RequestServices.GetService<JwtProvider>();
        
        if (jwtProvider == null)
        {
            context.Result = new StatusCodeResult(500);
            return;
        }

        if (!context.HttpContext.Request.Headers.ContainsKey("Authorization"))
        {
            context.Result = new UnauthorizedObjectResult(
                new ErrorResponse("Authorization header is required"));
            return;
        }

        string authorizationHeader = context.HttpContext.Request.Headers["Authorization"].ToString();

        if (string.IsNullOrWhiteSpace(authorizationHeader))
        {
            context.Result = new UnauthorizedObjectResult(
                new ErrorResponse("Authorization header is empty"));
            return;
        }

        string token = authorizationHeader.Trim();
        if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            token = token.Substring(7);
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            context.Result = new UnauthorizedObjectResult(
                new ErrorResponse("Token is required"));
            return;
        }

        if (!jwtProvider.ValidateAccessToken(token))
        {
            context.Result = new UnauthorizedObjectResult(
                new ErrorResponse("Invalid or expired token"));
            return;
        }

        var claims = jwtProvider.GetClaims(token);
        if (claims == null)
        {
            context.Result = new UnauthorizedObjectResult(
                new ErrorResponse("Invalid token"));
            return;
        }

        var uuidClaim = claims.FindFirst("uuid");
        if (uuidClaim == null || !Guid.TryParse(uuidClaim.Value, out Guid userId))
        {
            context.Result = new UnauthorizedObjectResult(
                new ErrorResponse("Invalid token"));
            return;
        }

        var identity = claims.Identity as ClaimsIdentity ?? new ClaimsIdentity();
        var userPrincipal = new UserIdPrincipal(userId, identity);
        context.HttpContext.User = userPrincipal;

        context.HttpContext.Items["UserId"] = userId;

        await Task.CompletedTask;
    }
}
