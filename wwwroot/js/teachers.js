// ============================================
// ФАЙЛ РАБОТЫ С ПРЕПОДАВАТЕЛЯМИ
// Отвечает за поиск преподавателей и отображение их занятости
// ============================================

let selectedTeachers = [];
let teachersList = [];

// Инициализация поиска преподавателей
document.addEventListener('DOMContentLoaded', function() {
    const teacherSearchInput = document.getElementById('teacher-search-input');
    const suggestionsList = document.getElementById('teacher-suggestions');
    const showBtn = document.getElementById('show-teachers-schedule');
    
    if (teacherSearchInput) {
        // Загрузка списка всех преподавателей при первом фокусе
        teacherSearchInput.addEventListener('focus', loadTeachersList);
        
        // Поиск при вводе
        teacherSearchInput.addEventListener('input', function(e) {
            const query = e.target.value.toLowerCase();
            const filtered = teachersList.filter(teacher => 
                teacher.teacher.toLowerCase().includes(query)
            );
            showSuggestions(filtered);
        });
        
        // Закрытие списка при клике вне
        document.addEventListener('click', function(e) {
            if (!teacherSearchInput.contains(e.target) && !suggestionsList.contains(e.target)) {
                suggestionsList.classList.remove('active');
            }
        });
    }
    
    if (showBtn) {
        showBtn.addEventListener('click', loadTeachersSchedule);
    }
});

// Загрузка списка преподавателей с сервера
async function loadTeachersList() {
    if (teachersList.length > 0) return;
    
    try {
        const response = await fetch('/api/teachers');
        if (response.ok) {
            teachersList = await response.json();
            console.log('Загружено преподавателей:', teachersList.length);
        }
    } catch (error) {
        console.error('Ошибка загрузки списка преподавателей:', error);
    }
}

// Показ подсказок
function showSuggestions(teachers) {
    const suggestionsList = document.getElementById('teacher-suggestions');
    suggestionsList.innerHTML = '';
    
    if (teachers.length === 0) {
        suggestionsList.classList.remove('active');
        return;
    }
    
    teachers.forEach(teacher => {
        const div = document.createElement('div');
        div.className = 'suggestion-item';
        div.textContent = teacher.teacher;
        div.onclick = () => addTeacher(teacher);
        suggestionsList.appendChild(div);
    });
    
    suggestionsList.classList.add('active');
}

// Добавление преподавателя в выбранные
function addTeacher(teacher) {
    if (selectedTeachers.some(t => t.teacher_id === teacher.teacher_id)) {
        showNotification('Этот преподаватель уже добавлен', 'warning');
        return;
    }
    
    selectedTeachers.push(teacher);
    updateSelectedTeachersUI();
    
    // Очищаем поле ввода
    const input = document.getElementById('teacher-search-input');
    input.value = '';
    
    // Скрываем подсказки
    document.getElementById('teacher-suggestions').classList.remove('active');
}

// Обновление UI выбранных преподавателей
function updateSelectedTeachersUI() {
    const container = document.getElementById('selected-teachers');
    container.innerHTML = '';
    
    selectedTeachers.forEach(teacher => {
        const tag = document.createElement('div');
        tag.className = 'teacher-tag';
        tag.innerHTML = `
            ${teacher.teacher}
            <span class="remove-teacher" data-id="${teacher.teacher_id}">×</span>
        `;
        container.appendChild(tag);
    });
    
    // Добавляем обработчики удаления
    document.querySelectorAll('.remove-teacher').forEach(btn => {
        btn.addEventListener('click', function() {
            const id = parseInt(this.dataset.id);
            selectedTeachers = selectedTeachers.filter(t => t.teacher_id !== id);
            updateSelectedTeachersUI();
        });
    });
}

// Загрузка расписания для выбранных преподавателей
async function loadTeachersSchedule() {
    if (selectedTeachers.length === 0) {
        showNotification('Выберите хотя бы одного преподавателя', 'warning');
        return;
    }
    
    const weekSelect = document.getElementById('teacher-week');
    const weekValue = weekSelect.value;
    
    const resultsContainer = document.getElementById('results-container');
    const resultsBody = document.getElementById('results-body');
    
    if (!resultsBody) return;
    
    // Показываем индикатор загрузки
    resultsBody.innerHTML = '<tr><td colspan="8" style="text-align:center; padding:40px;">Загрузка расписания...</td></tr>';
    resultsContainer.style.display = 'block';
    
    try {
        // Получаем расписание для каждого преподавателя
        const schedules = [];
        for (const teacher of selectedTeachers) {
            const schedule = await getTeacherSchedule(teacher.teacher_id, weekValue);
            schedules.push({
                teacher: teacher,
                schedule: schedule
            });
        }
        
        // Отображаем расписание
        displayTeachersSchedule(schedules);
        
    } catch (error) {
        console.error('Ошибка загрузки расписания:', error);
        resultsBody.innerHTML = '<tr><td colspan="8" style="text-align:center; padding:40px;">Ошибка загрузки расписания</td></tr>';
        showNotification('Ошибка при загрузке расписания', 'error');
    }
}

// Получение расписания преподавателя
async function getTeacherSchedule(teacherId, week) {
    try {
        const response = await fetch(`/api/teacher/${teacherId}/schedule?week=${week}`);
        if (response.ok) {
            return await response.json();
        }
        return {};
    } catch (error) {
        console.error('Ошибка:', error);
        return {};
    }
}

// Отображение расписания преподавателей в виде таблицы
// Отображение расписания преподавателей в виде таблицы
function displayTeachersSchedule(schedules) {
    const resultsContainer = document.getElementById('results-container');
    const resultsBody = document.getElementById('results-body');
    if (!resultsBody) return;

    // Дни недели (рабочие)
    const days = ['Понедельник', 'Вторник', 'Среда', 'Четверг', 'Пятница', 'Суббота'];
    // Пары (1–6)
    const pairs = [1, 2, 3, 4, 5, 6, 7, 8];

    // Очищаем контейнер
    resultsBody.innerHTML = '';

    // Для каждого дня создаём отдельную таблицу
    days.forEach(day => {
        const dayContainer = document.createElement('div');
        dayContainer.className = 'day-schedule-container';
        dayContainer.style.marginBottom = '30px';

        // Заголовок дня
        const dayTitle = document.createElement('h3');
        dayTitle.textContent = day;
        dayTitle.style.textAlign = 'center';
        dayTitle.style.margin = '20px 0 10px';
        dayContainer.appendChild(dayTitle);

        // Создаём таблицу
        const table = document.createElement('table');
        table.className = 'teacher-schedule-table';

        // Заголовок таблицы: строка с номерами пар
        const thead = document.createElement('thead');
        const headerRow = document.createElement('tr');

        // Первая ячейка - преподаватель
        const thTeacher = document.createElement('th');
        thTeacher.textContent = 'Преподаватель';
        headerRow.appendChild(thTeacher);

        // Ячейки для пар (без времени)
        pairs.forEach(pair => {
            const thPair = document.createElement('th');
            thPair.textContent = `${pair} пара`;
            thPair.style.textAlign = 'center';
            headerRow.appendChild(thPair);
        });

        thead.appendChild(headerRow);
        table.appendChild(thead);

        // Тело таблицы
        const tbody = document.createElement('tbody');

        // Для каждого преподавателя добавляем строку
        schedules.forEach(item => {
            const tr = document.createElement('tr');

            // Ячейка с именем преподавателя
            const tdName = document.createElement('td');
            tdName.textContent = item.teacher.teacher;
            tdName.style.fontWeight = 'bold';
            tr.appendChild(tdName);

            // Для каждой пары в текущем дне
            pairs.forEach(pair => {
                const scheduleKey = `${day}_${pair}`;
                const scheduleItem = item.schedule[scheduleKey];
                const td = document.createElement('td');

                if (scheduleItem && scheduleItem.discipline) {
                    td.innerHTML = `
                        <div class="schedule-cell">
                            <div class="discipline">${scheduleItem.discipline}</div>
                            <div class="group">${scheduleItem.group || ''}</div>
                            <div class="classroom">${scheduleItem.classroom || ''}</div>
                        </div>
                    `;
                } else {
                    td.innerHTML = '<div class="empty-cell">—</div>';
                }
                tr.appendChild(td);
            });

            tbody.appendChild(tr);
        });

        table.appendChild(tbody);
        dayContainer.appendChild(table);
        resultsBody.appendChild(dayContainer);
    });

    resultsContainer.style.display = 'block';
}