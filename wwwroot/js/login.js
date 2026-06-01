const err = document.getElementById('error-message');

// Функция установки учетных данных
function setAuth(email, password) {
    window.authEmail = email;
    window.authPassword = password;
    setCookie("Uni-Email", email, 90);
    setCookie("Uni-Password", password, 90);
}

async function login() {
    var email = document.getElementById('email').value;
    var pass = document.getElementById('password').value;
    if (email == "" || pass == "") {
        err.style.display = "block";
        err.innerHTML = "Введите все данные";
        return;
    }
    setAuth(email, pass);
    const response = await fetch(`/api/Database/tryauth?email=${encodeURIComponent(email)}&password=${encodeURIComponent(pass)}`);
    const text = await response.text();
    err.style.display = "block";
    err.innerHTML = text;
    if (text == "Успешно") window.location.href = '../index.html';
}