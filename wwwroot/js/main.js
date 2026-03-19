document.addEventListener('DOMContentLoaded', function() {
    
    // Данные для фильтров
    const weeks = [
        '16.03.2026 - 22.03.2026 (четная)',
        '23.03.2026 - 29.03.2026 (нечетная)',
        '30.03.2026 - 05.04.2026 (четная)',
        '06.04.2026 - 12.04.2026 (нечетная)'
    ];
    
    const groups = [
        'БИВТ-ВТД-23',
        'БИВТ-ВП-23',
        'ПМИ-23',
        'АТПП-23'
    ];
    
    const teachers = [
        'Богомолов Р.А.',
        'Золотов О.В.',
        'Скотаренко О.В.',
        'Петров А.В.',
        'Сидорова Е.В.'
    ];
    
    const classrooms = [
        '310 (Ленина, 57)',
        '311 (Егорова, 16)',
        '316 (Ленина, 57)',
        '410 (Ленина, 57)',
        '215 (Егорова, 16)'
    ];
    
    const institutes = [
        'ЕТИ',
        'ИГ и СН',
        'ИИС и ЦТ',
        'ИКИ и П',
        'ИП и П',
        'ИПАТ',
        'МА',
        'МБИ',
        'ФФК и С',
        'ЮФ'
    ];
    
    const courses = ['1 курс', '2 курс', '3 курс', '4 курс', '5 курс', '6 курс', '7 курс'];
    
    const buildings = [
        'ул. Ленина, 57',
        'ул. Егорова, 16',
        'ул. Егорова, 15',
        'ул. Коммуны, 9',
        'Корпус А',
        'Корпус В',
        'Корпус Е',
        'Корпус КСК',
        'Корпус Л',
        'Корпус М',
        'Корпус Н',
        'Корпус П',
        'Корпус С',
        'Корпус Э',
        'Столовая'
    ];
    
    // Получаем элементы
    const searchBtns = document.querySelectorAll('.search-btn');
    const filterContent = document.getElementById('filter-content');
    
    // Текущий активный тип поиска
    let currentType = 'group';
    
    // Функция создания селекта
    function createSelect(options, placeholder, id = '') {
        const select = document.createElement('select');
        if (id) select.id = id;
        
        const placeholderOption = document.createElement('option');
        placeholderOption.value = '';
        placeholderOption.textContent = placeholder;
        select.appendChild(placeholderOption);
        
        options.forEach(opt => {
            const option = document.createElement('option');
            option.value = opt;
            option.textContent = opt;
            select.appendChild(option);
        });
        
        return select;
    }
    
    // Функция создания фильтров для групп
    function renderGroupFilters() {
        filterContent.innerHTML = '';
        
        // Контейнер для фильтров
        const filterWrapper = document.createElement('div');
        filterWrapper.className = 'filter-group-dynamic';
        
        const row = document.createElement('div');
        row.className = 'filter-row-dynamic';
        
        // Неделя
        const weekItem = document.createElement('div');
        weekItem.className = 'filter-item';
        const weekLabel = document.createElement('label');
        weekLabel.textContent = 'Неделя';
        weekItem.appendChild(weekLabel);
        weekItem.appendChild(createSelect(weeks, 'Выберите неделю', 'week-select'));
        row.appendChild(weekItem);
        
        // Группа
        const groupItem = document.createElement('div');
        groupItem.className = 'filter-item';
        const groupLabel = document.createElement('label');
        groupLabel.textContent = 'Группа';
        groupItem.appendChild(groupLabel);
        groupItem.appendChild(createSelect(groups, 'Выберите группу', 'group-select'));
        row.appendChild(groupItem);
        
        // Институт
        const instItem = document.createElement('div');
        instItem.className = 'filter-item';
        const instLabel = document.createElement('label');
        instLabel.textContent = 'Институт/Факультет';
        instItem.appendChild(instLabel);
        instItem.appendChild(createSelect(institutes, 'Выберите институт', 'institute-select'));
        row.appendChild(instItem);
        
        // Курс
        const courseItem = document.createElement('div');
        courseItem.className = 'filter-item';
        const courseLabel = document.createElement('label');
        courseLabel.textContent = 'Курс';
        courseItem.appendChild(courseLabel);
        courseItem.appendChild(createSelect(courses, 'Выберите курс', 'course-select'));
        row.appendChild(courseItem);
        
        filterWrapper.appendChild(row);
        filterContent.appendChild(filterWrapper);
        
        addShowButton();
    }
    
    // Функция создания фильтров для преподавателей
    function renderTeacherFilters() {
        filterContent.innerHTML = '';
        
        const filterWrapper = document.createElement('div');
        filterWrapper.className = 'filter-group-dynamic';
        
        const row = document.createElement('div');
        row.className = 'filter-row-dynamic';
        
        // Неделя
        const weekItem = document.createElement('div');
        weekItem.className = 'filter-item';
        const weekLabel = document.createElement('label');
        weekLabel.textContent = 'Неделя';
        weekItem.appendChild(weekLabel);
        weekItem.appendChild(createSelect(weeks, 'Выберите неделю', 'week-select'));
        row.appendChild(weekItem);
        
        // Преподаватель - поле ввода
        const teacherItem = document.createElement('div');
        teacherItem.className = 'filter-item';
        teacherItem.style.flex = '2';
        
        const teacherLabel = document.createElement('label');
        teacherLabel.textContent = 'Преподаватель';
        teacherItem.appendChild(teacherLabel);
        
        const teacherInput = document.createElement('input');
        teacherInput.type = 'text';
        teacherInput.id = 'teacher-input';
        teacherInput.placeholder = 'Введите ФИО преподавателя';
        
        teacherItem.appendChild(teacherInput);
        row.appendChild(teacherItem);
        
        filterWrapper.appendChild(row);
        filterContent.appendChild(filterWrapper);
        
        addShowButton();
    }
    
    // Функция создания фильтров для аудиторий
    function renderClassroomFilters() {
        filterContent.innerHTML = '';
        
        const filterWrapper = document.createElement('div');
        filterWrapper.className = 'filter-group-dynamic';
        
        const row = document.createElement('div');
        row.className = 'filter-row-dynamic';
        
        // Неделя
        const weekItem = document.createElement('div');
        weekItem.className = 'filter-item';
        const weekLabel = document.createElement('label');
        weekLabel.textContent = 'Неделя';
        weekItem.appendChild(weekLabel);
        weekItem.appendChild(createSelect(weeks, 'Выберите неделю', 'week-select'));
        row.appendChild(weekItem);
        
        // Аудитория
        const classroomItem = document.createElement('div');
        classroomItem.className = 'filter-item';
        const classroomLabel = document.createElement('label');
        classroomLabel.textContent = 'Аудитория';
        classroomItem.appendChild(classroomLabel);
        classroomItem.appendChild(createSelect(classrooms, 'Выберите аудиторию', 'classroom-select'));
        row.appendChild(classroomItem);
        
        // Корпус
        const buildingItem = document.createElement('div');
        buildingItem.className = 'filter-item';
        const buildingLabel = document.createElement('label');
        buildingLabel.textContent = 'Корпус';
        buildingItem.appendChild(buildingLabel);
        buildingItem.appendChild(createSelect(buildings, 'Выберите корпус', 'building-select'));
        row.appendChild(buildingItem);
        
        filterWrapper.appendChild(row);
        filterContent.appendChild(filterWrapper);
        
        addShowButton();
    }
    
    // Функция добавления кнопки "Показать"
    function addShowButton() {
        const showBtn = document.createElement('button');
        showBtn.className = 'filter-show-btn';
        showBtn.textContent = 'Показать';
        
        showBtn.addEventListener('click', function() {
            // Собираем все выбранные значения из select и input
            const selects = filterContent.querySelectorAll('select');
            const inputs = filterContent.querySelectorAll('input');
            let message = 'Поиск по:\n';
            let hasValue = false;
            
            selects.forEach(select => {
                if (select.value) {
                    const label = select.previousElementSibling?.textContent || 'Параметр';
                    message += `• ${label}: ${select.value}\n`;
                    hasValue = true;
                }
            });
            
            inputs.forEach(input => {
                if (input.value.trim()) {
                    const label = input.previousElementSibling?.textContent || 'Параметр';
                    message += `• ${label}: ${input.value.trim()}\n`;
                    hasValue = true;
                }
            });
            
            if (hasValue) {
                alert(message);
                // Здесь потом будет запрос к API
            } else {
                alert('Выберите или введите параметры для поиска');
            }
        });
        
        filterContent.appendChild(showBtn);
    }
    
    // Обработчики для кнопок выбора типа
    searchBtns.forEach(btn => {
        btn.addEventListener('click', function() {
            // Убираем активный класс у всех
            searchBtns.forEach(b => b.classList.remove('active'));
            // Добавляем активный класс текущей кнопке
            this.classList.add('active');
            
            // Получаем тип поиска
            const type = this.dataset.type;
            currentType = type;
            
            // Рендерим соответствующие фильтры
            switch(type) {
                case 'group':
                    renderGroupFilters();
                    break;
                case 'teacher':
                    renderTeacherFilters();
                    break;
                case 'classroom':
                    renderClassroomFilters();
                    break;
            }
        });
    });
    
    // Инициализация - показываем фильтры по группам
    renderGroupFilters();
});
