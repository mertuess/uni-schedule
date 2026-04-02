const dep = document.getElementById('department');
const mail = document.getElementById('mail');
const pass = document.getElementById('password');
const name = document.getElementById('name');
const engName = document.getElementById('engName');
const role = document.getElementById('role');

getDepartments().then(function(result) {
    if (result.success) {
        let arr = result.data;
        for (let i = 0; i < arr.length; i++) {
            const d = arr[i];
            let s = document.createElement('option');
            s.innerHTML = d.Name;
            s.value = d.Id;
            dep.appendChild(s);
        }
    }
});

function submit() {
    let new_dep = null;
    
    if (dep.value != "null" && dep.value != "") {
        new_dep = parseInt(dep.value);
    }
    
    createUser(email.value, pass.value, name.value, engName.value, role.value).then(function(result) {
        if (result.success) {
            window.location.href = './dashboard.html';
        } else {
            const messageBox = document.getElementById('messageBox');
            messageBox.textContent = result.error || 'Ошибка при создании пользователя';
            messageBox.className = 'message-box error';
            messageBox.style.display = 'block';
        }
    });
}