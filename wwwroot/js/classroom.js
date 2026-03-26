// ============================================
// ФАЙЛ ЗАНЯТОСТИ АУДИТОРИЙ
// ============================================

let buildings = [];
let classrooms = [];

document.addEventListener('DOMContentLoaded', async function () {
    // Проверка прав администратора
    if (!checkOperator()) {
        return;
    }

    // Загружаем недели из серверной переменной (вставляется в index.html, но здесь нет динамики, поэтому получаем отдельно)
    await loadWeeks();
    await loadBuildings();

    const weekSelect = document.getElementById('week-select');
    const buildingSelect = document.getElementById('building-select');
    const classroomSelect = document.getElementById('classroom-select');
    const showBtn = document.getElementById('show-schedule-btn');

    buildingSelect.addEventListener('change', async function () {
        const buildingId = this.value;
        if (buildingId) {
            await loadClassrooms(buildingId);
            classroomSelect.disabled = false;
        } else {
            classroomSelect.innerHTML = '<option value="">Сначала выберите корпус</option>';
            classroomSelect.disabled = true;
        }
        document.getElementById('schedule-container').style.display = 'none';
    });

    showBtn.addEventListener('click', loadClassroomSchedule);
});

// Загрузка списка недель (используем данные из DataManager.CurrentDates)
async function loadWeeks() {
    try {
        const response = await fetch('/api/weeks');
        if (response.ok) {
            const weeks = await response.json();
            const weekSelect = document.getElementById('week-select');
            weekSelect.innerHTML = weeks.map(w => `<option value="${w}">${w}</option>`).join('');
        }
    } catch (error) {
        console.error('Ошибка загрузки недель:', error);
    }
}

// Загрузка списка корпусов
async function loadBuildings() {
    try {
        const response = await fetch('/api/buildings');
        if (response.ok) {
            buildings = await response.json();
            const buildingSelect = document.getElementById('building-select');
            buildingSelect.innerHTML = '<option value="">Выберите корпус</option>' +
                buildings.map(b => `<option value="${b.id}">${b.name}</option>`).join('');
        }
    } catch (error) {
        console.error('Ошибка загрузки корпусов:', error);
    }
}

// Загрузка аудиторий для выбранного корпуса
async function loadClassrooms(buildingId) {
    try {
        const response = await fetch(`/api/buildings/${buildingId}/classrooms`);
        if (response.ok) {
            classrooms = await response.json();
            const classroomSelect = document.getElementById('classroom-select');
            classroomSelect.innerHTML = '<option value="">Выберите аудиторию</option>' +
                classrooms.map(c => `<option value="${c.id}">${c.name}</option>`).join('');
        }
    } catch (error) {
        console.error('Ошибка загрузки аудиторий:', error);
    }
}

// Загрузка расписания для выбранной аудитории и недели
async function loadClassroomSchedule() {
    const week = document.getElementById('week-select').value;
    const classroomId = document.getElementById('classroom-select').value;

    if (!week || !classroomId) {
        showMessage('Выберите неделю и аудиторию');
        return;
    }

    const scheduleContainer = document.getElementById('schedule-container');
    const scheduleBody = document.getElementById('schedule-body');
    const messageContainer = document.getElementById('message-container');
    const messageText = document.getElementById('message-text');

    // Показываем загрузку
    scheduleContainer.style.display = 'block';
    scheduleBody.innerHTML = '<tr><td colspan="6" style="text-align:center;">Загрузка...</td></tr>';
    messageContainer.style.display = 'none';

    try {
        const response = await fetch(`/api/classroom/${classroomId}/schedule?week=${week}`);
        if (response.ok) {
            const data = await response.json();
            displayClassroomSchedule(data);
        } else {
            showMessage('Не удалось загрузить расписание');
        }
    } catch (error) {
        console.error('Ошибка:', error);
        showMessage('Ошибка соединения');
    }
}

// Отображение таблицы расписания аудитории
// Отображение расписания аудитории в виде 6 отдельных таблиц (по дням)
function displayClassroomSchedule(scheduleData) {
    const scheduleContainer = document.getElementById('schedule-container');
    const scheduleBody = document.getElementById('schedule-body');
    const container = scheduleContainer; // контейнер, где будут таблицы

    if (!container) return;

    // Дни недели
    const days = ['Понедельник', 'Вторник', 'Среда', 'Четверг', 'Пятница', 'Суббота'];
    // Пары
    const pairs = [1, 2, 3, 4, 5, 6];

    // Очищаем контейнер и скрываем старую таблицу
    container.innerHTML = '';

    // Для каждого дня создаём отдельную таблицу
    days.forEach(day => {
        // Блок таблицы
        const dayBlock = document.createElement('div');
        dayBlock.className = 'day-schedule';
        dayBlock.style.marginBottom = '30px';
        dayBlock.style.background = 'var(--white)';
        dayBlock.style.borderRadius = 'var(--radius)';
        dayBlock.style.boxShadow = 'var(--shadow)';
        dayBlock.style.padding = '20px';

        // Заголовок дня
        const dayTitle = document.createElement('h3');
        dayTitle.textContent = day;
        dayTitle.style.margin = '0 0 15px 0';
        dayTitle.style.color = 'var(--blue-dark)';
        dayBlock.appendChild(dayTitle);

        // Таблица: строки – пары, столбцы – информация о занятии
        const table = document.createElement('table');
        table.style.width = '100%';
        table.style.borderCollapse = 'collapse';

        // Заголовок: номер пары
        const thead = document.createElement('thead');
        const headerRow = document.createElement('tr');
        const thPair = document.createElement('th');
        thPair.textContent = 'Пара';
        thPair.style.padding = '12px';
        thPair.style.backgroundColor = 'var(--blue)';
        thPair.style.color = 'white';
        headerRow.appendChild(thPair);

        const thInfo = document.createElement('th');
        thInfo.textContent = 'Занятие';
        thInfo.style.padding = '12px';
        thInfo.style.backgroundColor = 'var(--blue)';
        thInfo.style.color = 'white';
        headerRow.appendChild(thInfo);

        thead.appendChild(headerRow);
        table.appendChild(thead);

        // Тело таблицы: строки для пар
        const tbody = document.createElement('tbody');
        pairs.forEach(pair => {
            const tr = document.createElement('tr');

            // Ячейка с номером пары
            const tdPair = document.createElement('td');
            tdPair.textContent = `${pair} пара`;
            tdPair.style.padding = '10px';
            tdPair.style.fontWeight = '500';
            tdPair.style.backgroundColor = 'var(--bg)';
            tr.appendChild(tdPair);

            // Ячейка с информацией о занятии
            const tdInfo = document.createElement('td');
            tdInfo.style.padding = '10px';
            const key = `${day}_${pair}`;
            const item = scheduleData[key];

            if (item && item.discipline) {
                tdInfo.innerHTML = '<span style="color: #e74c3c;">●</span> Занято';
            } else {
                tdInfo.innerHTML = '<span style="color: #27ae60;">○</span> Свободно';
            }

            tr.appendChild(tdInfo);
            tbody.appendChild(tr);
        });

        table.appendChild(tbody);
        dayBlock.appendChild(table);
        container.appendChild(dayBlock);
    });

    // Показываем контейнер
    container.style.display = 'block';
}

function showMessage(msg) {
    const scheduleContainer = document.getElementById('schedule-container');
    const messageContainer = document.getElementById('message-container');
    const messageText = document.getElementById('message-text');
    scheduleContainer.style.display = 'none';
    messageText.textContent = msg;
    messageContainer.style.display = 'block';
}