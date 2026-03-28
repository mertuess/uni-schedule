// Расписание по аудиториям

// Обработка поиска
async function handleSearch() {
    const roomId = document.getElementById('roomId').value;
    const startDate = document.getElementById('startDate').value;
    const endDate = document.getElementById('endDate').value;
    const resultContainer = document.getElementById('resultContainer');
    const messageBox = document.getElementById('messageBox');
    
    if (!roomId || !startDate || !endDate) {
        messageBox.textContent = 'Заполните все поля поиска';
        messageBox.className = 'error-message';
        resultContainer.innerHTML = '';
        return;
    }
    
    messageBox.textContent = '';
    resultContainer.innerHTML = '<div class="loading">Загрузка...</div>';
    
    // Убеждаемся, что учетные данные загружены
    if (!loadAuth()) {
        resultContainer.innerHTML = '<div class="error-message">Ошибка авторизации</div>';
        return;
    }
    
    const result = await getRoomWorkload(roomId, startDate, endDate);
    
    if (result.success) {
        displayWorkload(result.data, roomId);
        messageBox.textContent = '';
    } else {
        resultContainer.innerHTML = '<div class="error-message">Ошибка загрузки данных</div>';
        messageBox.textContent = 'Ошибка: ' + result.error;
        messageBox.className = 'error-message';
        
        // Если ошибка авторизации, перенаправляем на вход
        if (result.error === 'Неверные учетные данные' || result.error === 'Нет прав доступа') {
            setTimeout(() => {
                window.location.href = 'login.html';
            }, 2000);
        }
    }
}

// Отображаем загруженность аудитории
function displayWorkload(data, roomId) {
    const resultContainer = document.getElementById('resultContainer');
    
    if (!data) {
        resultContainer.innerHTML = `
            <div class="no-data">
                <p>Нет данных о загруженности аудитории ${roomId}</p>
            </div>
        `;
        return;
    }
    
    // Если данные содержат workload объект
    if (data.workload) {
        let html = `
            <div class="workload-info">
                <h3>Аудитория: ${data.room || roomId}</h3>
                <p>Общая загруженность: ${data.workload_percent || 0}%</p>
            </div>
            <div class="table-container">
                <table class="results-table">
                    <thead>
                        <tr>
                            <th>Дата</th>
                            <th>1 пара</th>
                            <th>2 пара</th>
                            <th>3 пара</th>
                            <th>4 пара</th>
                            <th>5 пара</th>
                            <th>6 пара</th>
                            <th>7 пара</th>
                        </tr>
                    </thead>
                    <tbody>
        `;
        
        // Получаем все даты из workload
        const dates = Object.keys(data.workload).sort();
        
        dates.forEach(date => {
            const slots = data.workload[date];
            html += `
                <tr>
                    <td><strong>${date}</strong></td>
            `;
            
            slots.forEach((isBusy, index) => {
                const status = isBusy ? 'Занято' : 'Свободно';
                const statusClass = isBusy ? 'busy' : 'free';
                html += `<td class="${statusClass}">${status}</td>`;
            });
            
            html += `</tr>`;
        });
        
        html += `
                    </tbody>
                </table>
            </div>
        `;
        
        resultContainer.innerHTML = html;
        
        // Добавляем стили для отображения статуса
        const style = document.createElement('style');
        style.textContent = `
            .busy {
                background-color: #ffebee;
                color: #c62828;
                font-weight: 500;
            }
            .free {
                background-color: #e8f5e9;
                color: #2e7d32;
                font-weight: 500;
            }
            .workload-info {
                background: var(--bg);
                padding: 15px;
                border-radius: var(--radius);
                margin-bottom: 20px;
                text-align: center;
            }
            .workload-info h3 {
                margin: 0 0 10px 0;
                color: var(--blue-dark);
            }
        `;
        document.head.appendChild(style);
        
    } else if (Array.isArray(data)) {
        // Если данные пришли как массив событий
        let html = `
            <div class="table-container">
                <table class="results-table">
                    <thead>
                        <tr>
                            <th>Время</th>
                            <th>Событие</th>
                            <th>Преподаватель</th>
                            <th>Группа</th>
                        </tr>
                    </thead>
                    <tbody>
        `;
        
        data.forEach(item => {
            html += `
                <tr>
                    <td>${item.Time || item.StartTime || '-'}</td>
                    <td>${item.Event || item.Name || '-'}</td>
                    <td>${item.Teacher || '-'}</td>
                    <td>${item.Group || '-'}</td>
                </tr>
            `;
        });
        
        html += `
                    </tbody>
                </table>
            </div>
        `;
        
        resultContainer.innerHTML = html;
    } else {
        resultContainer.innerHTML = `
            <div class="no-data">
                <p>Данные в неожиданном формате</p>
                <pre>${JSON.stringify(data, null, 2)}</pre>
            </div>
        `;
    }
}

// Устанавливаем даты по умолчанию
function setDefaultDates() {
    const today = new Date();
    const nextWeek = new Date();
    nextWeek.setDate(today.getDate() + 7);
    
    const startDateInput = document.getElementById('startDate');
    const endDateInput = document.getElementById('endDate');
    
    if (startDateInput && !startDateInput.value) {
        startDateInput.value = today.toISOString().split('T')[0];
    }
    if (endDateInput && !endDateInput.value) {
        endDateInput.value = nextWeek.toISOString().split('T')[0];
    }
}

// Инициализация
(async function init() {
    // Загружаем учетные данные
    if (!loadAuth()) {
        // Если нет сохраненных учетных данных, перенаправляем на вход
        window.location.href = 'login.html';
        return;
    }
    
    setDefaultDates();
    
    const searchBtn = document.getElementById('searchBtn');
    if (searchBtn) {
        searchBtn.addEventListener('click', handleSearch);
    }
})();