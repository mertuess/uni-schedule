const email = document.getElementById('email');
const pass = document.getElementById('password');
const msgBox = document.getElementById('messageBox');

function submit() {
    const emailValue = email ? email.value.trim() : '';
    const passwordValue = pass ? pass.value.trim() : '';

    if (!emailValue || !passwordValue) {
        if (msgBox) {
            msgBox.textContent = 'Заполните Email и Пароль';
            msgBox.className = 'message-box error';
            msgBox.style.display = 'block';
        }
        return;
    }

    if (msgBox) {
        msgBox.style.display = 'none';
        msgBox.textContent = '';
    }

    createUser(emailValue, passwordValue, 'Оператор', 'Operator', 'operator')
        .then(function(result) {
            if (result.success) {
                alert('Оператор успешно создан');
                window.location.href = './dashboard.html';
            } else {
                if (msgBox) {
                    msgBox.textContent = result.error || 'Ошибка при создании';
                    msgBox.className = 'message-box error';
                    msgBox.style.display = 'block';
                }
            }
        })
        .catch(function(error) {
            if (msgBox) {
                msgBox.textContent = 'Ошибка сети: ' + error.message;
                msgBox.className = 'message-box error';
                msgBox.style.display = 'block';
            }
        });
}