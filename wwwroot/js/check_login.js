const l_btn = document.getElementById('auth-link');
const t_btn = document.getElementById('t-btn');
const r_btn = document.getElementById('r-btn');
const d_btn = document.getElementById('d-btn');
const dep_btn = document.getElementById('dep-btn');

if (authEmail && authPassword) {
    apiGet(`/Database/users/${authEmail}/role`).then((result) => {
        if (result.success && result.data == 'operator') {
            if (d_btn) d_btn.style.display = 'block';
            if (dep_btn) dep_btn.style.display = 'block';
            } else{
            if (d_btn) d_btn.style.display = 'none';
            if (dep_btn) dep_btn.style.display = 'none';
            if (t_btn) t_btn.style.display = 'none';
            if (r_btn) r_btn.style.display = 'none';
        }
    });
} else {
    if (d_btn) d_btn.style.display = "none";
    if (dep_btn) dep_btn.style.display = 'none';
    if (t_btn) t_btn.style.display = 'none';
    if (r_btn) r_btn.style.display = 'none';
}

if (l_btn) {
    if (authEmail && authPassword) {
        l_btn.innerHTML = 'Выход';
        l_btn.onclick = function(e) {
            if (e) e.preventDefault();
            clearAuth();
            window.location.href = '';
        };
        l_btn.href = '#';
    } else {
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