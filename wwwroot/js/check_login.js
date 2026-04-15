const l_btn = document.getElementById('auth-link');
const t_btn = document.getElementById('t-btn');
const r_btn = document.getElementById('r-btn');
const d_btn = document.getElementById('d-btn');
const dep_btn = document.getElementById('dep-btn');

if (authEmail && authPassword) {
    apiGet(`/Database/users/${authEmail}/role`).then((result) => {
        if (result.success) {
            let role = result.data;
            if (role !== "operator" && d_btn) {
                d_btn.style.display = "none";
            }
        }
    });
} else {
    if (d_btn) d_btn.style.display = "none";
}

if (authEmail && authPassword) {
    if (l_btn) {
        l_btn.innerHTML = 'Выход';
        l_btn.onclick = function (e) {
            e.preventDefault();
            clearAuth();
            window.location.href = '';
        };
        l_btn.href = '';
    }
} else {
    if (l_btn) {
        l_btn.innerHTML = 'Вход';
        l_btn.href = './pages/login.html';
    }
}

if (!authEmail || !authPassword) {
    if (t_btn) t_btn.style.display = "none";
    if (r_btn) r_btn.style.display = "none";
    if (d_btn) d_btn.style.display = "none";
    if (dep_btn) dep_btn.style.display = "none";
}