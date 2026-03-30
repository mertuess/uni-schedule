// ============================================
// ФАЙЛ РЕДАКТИРОВАНИЯ ПОЛЬЗОВАТЕЛЯ
// Отвечает за загрузку и сохранение данных пользователя
// ============================================

let userId = null; // ID редактируемого пользователя

document.addEventListener('DOMContentLoaded', async function() {
    // Проверяем права оператора
    if (!checkOperator()) {
        return;
    }
    
    // Получаем ID пользователя из URL (?id=123)
    const urlParams = new URLSearchParams(window.location.search);
    userId = urlParams.get('id');
    
    if (!userId) {
        alert('ID пользователя не указан');
        window.location.href = 'dashboard.html';
        return;
    }
    
    // Загружаем данные пользователя
    const user = await getUserById(userId);
    
    if (!user) {
        alert('Пользователь не найден. ID: ' + userId);
        window.location.href = 'dashboard.html';
        return;
    }
    
    // Заполняем форму данными пользователя
    document.getElementById('email').value = user.mail;
    document.getElementById('role').value = user.role;
    document.getElementById('user-id').textContent = user.id;
    
    // Назначаем обработчик кнопки "Сохранить"
    const saveBtn = document.getElementById('save-btn');
    if (saveBtn) {
        saveBtn.addEventListener('click', updateUserHandler);
    }
    
    // Назначаем обработчик кнопки "Удалить" (если не оператор)
    const deleteBtn = document.getElementById('delete-btn');
    if (deleteBtn) {
        if (user.role !== 'operator') {
            deleteBtn.addEventListener('click', deleteUserHandler);
        } else {
            deleteBtn.style.display = 'none';
        }
    }
});

// Сохранение изменений пользователя
async function updateUserHandler() {
    const email = document.getElementById('email').value;
    const role = document.getElementById('role').value;
    const password = document.getElementById('password').value;
    const messageDiv = document.getElementById('message');
    
    // Скрываем предыдущие сообщения
    messageDiv.style.display = 'none';
    messageDiv.className = 'error';
    messageDiv.style.background = '';
    
    // Проверка email
    if (!email) {
        messageDiv.textContent = 'Введите email';
        messageDiv.style.display = 'block';
        return;
    }
    
    // Формируем данные для отправки
    const userData = {};
    
    if (email) {
        userData.mail = email;
    }
    
    if (role) {
        userData.role = role;
    }
    
    // Если указан новый пароль - проверяем и добавляем
    if (password && password.length > 0) {
        if (password.length < 6) {
            messageDiv.textContent = 'Пароль должен быть не менее 6 символов';
            messageDiv.style.display = 'block';
            return;
        }
        userData.password = password;
    }
    
    // Отправляем запрос на обновление
    const saveBtn = document.getElementById('save-btn');
    saveBtn.disabled = true;
    saveBtn.textContent = 'Сохранение...';
    
    const success = await updateUser(userId, userData);
    
    saveBtn.disabled = false;
    saveBtn.textContent = 'Сохранить изменения';
    
    if (success) {
        messageDiv.className = 'success';
        messageDiv.style.background = '#27ae60';
        messageDiv.style.color = 'white';
        messageDiv.style.padding = '12px';
        messageDiv.style.borderRadius = '8px';
        messageDiv.style.marginTop = '20px';
        messageDiv.textContent = 'Пользователь обновлен';
        messageDiv.style.display = 'block';
        
        // Возвращаемся на админ-панель
        setTimeout(function() {
            window.location.href = 'dashboard.html';
        }, 1500);
    }
}

// Удаление пользователя
async function deleteUserHandler() {
    if (!confirm('Удалить пользователя?')) {
        return;
    }
    
    const deleteBtn = document.getElementById('delete-btn');
    deleteBtn.disabled = true;
    deleteBtn.textContent = 'Удаление...';
    
    const success = await deleteUser(userId);
    
    if (success) {
        setTimeout(function() {
            window.location.href = 'dashboard.html';
        }, 1500);
    } else {
        deleteBtn.disabled = false;
        deleteBtn.textContent = 'Удалить пользователя';
    }
}