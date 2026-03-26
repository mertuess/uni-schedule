// ============================================
// ФАЙЛ АВТОРИЗАЦИИ
// Отвечает за вход пользователя в систему
// ============================================

// Функция входа в систему
async function login() {
    const email = document.getElementById('email').value;
    const password = document.getElementById('password').value;
    const errorDiv = document.getElementById('errorMessage');

    // Проверка заполнения полей
    if (!email || !password) {
        alert('Заполните все поля');
        return;
    }

    try {
        // Отправка запроса на сервер для проверки учетных данных
        const response = await fetch('/api/login', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ mail: email, password: password })
        });

        if (response.ok) {
            // Успешный вход - сохраняем данные пользователя
            const user = await response.json();
            localStorage.setItem('user', JSON.stringify(user));
            
            // Определяем текст роли для уведомления
            let roleText = '';
            if (user.role === 'operator') roleText = ' (Оператор)';
            if (user.role === 'teacher') roleText = ' (Преподаватель)';
            
            // Показываем приветствие
            showNotification('Добро пожаловать, ' + user.mail + roleText + '!', 'success');
            
            // Перенаправляем на главную страницу
            setTimeout(function() {
                window.location.href = '/index.html';
            }, 1500);
        } else if (response.status === 401) {
            // Неверные учетные данные
            errorDiv.style.display = 'block';
            showNotification('Неверный email или пароль', 'error');
        } else {
            showNotification('Ошибка сервера', 'error');
        }
    } catch (error) {
        showNotification('Ошибка соединения', 'error');
    }
}

// Функция показа уведомлений (используется из main.js)
function showNotification(message, type) {
    const notification = document.createElement('div');
    notification.className = 'notification ' + type;
    notification.textContent = message;
    
    document.body.appendChild(notification);
    
    setTimeout(function() {
        notification.style.animation = 'slideOut 0.3s ease';
        setTimeout(function() {
            notification.remove();
        }, 300);
    }, 3000);
}

// Если пользователь уже залогинен - перенаправляем на главную
document.addEventListener('DOMContentLoaded', function() {
    const user = localStorage.getItem('user');
    if (user) {
        window.location.href = '/index.html';
    }
});