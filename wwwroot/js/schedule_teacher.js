// Расписание по преподавателям

// Поиск свободных окон
async function searchFreeSlots() {
    const teacherIdsInput = document.getElementById('teacherIds');
    const startDate = document.getElementById('startDate').value;
    const endDate = document.getElementById('endDate').value;
    const resultsDiv = document.getElementById('results');
    
    // Получаем и очищаем введенные ID
    let teacherIds = teacherIdsInput.value.trim();
    
    if (!teacherIds) {
        resultsDiv.innerHTML = '<div class="error-message">Введите ID преподавателя</div>';
        return;
    }
    
    if (!startDate || !endDate) {
        resultsDiv.innerHTML = '<div class="error-message">Выберите период</div>';
        return;
    }
    
    // Преобразуем строку в массив ID (убираем пробелы, фильтруем пустые)
    let idsArray = teacherIds.split(',').map(id => id.trim()).filter(id => id !== '');
    
    if (idsArray.length === 0) {
        resultsDiv.innerHTML = '<div class="error-message">Введите корректные ID преподавателей</div>';
        return;
    }
    
    resultsDiv.innerHTML = '<div class="loading">Поиск свободных окон...</div>';
    
    // Восстанавливаем учетные данные из localStorage
    loadAuth();
    
    // Формируем строку UIDs через запятую
    const uids = idsArray.join(',');
    
    console.log('Поиск для ID:', uids);
    console.log('Период:', startDate, endDate);
    
    // Вызываем API
    const result = await getTeachersFreeSlots(idsArray, startDate, endDate);
    
    console.log('Результат:', result);
    
    if (result.success) {
        displayFreeSlots(result.data);
    } else {
        resultsDiv.innerHTML = '<div class="error-message">Ошибка поиска: ' + result.error + '</div>';
        
        // Если ошибка авторизации
        if (result.error === 'Неверные учетные данные' || result.error === 'Нет прав доступа') {
            setTimeout(() => {
                window.location.href = 'login.html';
            }, 2000);
        }
    }
}

// Отображаем свободные окна
function displayFreeSlots(data) {
    const resultsDiv = document.getElementById('results');
    
    if (!data || data.length === 0) {
        resultsDiv.innerHTML = '<div class="no-data">Свободные окна не найдены</div>';
        return;
    }
    
    let html = `
        <h3>Результаты поиска</h3>
        <div class="table-container">
            <table class="results-table">
                <thead>
                    <tr>
                        <th>Дата</th>
                        <th>Время начала</th>
                        <th>Время окончания</th>
                        <th>Аудитория</th>
                    </tr>
                </thead>
                <tbody>
    `;
    
    data.forEach(slot => {
        html += `
            <tr>
                <td>${slot.Date || slot.StartDate || '-'}</td>
                <td>${slot.StartTime || slot.Start || '-'}</td>
                <td>${slot.EndTime || slot.End || '-'}</td>
                <td>${slot.Room || '-'}</td>
            </tr>
        `;
    });
    
    html += `
                </tbody>
            </table>
        </div>
    `;
    
    resultsDiv.innerHTML = html;
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
(function init() {
    // Восстанавливаем учетные данные из localStorage в заголовки API
    loadAuth();
    
    setDefaultDates();
    
    const searchBtn = document.getElementById('searchBtn');
    if (searchBtn) {
        searchBtn.addEventListener('click', searchFreeSlots);
    }
})();