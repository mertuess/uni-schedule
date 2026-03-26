// ============================================
// ФАЙЛ АДМИН-ПАНЕЛИ
// Отвечает за отображение списка пользователей
// ============================================

// Загрузка и отображение списка пользователей
async function loadUsers() {
    const tbody = document.getElementById('users-tbody');
    const countSpan = document.getElementById('user-count');
    const timeSpan = document.getElementById('update-time');
    
    if (!tbody) return;
    
    // Показываем индикатор загрузки
    tbody.innerHTML = '<td colspan="4" style="text-align:center;">Загрузка...<\/td><\/tr>';
    
    // Получаем список пользователей из API
    const users = await getUsers();
    
    tbody.innerHTML = '';
    
    if (users.length === 0) {
        tbody.innerHTML = '<td colspan="4" style="text-align:center;">Нет пользователей<\/td><\/tr>';
    } else {
        // Заполняем таблицу пользователями
        for (let i = 0; i < users.length; i++) {
            const user = users[i];
            const tr = document.createElement('tr');
            
            // ID пользователя
            const tdId = document.createElement('td');
            tdId.textContent = user.id;
            tr.appendChild(tdId);
            
            // Email
            const tdEmail = document.createElement('td');
            tdEmail.textContent = user.mail;
            tr.appendChild(tdEmail);
            
            // Роль
            const tdRole = document.createElement('td');
            let roleText = 'Пользователь';
            if (user.role === 'operator') roleText = 'Оператор';
            if (user.role === 'teacher') roleText = 'Преподаватель';
            tdRole.textContent = roleText;
            tr.appendChild(tdRole);
            
            // Действия (редактировать/удалить)
            const tdActions = document.createElement('td');
            tdActions.className = 'action-buttons';
            
            if (user.role !== 'operator') {
                tdActions.innerHTML = `
                    <button onclick="editUser(${user.id})" class="btn btn-warning" style="padding: 6px 12px;">Редакт</button>
                    <button onclick="deleteUserById(${user.id})" class="btn btn-danger" style="padding: 6px 12px;">Удалить</button>
                `;
            } else {
                tdActions.innerHTML = '<span style="color:gray;">Нельзя редактировать</span>';
            }
            
            tr.appendChild(tdActions);
            tbody.appendChild(tr);
        }
    }
    
    // Обновляем счетчик пользователей
    if (countSpan) countSpan.textContent = users.length;
    
    // Обновляем время последнего обновления
    if (timeSpan) {
        const now = new Date();
        const formattedDate = now.getDate().toString().padStart(2, '0') + '.' + 
            (now.getMonth()+1).toString().padStart(2, '0') + '.' + 
            now.getFullYear() + ' ' + 
            now.getHours().toString().padStart(2, '0') + ':' + 
            now.getMinutes().toString().padStart(2, '0');
        timeSpan.textContent = formattedDate;
    }
}

// Переход на страницу редактирования пользователя
function editUser(id) {
    window.location.href = 'user_action.html?id=' + id;
}

// Удаление пользователя
async function deleteUserById(id) {
    if (!confirm('Удалить пользователя?')) {
        return;
    }
    
    const success = await deleteUser(id);
    if (success) {
        // Обновляем список после удаления
        loadUsers();
    }
}

// Инициализация админ-панели
document.addEventListener('DOMContentLoaded', function() {
    if (checkOperator()) {
        loadUsers();
        updateUserHeader();
        updateNavigation();
    }
});