const l_btn = document.getElementById('auth-link');
const t_btn = document.getElementById('t-btn');
const r_btn = document.getElementById('r-btn');
const d_btn = document.getElementById('d-btn');
const dep_btn = document.getElementById('dep-btn');

if (authEmail && authPassword) {
    apiGet(`/Database/users/${authEmail}/role`).then((result) => {
        if (result.success && result.data == 'operator') {
            if (d_btn) d_btn.style.display = 'block';

        }
    });

    if (l_btn) {
        l_btn.innerHTML = 'Выход';
        l_btn.onclick = function (e) {
            if (e) e.preventDefault();
            clearAuth();
            window.location.href = '/index.html'; 
        };
        l_btn.href = '#';
    }

} else {
    if (d_btn && d_btn.parentElement) {
    d_btn.parentElement.style.display = 'none'; 
}
    
    if (l_btn) {
        l_btn.innerHTML = 'Вход';
        l_btn.href = '/pages/login.html'; 
    }
}