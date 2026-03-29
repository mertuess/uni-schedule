// Модуль для работы с аутентификацией

// Сохраняем учетные данные
function saveAuth(email, password) {
    localStorage.setItem('authEmail', email);
    localStorage.setItem('authPassword', password);
    setAuth(email, password);
}

// Загружаем учетные данные из localStorage
function loadAuth() {
    const email = localStorage.getItem('authEmail');
    const password = localStorage.getItem('authPassword');
    
    if (email && password) {
        setAuth(email, password);
        return true;
    }
    return false;
}

// Очищаем учетные данные при выходе
function clearAuth() {
    localStorage.removeItem('authEmail');
    localStorage.removeItem('authPassword');
    localStorage.removeItem('userRole');
    localStorage.removeItem('userName');
    localStorage.removeItem('userEmail');
    setAuth(null, null);
}

// Проверяем, авторизован ли пользователь
function isAuthenticated() {
    return localStorage.getItem('authEmail') !== null;
}

// Получаем роль текущего пользователя
function getUserRole() {
    return localStorage.getItem('userRole');
}

// Сохраняем роль пользователя (только для оператора)
function setUserRole(role) {
    localStorage.setItem('userRole', role);
}

// Проверяем, является ли пользователь оператором
function isOperator() {
    const role = getUserRole();
    return role === 'operator';
}