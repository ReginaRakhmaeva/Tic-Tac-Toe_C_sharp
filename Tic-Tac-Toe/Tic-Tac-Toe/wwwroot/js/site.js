// Безопасный разбор accessToken и извлечение uuid (userId)
function getUserIdFromToken(accessToken) {
    if (!accessToken) return null;
    try {
        const parts = accessToken.split('.');
        if (parts.length !== 3) {
            return null;
        }
        const payloadJson = atob(parts[1]);
        const payload = JSON.parse(payloadJson);
        return payload.uuid || null;
    } catch (e) {
        console.error('Ошибка при разборе accessToken:', e);
        return null;
    }
}

function ensureUserIdFromToken() {
    const accessToken = localStorage.getItem('accessToken');
    if (!accessToken) {
        return;
    }

    const existingUserId = localStorage.getItem('userId');
    if (existingUserId) {
        return;
    }

    const uuid = getUserIdFromToken(accessToken);
    if (uuid) {
        localStorage.setItem('userId', uuid);
    }
}

function logout() {
    localStorage.removeItem('authCredentials');
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('userId');
    localStorage.removeItem('userLogin');
    
    window.location.href = '/Login';
}

function getLogin() {
    return localStorage.getItem('userLogin');
}

function updateNavigation() {
    const accessToken = localStorage.getItem('accessToken');
    const userInfo = document.getElementById('userInfo');
    const loginLink = document.getElementById('loginLink');
    const userLogin = document.getElementById('userLogin');
    
    if (accessToken) {
        ensureUserIdFromToken();

        const login = getLogin();
        if (login && userLogin) {
            userLogin.textContent = login;
        }
        if (userInfo) {
            userInfo.style.display = 'block';
        }
        if (loginLink) {
            loginLink.style.display = 'none';
        }
    } else {
        if (userInfo) {
            userInfo.style.display = 'none';
        }
        if (loginLink) {
            loginLink.style.display = 'block';
        }
    }
}

document.addEventListener('DOMContentLoaded', function() {
    ensureUserIdFromToken();

    updateNavigation();
    
    const logoutBtn = document.getElementById('logoutBtn');
    if (logoutBtn) {
        logoutBtn.addEventListener('click', function(e) {
            e.preventDefault();
            logout();
        });
    }
});
function hideNavigation() {
    const navbar = document.querySelector('nav.navbar');
    if (navbar) {
        navbar.style.display = 'none';
    }
}

function showNavigation() {
    const navbar = document.querySelector('nav.navbar');
    if (navbar) {
        navbar.style.display = 'block';
    }
}
window.logout = logout;
window.updateNavigation = updateNavigation;
window.getUserIdFromToken = getUserIdFromToken;
window.ensureUserIdFromToken = ensureUserIdFromToken;
window.hideNavigation = hideNavigation;
window.showNavigation = showNavigation;