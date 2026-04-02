let filter = [
    'Id',
    'Mail',
    'Name',
    'Role'
]

getUsers().then(function(result) {
    let arr = result.data;
    let count = document.getElementById('user-count');
    let table = document.getElementById('users-list');
    count.innerHTML = arr.length;

    for (let i = 0; i < arr.length; i++) {
        const u = arr[i];
        const tr = document.createElement('tr');

        for (let atr in u) {
            if (!filter.includes(atr)) continue;
            const td = document.createElement('td');
            td.textContent = u[atr]
            tr.appendChild(td);
        }

        const tdActions = document.createElement('td');
        tdActions.className = 'action-buttons';
        tdActions.innerHTML = `
            <button onclick="editUser(${u.Id})" class="btn btn-warning" style="padding: 6px 12px;">Изменить</button>
            <button onclick="deleteUserById('${u.Mail}')" class="btn btn-danger" style="padding: 6px 12px;">Удалить</button>
        `;

        tr.appendChild(tdActions);
        table.appendChild(tr);
    }
});

getDepartments().then(function(result) {});

function editUser(id) {
    localStorage.setItem("user-to-edit", id);
    window.location.href = './user_edit.html';
}

function deleteUserById(email) {
    if (!confirm(`Удалить пользователя ${email}?`)) return;
    
    deleteUser(email).then(function(result) {
        alert('Пользователь удален. Обновите страницу для обновления списка');
    });
}

function updateUserById(id) {

}