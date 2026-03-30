// Создание нового пользователя

// Проверка доступа - только для оператора
async function checkAccess() {
    if (!isAuthenticated()) {
        window.location.href = '../index.html';
        return false;
    }
    
    loadAuth();
    
    // Получаем роль из localStorage
    const userRole = getUserRole();
    
    // Если не оператор, перенаправляем на главную
    if (userRole !== 'operator') {
        window.location.href = '../index.html';
        return false;
    }
    
    return true;
}

// Обработка создания пользователя
async function handleCreateUser(event) {
    event.preventDefault();
    
    const email = document.getElementById('email').value;
    const password = document.getElementById('password').value;
    const name = document.getElementById('name').value;
    const engName = document.getElementById('engName').value;
    const role = document.getElementById('role').value;
    const messageBox = document.getElementById('messageBox');
    
    // Проверка обязательных полей
    if (!email || !password || !name || !engName || !role) {
        messageBox.textContent = 'Заполните все обязательные поля';
        messageBox.className = 'message-box error';
        return;
    }
    
    // Создаем пользователя
    const result = await createUser(email, password, name, engName, role);
    
    if (result.success) {
        messageBox.textContent = 'Пользователь успешно создан!';
        messageBox.className = 'message-box success';
        
        // Очищаем форму
        document.getElementById('createUserForm').reset();
        
        // Перенаправляем через 2 секунды
        setTimeout(() => {
            window.location.href = 'dashboard.html';
        }, 2000);
    } else {
        messageBox.textContent = 'Ошибка создания: ' + result.error;
        messageBox.className = 'message-box error';
    }
}

// Инициализация
(async function init() {
    const hasAccess = await checkAccess();
    if (hasAccess) {
        const createForm = document.getElementById('createUserForm');
        if (createForm) {
            createForm.addEventListener('submit', handleCreateUser);
        }
    }
})();