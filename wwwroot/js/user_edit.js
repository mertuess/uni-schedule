// Редактирование пользователя

let currentEmail = '';

// Получаем email пользователя из URL
function getEmailFromUrl() {
    const params = new URLSearchParams(window.location.search);
    return params.get('email');
}

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

// Загружаем данные пользователя
async function loadUserData() {
    currentEmail = getEmailFromUrl();
    
    if (!currentEmail) {
        window.location.href = 'dashboard.html';
        return;
    }
    
    const result = await getUsers();
    
    if (result.success) {
        const user = result.data.find(u => u.Mail === currentEmail);
        
        if (user) {
            document.getElementById('currentMail').value = user.Mail;
        } else {
            showMessage('Пользователь не найден', 'error');
            setTimeout(() => {
                window.location.href = 'dashboard.html';
            }, 2000);
        }
    } else {
        showMessage('Ошибка загрузки данных', 'error');
    }
}

// Обработка обновления пользователя
async function handleUpdateUser(event) {
    event.preventDefault();
    
    const newEmail = document.getElementById('newMail').value.trim();
    const newPassword = document.getElementById('newPassword').value.trim();
    const newName = document.getElementById('newName').value.trim();
    const newEngName = document.getElementById('newEngName').value.trim();
    const newRole = document.getElementById('newRole').value;
    const department = document.getElementById('department').value.trim();
    
    const messageBox = document.getElementById('messageBox');
    
    // Проверяем, есть ли что обновлять
    if (!newEmail && !newPassword && !newName && !newEngName && !newRole && !department) {
        messageBox.textContent = 'Нет данных для обновления';
        messageBox.className = 'message-box warning';
        return;
    }
    
    // Обновляем пользователя
    const result = await updateUser(currentEmail, newEmail, newPassword, newName, newEngName, newRole, department);
    
    if (result.success) {
        messageBox.textContent = 'Пользователь успешно обновлен!';
        messageBox.className = 'message-box success';
        
        setTimeout(() => {
            window.location.href = 'dashboard.html';
        }, 2000);
    } else {
        messageBox.textContent = 'Ошибка обновления: ' + result.error;
        messageBox.className = 'message-box error';
    }
}

// Показываем сообщение
function showMessage(text, type) {
    const messageBox = document.getElementById('messageBox');
    messageBox.textContent = text;
    messageBox.className = `message-box ${type}`;
}

// Инициализация
(async function init() {
    const hasAccess = await checkAccess();
    if (hasAccess) {
        await loadUserData();
        
        const editForm = document.getElementById('editUserForm');
        if (editForm) {
            editForm.addEventListener('submit', handleUpdateUser);
        }
    }
})();