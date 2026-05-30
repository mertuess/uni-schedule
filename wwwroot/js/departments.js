const depSelect = document.getElementById('department');

// Загрузка кафедр при старте страницы
async function loadDepartments() {
    try {
        const response = await getDepartments();
        if (response.success && Array.isArray(response.data) && depSelect) {
            depSelect.innerHTML = '<option value="-1">Выберите кафедру</option>';
            response.data.forEach(function(d) {
                const opt = document.createElement('option');
                opt.value = d.Id;
                opt.textContent = d.Name;
                depSelect.appendChild(opt);
            });
        }
    } catch (e) {
        console.error('Ошибка загрузки кафедр:', e);
    }
}

// Загрузка преподавателей, привязанных к выбранной кафедре
async function loadTeachersByDepartment(departmentId) {
    if (!departmentId || departmentId === '-1') return [];
    
    try {
        // Используем НОВЫЙ эндпоинт для привязок преподавателей
        const response = await apiGet('/Database/departments/' + departmentId + '/teachers');
        if (response.success && Array.isArray(response.data)) {
            return response.data.map(function(t) {
                // Возвращаем формат, совместимый с teachers.js
                return {
                    UID: t.uid,
                    teacher: t.name,
                    teacher_id: t.uid,
                    departmentId: t.departmentId
                };
            });
        }
    } catch (e) {
        console.error('Ошибка загрузки преподавателей кафедры:', e);
    }
    return [];
}

// Добавить всех преподавателей кафедры в выбранные (использует переменные из teachers.js)
async function getAllTeachers() {
    // Очищаем текущий список выбранных преподавателей (переменная из teachers.js)
    if (typeof selected_teachers !== 'undefined') {
        selected_teachers = [];
    }
    
    const departmentId = depSelect ? depSelect.value : null;
    if (!departmentId || departmentId === '-1') return;
    
    const teachers = await loadTeachersByDepartment(departmentId);
    
    // Добавляем в глобальный список selected_teachers (из teachers.js)
    if (typeof selected_teachers !== 'undefined' && Array.isArray(selected_teachers)) {
        teachers.forEach(function(teacher) {
            // Проверяем, нет ли уже такого преподавателя
            if (!selected_teachers.some(function(t) { return t.UID === teacher.UID; })) {
                selected_teachers.push(teacher);
            }
        });
    }
    
    // Обновляем отображение выбранных преподавателей (функция из teachers.js)
    if (typeof updateSelected === 'function') {
        updateSelected();
    }
}

// Инициализация при загрузке страницы
document.addEventListener('DOMContentLoaded', function() {
    loadDepartments();
    
    // Обработчик изменения выбранной кафедры
    if (depSelect) {
        depSelect.addEventListener('change', async function() {
            await getAllTeachers();
        });
    }
});