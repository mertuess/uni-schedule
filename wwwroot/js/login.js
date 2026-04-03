const err = document.getElementById('error-message');
const t_btn = document.getElementById('t-btn');
const r_btn = document.getElementById('r-btn');
const d_btn = document.getElementById('d-btn');

if (authEmail == "") {
  t_btn.style["display"] = "none";
  r_btn.style["display"] = "none";
  d_btn.style["display"] = "none";
}

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
