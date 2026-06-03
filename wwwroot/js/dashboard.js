// ============================================================================
// ГЛОБАЛЬНЫЕ ПЕРЕМЕННЫЕ
// ============================================================================
let filter = ['Id', 'Mail'];
let allTeachers = [];
let allDepartments = [];
let selectedTeacher = null;
let currentDeptId = null;
let currentDeptName = null;
let searchTimeout = null;  // ← Для дебаунса поиска

// ============================================================================
// ЗАГРУЗКА ДАННЫХ (вызывается ОДИН раз при старте)
// ============================================================================
async function loadAllData() {
    if (allTeachers.length > 0) return;  // ← Защита от повторной загрузки
    
    try {
        const teachersResponse = await apiGet('/Database/teachers/all');
        if (teachersResponse.success && Array.isArray(teachersResponse.data)) {
            allTeachers = teachersResponse.data;
        }
        const deptsResponse = await getDepartments();
        if (deptsResponse.success && Array.isArray(deptsResponse.data)) {
            allDepartments = deptsResponse.data;
        }
    } catch (e) { console.error('Ошибка загрузки данных:', e); }
}

// ============================================================================
// ОТОБРАЖЕНИЕ ПОЛЬЗОВАТЕЛЕЙ
// ============================================================================
getUsers().then((result) => {
    if (!result.success || !result.data) return;
    
    let arr = result.data;
    let count = document.getElementById('user-count');
    let table = document.getElementById('users-list');
    if (!table) return;
    
    count.innerHTML = arr.length;
    for (let i = 0; i < arr.length; i++) {
        const u = arr[i];
        const tr = document.createElement('tr');
        for (let atr in u) { if (!filter.includes(atr)) continue; const td = document.createElement('td'); td.textContent = u[atr]; tr.appendChild(td); }
        const tdActions = document.createElement('td');
        tdActions.className = 'action-buttons';
        tdActions.innerHTML = `<button onclick="editUser(${u.Id})" class="btn btn-warning" style="padding:6px 12px">Изменить</button><button onclick="deleteUserById('${u.Mail}')" class="btn btn-danger" style="padding:6px 12px">Удалить</button>`;
        tr.appendChild(tdActions); table.appendChild(tr);
    }
});

// ============================================================================
// ОТОБРАЖЕНИЕ КАФЕДР
// ============================================================================
getDepartments().then((result) => {
    if (!result.success || !result.data) return;
    
    let arr = result.data;
    let table = document.getElementById('departments-list');
    if (!table) return;
    table.innerHTML = '';
    for (let i = 0; i < arr.length; i++) {
        const d = arr[i];
        const tr = document.createElement('tr');
        const tdId = document.createElement('td'); tdId.textContent = d.Id; tr.appendChild(tdId);
        const tdName = document.createElement('td'); tdName.textContent = d.Name; tr.appendChild(tdName);
        const tdActions = document.createElement('td');
        tdActions.className = 'action-buttons';
        tdActions.innerHTML = `<button onclick="editDepartment(${d.Id}, '${d.Name}')" class="btn btn-warning" style="padding:6px 12px">Изменить</button><button onclick="deleteDepartmentById('${d.Name}')" class="btn btn-danger" style="padding:6px 12px">Удалить</button>`;
        tr.appendChild(tdActions); table.appendChild(tr);
    }
});

// ============================================================================
// ФУНКЦИИ УПРАВЛЕНИЯ
// ============================================================================
function editUser(id) { localStorage.setItem("user-to-edit", id); window.location.href = './user_edit.html'; }
function deleteUserById(email) { if (!confirm('Удалить пользователя ' + email + '?')) return; deleteUser(email).then(() => { alert('Пользователь удален'); window.location.reload(); }); }
function editDepartment(id, name) { currentDeptId = id; currentDeptName = name; document.getElementById('dept-modal-title').innerHTML = 'Редактировать кафедру'; document.getElementById('dept-name').value = name; document.getElementById('dept-modal').style.display = 'flex'; }
function deleteDepartmentById(name) { if (!confirm('Удалить кафедру "' + name + '"?')) return; deleteDepartment(name).then(() => { alert('Кафедра удалена'); window.location.reload(); }); }
function saveDepartment() { 
    
    const deptName = document.getElementById('dept-name').value.trim(); 
    
    if (!deptName) { 
        alert('Введите название кафедры'); 
        return; 
    } 
    
    if (currentDeptName) { 
        updateDepartment(currentDeptName, deptName).then(() => { 
            alert('Кафедра обновлена'); 
            closeDeptModal(); 
            window.location.reload(); 
        }); 
    } else { 
        createDepartment(deptName).then((result) => { 
            alert('Кафедра создана'); 
            closeDeptModal(); 
            window.location.reload(); 
        }); 
    } 
}
function closeDeptModal() { document.getElementById('dept-modal').style.display = 'none'; document.getElementById('dept-name').value = ''; currentDeptId = null; currentDeptName = null; document.getElementById('dept-modal-title').innerHTML = 'Создать кафедру'; }

document.getElementById('createDeptBtn').onclick = function () { currentDeptId = null; currentDeptName = null; document.getElementById('dept-modal-title').innerHTML = 'Создать кафедру'; document.getElementById('dept-name').value = ''; document.getElementById('dept-modal').style.display = 'flex'; };
document.getElementById('saveDeptBtn').onclick = saveDepartment;
document.getElementById('closeDeptModal').onclick = closeDeptModal;

// ============================================================================
// ПОИСК ПРЕПОДАВАТЕЛЕЙ (с дебаунсом 300мс)
// ============================================================================
async function searchTeachers() {
    if (searchTimeout) clearTimeout(searchTimeout);
    
    searchTimeout = setTimeout(async function() {
        const query = document.getElementById('teacher-search').value.trim();
        const resultsDiv = document.getElementById('teacher-results');
        
        if (query.length < 2) { resultsDiv.style.display = 'none'; return; }
        
        // Загружаем данные только если ещё не загружены
        if (allTeachers.length === 0) { 
            resultsDiv.innerHTML = '<div style="padding:10px;color:#666">Загрузка...</div>'; 
            resultsDiv.style.display = 'block'; 
            await loadAllData(); 
        }
        
        // Фильтрация на клиенте (мгновенно)
        const filtered = allTeachers.filter(function(t) {
            const name = (t.name || t.teacher || '').toLowerCase();
            return name.includes(query.toLowerCase());
        });
        
        resultsDiv.innerHTML = '';
        if (filtered.length === 0) {
            resultsDiv.innerHTML = '<div style="padding:10px;color:#666">Не найдено</div>';
        } else {
            // ← ИСПРАВЛЕНО: bindings теперь хранит МАССИВ кафедр для каждого UID
            const bindings = {};
            
            for (const dept of allDepartments) {
                try {
                    const resp = await apiGet('/Database/departments/' + dept.Id + '/teachers');
                    if (resp.success && Array.isArray(resp.data)) {
                        resp.data.forEach(function(t) {
                            const uid = t.uid;
                            if (!bindings[uid]) {
                                bindings[uid] = []; 
                            }
                            bindings[uid].push(dept.Name); 
                        });
                    }
                } catch (e) {}
            }

            filtered.slice(0, 30).forEach(function(t) {
                const uid = t.uid || t.UID;
                const name = t.name || t.teacher;
                const deptList = bindings[uid];
                
                 const deptText = deptList && deptList.length > 0 
                    ? `<small style="color:#0066cc">[кафедра: ${deptList.join(', ')}]</small>` 
                    : '';
                
                const item = document.createElement('div');
                item.className = 'teacher-item';
                item.style.padding = '10px';
                item.style.cursor = 'pointer';
                item.style.borderBottom = '1px solid #eee';
                item.style.background = deptList && deptList.length > 0 ? '#f8f9fa' : 'white';
                
                item.innerHTML = `<strong>${name}</strong> ${deptText}<br><small style="color:#666">${t.faculty || ''}</small>`;
                
                item.onclick = () => selectTeacher({ uid, name, faculty: t.faculty, rebind: !!(deptList && deptList.length > 0) });
                item.onmouseenter = () => item.style.background = '#f0f0f0';
                item.onmouseleave = () => item.style.background = (deptList && deptList.length > 0) ? '#f8f9fa' : 'white';
                
                resultsDiv.appendChild(item);
            });
        }
        resultsDiv.style.display = 'block';
    }, 300);
}

// ============================================================================
// ВЫБОР ПРЕПОДАВАТЕЛЯ
// ============================================================================
function selectTeacher(t) {
    selectedTeacher = t;
    document.getElementById('selected-name').textContent = t.name;
    document.getElementById('selected-teacher').style.display = 'block';
    document.getElementById('teacher-results').style.display = 'none';
    document.getElementById('teacher-search').value = '';
    
    const select = document.getElementById('bind-dept');
    select.innerHTML = '<option value="">-- Не привязан --</option>';
    allDepartments.forEach(function(d) {
        const opt = document.createElement('option');
        opt.value = d.Id;
        opt.textContent = d.Name;
        select.appendChild(opt);
    });
    
    document.getElementById('department-select-block').style.display = 'block';
    document.getElementById('btn-bind').disabled = false;
    document.getElementById('btn-bind').textContent = 'Привязать к кафедре';
    
    loadTeacherBindingsDisplay(t.uid);

    const msg = document.getElementById('bind-msg');
    if (t.rebind) {
        msg.innerHTML = '<span style="color:#0066cc">Режим изменения привязки. Выберите новую кафедру и нажмите "Привязать".</span>';
    } else {
        msg.innerHTML = '';
    }
}

function clearSelection() {
    selectedTeacher = null;
    document.getElementById('selected-teacher').style.display = 'none';
    document.getElementById('department-select-block').style.display = 'none';
    document.getElementById('btn-bind').disabled = true;
    document.getElementById('bind-msg').innerHTML = '';
}

async function loadTeacherBindingsDisplay(uid) {
    const bindingsContainer = document.getElementById('current-bindings-list');
    if (!bindingsContainer) return; 
  
    
    const allDepts = allDepartments; 
    let html = '<div style="margin-top:10px; font-size:13px; color:#666;">Уже привязан к:</div><div style="display:flex; flex-wrap:wrap; gap:5px; margin-top:5px;">';
    
    let hasBindings = false;

    for (const dept of allDepts) {
        try {
            const resp = await apiGet('/Database/departments/' + dept.Id + '/teachers');
            if (resp.success && Array.isArray(resp.data)) {
                const isBound = resp.data.some(t => t.uid === uid);
                if (isBound) {
                    hasBindings = true;
                    html += `<span class="binding-tag" style="background:#e3f2fd; color:#0066cc; padding:2px 8px; border-radius:12px; font-size:12px; display:inline-flex; align-items:center; gap:4px;">
                        ${dept.Name} 
                        <span style="cursor:pointer; color:#c81414; font-weight:bold;" onclick="unbindTeacher('${uid}', ${dept.Id})">×</span>
                    </span>`;
                }
            }
        } catch(e) {}
    }
    
    html += '</div>';
    
    let container = document.getElementById('current-bindings-list');
    if (!container) {
        container = document.createElement('div');
        container.id = 'current-bindings-list';
        document.getElementById('department-select-block').after(container);
    }
    container.innerHTML = hasBindings ? html : '<div style="margin-top:10px; font-size:13px; color:#999;">Нет активных привязок</div>';
}

// ============================================================================
// ПРИВЯЗКА ПРЕПОДАВАТЕЛЯ
// ============================================================================
async function bindTeacher() {
    if (!selectedTeacher || !selectedTeacher.uid) { 
        document.getElementById('bind-msg').innerHTML = '<span style="color:#dc3545">Выберите преподавателя</span>'; 
        return; 
    }
    
    const deptId = document.getElementById('bind-dept').value;
    const departmentId = deptId ? parseInt(deptId) : null;
    const msg = document.getElementById('bind-msg');
    msg.innerHTML = 'Сохранение...';
    
    try {
        const response = await fetch('/api/Database/teachers/' + encodeURIComponent(selectedTeacher.uid) + '/bind', {
            method: 'POST',
            headers: getHeaders(),
            body: JSON.stringify({ name: selectedTeacher.name, departmentId: departmentId })
        });
        
        const result = await response.json();
        
        if (response.ok && result.success) { 
            msg.innerHTML = '<span style="color:#28a745">✓ Привязано!</span>'; 
            setTimeout(clearSelection, 1500); 
        } else { 
            msg.innerHTML = '<span style="color:#dc3545">Ошибка: ' + (result.error || 'Не удалось') + '</span>'; 
        }
    } catch (e) { 
        msg.innerHTML = '<span style="color:#dc3545">Ошибка: ' + e.message + '</span>'; 
    }
}

async function unbindTeacher(uid, deptId) {
    const dept = allDepartments.find(d => d.Id === deptId);
    const deptName = dept ? dept.Name : 'этой кафедре';
    
    if (!confirm(`Убрать привязку к кафедре "${deptName}"?`)) return;
    
    try {
        const response = await fetch(`/api/Database/teachers/${encodeURIComponent(uid)}/unbind?departmentId=${deptId}`, {
            method: 'DELETE',
            headers: getHeaders()
        });
        
        const result = await response.json();
        
        if (response.ok && result.success) {
            if (selectedTeacher) {
                await loadTeacherBindingsDisplay(selectedTeacher.uid);
            }
            await loadAllData();
        } else {
            alert('Не удалось удалить привязку: ' + (result.error || 'Неизвестная ошибка'));
        }
    } catch (e) {
        console.error('Ошибка удаления привязки:', e);
        alert('Ошибка: ' + e.message);
    }
}



// ============================================================================
// ИНИЦИАЛИЗАЦИЯ
// ============================================================================
document.addEventListener('click', function(e) {
    const results = document.getElementById('teacher-results');
    const search = document.getElementById('teacher-search');
    if (results && !results.contains(e.target) && e.target !== search) results.style.display = 'none';
});

document.addEventListener('DOMContentLoaded', function() { 
    loadAllData();  // ← Вызывается только один раз
});