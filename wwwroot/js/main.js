document.addEventListener('DOMContentLoaded', function() {
   
    // Переключение между типами поиска
    const searchBtns = document.querySelectorAll('.search-btn');
    const filterBlocks = document.querySelectorAll('.filter-group');
    
    // Элементы для сообщений и результатов
    const resultsContainer = document.getElementById('results-container');
    const messageContainer = document.getElementById('message-container');
    const messageText = document.getElementById('message-text');
    const resultsBody = document.getElementById('results-body');
    
    // Функция скрыть всё
    function hideAll() {
        if (resultsContainer) resultsContainer.style.display = 'none';
        if (messageContainer) messageContainer.style.display = 'none';
    }
    
    // Функция показать сообщение
    function showMessage(msg) {
        hideAll();
        if (messageText) messageText.textContent = msg;
        if (messageContainer) messageContainer.style.display = 'block';
    }
    
    // Функция показать результаты
    function showResults() {
        hideAll();
        if (resultsContainer) resultsContainer.style.display = 'block';
    }
    
    // Функция переключения фильтров
    function switchFilter(type) {
        searchBtns.forEach(btn => {
            if (btn.dataset.type === type) {
                btn.classList.add('active');
            } else {
                btn.classList.remove('active');
            }
        });
        
        filterBlocks.forEach(block => {
            if (block.dataset.filterType === type) {
                block.style.display = 'block';
            } else {
                block.style.display = 'none';
            }
        });
        
        // При смене типа скрываем всё
        hideAll();
    }
    
    // Проверка выбраны ли фильтры
    function hasSelectedFilters(filterBlock) {
        const selects = filterBlock.querySelectorAll('select');
        const inputs = filterBlock.querySelectorAll('input');
        
        for (let select of selects) {
            if (select.value && select.value !== '') {
                return true;
            }
        }
        
        for (let input of inputs) {
            if (input.value && input.value.trim() !== '') {
                return true;
            }
        }
        
        return false;
    }
    
    // Получить данные (имитация API)
    async function getData(filterType, filters) {
        // Здесь будет реальный запрос к API
        return new Promise((resolve) => {
            setTimeout(() => {
                if (filterType === 'group') {
                    resolve([
                        { name: 'АТПП6233-1', specialty: 'Автоматизация технологических процессов' },
                        { name: 'БИВТ-ВП-23', specialty: 'Информатика и вычислительная техника' },
                        { name: 'БПМИ-ПТ-23', specialty: 'Прикладная математика и информатика' }
                    ]);
                } else if (filterType === 'teacher') {
                    resolve([
                        { name: 'Богомолов Р.А.', specialty: 'Кафедра математики' },
                        { name: 'Иванов И.И.', specialty: 'Кафедра информатики' }
                    ]);
                } else {
                    resolve([
                        { name: '310 (Ленина, 57)', specialty: 'Лекционная аудитория' },
                        { name: '205 (Егорова, 16)', specialty: 'Компьютерный класс' }
                    ]);
                }
            }, 500);
        });
    }
    
    // Обновить таблицу
    function updateTable(data) {
        if (!resultsBody) return;
        
        resultsBody.innerHTML = '';
        
        if (!data || data.length === 0) {
            const tr = document.createElement('tr');
            const td = document.createElement('td');
            td.colSpan = 2;
            td.textContent = 'Ничего не найдено';
            td.style.textAlign = 'center';
            td.style.padding = '40px';
            tr.appendChild(td);
            resultsBody.appendChild(tr);
        } else {
            data.forEach(item => {
                const tr = document.createElement('tr');
                
                const tdName = document.createElement('td');
                const link = document.createElement('a');
                link.href = `templates/group-schedule.html?group=${encodeURIComponent(item.name)}`;
                link.textContent = item.name;
                tdName.appendChild(link);
                
                const tdSpecialty = document.createElement('td');
                tdSpecialty.textContent = item.specialty;
                
                tr.appendChild(tdName);
                tr.appendChild(tdSpecialty);
                resultsBody.appendChild(tr);
            });
        }
        
        showResults();
    }
    
    // Собрать выбранные фильтры
    function getSelectedFilters(filterBlock) {
        const selects = filterBlock.querySelectorAll('select');
        const inputs = filterBlock.querySelectorAll('input');
        const filters = {};
        
        selects.forEach(select => {
            if (select.value && select.value !== '') {
                const label = select.previousElementSibling?.textContent || 'Параметр';
                filters[label] = select.value;
            }
        });
        
        inputs.forEach(input => {
            if (input.value && input.value.trim() !== '') {
                const label = input.previousElementSibling?.textContent || 'Параметр';
                filters[label] = input.value.trim();
            }
        });
        
        return filters;
    }
    
    // Обработчики для кнопок выбора типа
    searchBtns.forEach(btn => {
        btn.addEventListener('click', function() {
            const type = this.dataset.type;
            switchFilter(type);
        });
    });

    // Обработчик для кнопки "Показать"
    const showButtons = document.querySelectorAll('.filter-show-btn');
    
    showButtons.forEach(btn => {
        btn.addEventListener('click', async function() {
            const filterBlock = this.closest('.filter-group');
            const filterType = filterBlock.dataset.filterType;
            
            // Проверяем, выбраны ли фильтры
            if (!hasSelectedFilters(filterBlock)) {
                // Показываем сообщение на странице
                showMessage(' Пожалуйста, выберите или введите параметры для поиска');
                return;
            }
            
            // Показываем загрузку
            if (resultsBody) {
                resultsBody.innerHTML = '<tr><td colspan="2" style="text-align:center; padding:40px;">⏳ Загрузка...</td></tr>';
                showResults();
            }
            
            try {
                const filters = getSelectedFilters(filterBlock);
                const data = await getData(filterType, filters);
                updateTable(data);
            } catch (error) {
                console.error('Ошибка:', error);
                showMessage(' Ошибка при загрузке данных. Попробуйте позже.');
            }
        });
    });

    // Инициализация - скрываем всё
    hideAll();
});