using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Tic_Tac_Toe.web.model;
using Tic_Tac_Toe.web.service;

namespace Tic_Tac_Toe.web.middleware;

/// Атрибут для авторизации пользователей через Basic Authentication
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

        var authService = context.HttpContext.RequestServices.GetService<IAuthService>();
        
        if (authService == null)
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

        Guid? userId = authService.Authenticate(authorizationHeader);

        if (userId == null)
        {
            context.Result = new UnauthorizedObjectResult(
                new ErrorResponse("Invalid login or password"));
            return;
        }

        context.HttpContext.Items["UserId"] = userId.Value;

        await Task.CompletedTask;
    }
}
