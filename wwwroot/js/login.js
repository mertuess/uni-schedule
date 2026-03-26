async function login() {
    const email = document.getElementById('email').value;
    const password = document.getElementById('password').value;
    const errorDiv = document.getElementById('errorMessage');

    if (!email || !password) {
        alert('Заполните все поля');
        return;
    }

    try {
        const response = await fetch('/api/login', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ mail: email, password: password })
        });

        if (response.ok) {
            const user = await response.json();
            // Сохраняем данные пользователя
            localStorage.setItem('user', JSON.stringify(user));
            
            // Перенаправляем на главную
            window.location.href = '/index.html';
        } else if (response.status === 401) {
            errorDiv.style.display = 'block';
        } else {
            alert('Ошибка сервера');
        }
    } catch (error) {
        console.error('Ошибка:', error);
        alert('Ошибка соединения');
    }
}

// Проверяем, может уже залогинен
document.addEventListener('DOMContentLoaded', function() {
    const user = localStorage.getItem('user');
    if (user) {
        console.log('Вы уже вошли как:', JSON.parse(user));
    }
});