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
        err.style["display"] = "block";
        err.innerHTML = "Введите все данные";
        return;
    }
    setAuth(email, pass);
    const response = await fetch(`${API_BASE}/Database/tryauth?email=${email}&password=${pass}`, {
        method: 'GET'
    });
    response.text().then(function (text) {
        err.style["display"] = "block";
        err.innerHTML = text;
        if (text == "Успешно")
            window.location.href = '../index.html';
    });
}

function saveToken() {
    var token = document.getElementById('o-api-token').value;
    setCookie('o-api-token', token);
    alert('Токен сохранен');
}
