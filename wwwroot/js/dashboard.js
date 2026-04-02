let filter = [
    'Id',
    'Mail',
    'Name',
    'Role'
]
let currentDeptId = null;
let currentDeptName = null;

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

getDepartments().then(function(result) {
    let arr = result.data;
    let table = document.getElementById('departments-list');
    if (!table) return;
    
    table.innerHTML = '';

    for (let i = 0; i < arr.length; i++) {
        const d = arr[i];
        const tr = document.createElement('tr');
        
        const tdId = document.createElement('td');
        tdId.textContent = d.Id;
        tr.appendChild(tdId);
        
        const tdName = document.createElement('td');
        tdName.textContent = d.Name;
        tr.appendChild(tdName);
        
        const tdActions = document.createElement('td');
        tdActions.className = 'action-buttons';
        tdActions.innerHTML = `
            <button onclick="editDepartment(${d.Id}, '${d.Name}')" class="btn btn-warning" style="padding: 6px 12px;">Изменить</button>
            <button onclick="deleteDepartmentById(${d.Id}, '${d.Name}')" class="btn btn-danger" style="padding: 6px 12px;">Удалить</button>
        `;
        tr.appendChild(tdActions);
        table.appendChild(tr);
    }
});

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

function editDepartment(id, name) {
    currentDeptId = id;
    currentDeptName = name;
    document.getElementById('dept-modal-title').innerHTML = 'Редактировать кафедру';
    document.getElementById('dept-name').value = name;
    document.getElementById('dept-modal').style.display = 'flex';
}

function deleteDepartmentById(id, name) {
    if (!confirm(`Удалить кафедру "${name}"?`)) return;
    
    deleteDepartment(name).then(function(result) {
        alert('Кафедра удалена. Обновите страницу');
        window.location.reload();
    });
}

function saveDepartment() {
    const deptName = document.getElementById('dept-name').value.trim();
    
    if (!deptName) {
        alert('Введите название кафедры');
        return;
    }
    
    if (currentDeptName) {
        updateDepartment(currentDeptName, deptName).then(function(result) {
            alert('Кафедра обновлена. Обновите страницу');
            closeDeptModal();
            window.location.reload();
        });
    } else {
        createDepartment(deptName).then(function(result) {
            alert('Кафедра создана. Обновите страницу');
            closeDeptModal();
            window.location.reload();
        });
    }
}

function closeDeptModal() {
    document.getElementById('dept-modal').style.display = 'none';
    document.getElementById('dept-name').value = '';
    currentDeptId = null;
    currentDeptName = null;
    document.getElementById('dept-modal-title').innerHTML = 'Создать кафедру';
}

document.getElementById('createDeptBtn').onclick = function() {
    currentDeptId = null;
    currentDeptName = null;
    document.getElementById('dept-modal-title').innerHTML = 'Создать кафедру';
    document.getElementById('dept-name').value = '';
    document.getElementById('dept-modal').style.display = 'flex';
}

document.getElementById('saveDeptBtn').onclick = saveDepartment;
document.getElementById('closeDeptModal').onclick = closeDeptModal;