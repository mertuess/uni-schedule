// ============================================
// ФАЙЛ СОЗДАНИЯ ПОЛЬЗОВАТЕЛЯ
// Отвечает за создание нового пользователя
// ============================================

document.addEventListener('DOMContentLoaded', function() {
    // Проверяем права оператора
    if (!checkOperator()) {
        return;
    }
    
    // Назначаем обработчик кнопки "Создать"
    const createBtn = document.getElementById('create-btn');
    if (createBtn) {
        createBtn.addEventListener('click', createUserHandler);
    }
});

// Создание нового пользователя
async function createUserHandler() {
    const email = document.getElementById('email').value;
    const password = document.getElementById('password').value;
    const confirmPassword = document.getElementById('confirm-password').value;
    const role = document.getElementById('role').value;
    const errorDiv = document.getElementById('error-message');
    const successDiv = document.getElementById('success-message');
    
    // Скрываем предыдущие сообщения
    errorDiv.style.display = 'none';
    successDiv.style.display = 'none';
    
    // Проверка заполнения полей
    if (!email || !password || !role) {
        errorDiv.textContent = 'Заполните все поля';
        errorDiv.style.display = 'block';
        return;
    }
    
    // Проверка длины пароля
    if (password.length < 6) {
        errorDiv.textContent = 'Пароль должен быть не менее 6 символов';
        errorDiv.style.display = 'block';
        return;
    }
    
    // Проверка совпадения паролей
    if (password !== confirmPassword) {
        errorDiv.textContent = 'Пароли не совпадают';
        errorDiv.style.display = 'block';
        return;
    }
    
    // Формируем данные для отправки
    const userData = {
        mail: email,
        password: password,
        role: role
    };
    
    // Отправляем запрос на создание
    const createBtn = document.getElementById('create-btn');
    createBtn.disabled = true;
    createBtn.textContent = 'Создание...';
    
    const result = await createUser(userData);
    
    createBtn.disabled = false;
    createBtn.textContent = 'Создать пользователя';
    
    if (result) {
        successDiv.textContent = 'Пользователь успешно создан';
        successDiv.style.display = 'block';
        
        // Очищаем форму
        document.getElementById('email').value = '';
        document.getElementById('password').value = '';
        document.getElementById('confirm-password').value = '';
        document.getElementById('role').value = '';
        
        // Возвращаемся на админ-панель
        setTimeout(function() {
            window.location.href = 'dashboard.html';
        }, 1500);
    }
}