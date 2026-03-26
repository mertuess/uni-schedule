// ============================================
// ГЛАВНЫЙ ФАЙЛ - основная логика главной страницы
// Отвечает за:
// - Фильтрацию расписания (группы, преподаватели, аудитории)
// - Отображение результатов поиска
// - Управление навигацией (меню, выход, отображение email)
// ============================================

document.addEventListener('DOMContentLoaded', function() {
   
    // Обновляем навигацию и информацию о пользователе при загрузке страницы
    updateUserHeader();
    updateNavigation();
   
    // Элементы DOM для работы с фильтрами и результатами
    const searchBtns = document.querySelectorAll('.search-btn');
    const filterBlocks = document.querySelectorAll('.filter-group');
    const resultsContainer = document.getElementById('results-container');
    const messageContainer = document.getElementById('message-container');
    const messageText = document.getElementById('message-text');
    const resultsBody = document.getElementById('results-body');
    
    // Скрыть блоки результатов и сообщений
    function hideAll() {
        if (resultsContainer) resultsContainer.style.display = 'none';
        if (messageContainer) messageContainer.style.display = 'none';
    }
    
    // Показать сообщение (например, ошибку или подсказку)
    function showMessage(msg) {
        hideAll();
        if (messageText) messageText.textContent = msg;
        if (messageContainer) messageContainer.style.display = 'block';
    }
    
    // Показать результаты поиска
    function showResults() {
        hideAll();
        if (resultsContainer) resultsContainer.style.display = 'block';
    }
    
    // Переключение типа поиска (группы/преподаватели/аудитории)
    function switchFilter(type) {
        searchBtns.forEach(function(btn) {
            if (btn.dataset.type === type) {
                btn.classList.add('active');
            } else {
                btn.classList.remove('active');
            }
        });
        
        filterBlocks.forEach(function(block) {
            if (block.dataset.filterType === type) {
                block.style.display = 'block';
            } else {
                block.style.display = 'none';
            }
        });
        
        hideAll();
    }
    
    // Проверка, выбраны ли какие-то фильтры
    function hasSelectedFilters(filterBlock) {
        const selects = filterBlock.querySelectorAll('select');
        const inputs = filterBlock.querySelectorAll('input');
        
        for (let i = 0; i < selects.length; i++) {
            if (selects[i].value && selects[i].value !== '') {
                return true;
            }
        }
        
        for (let i = 0; i < inputs.length; i++) {
            if (inputs[i].value && inputs[i].value.trim() !== '') {
                return true;
            }
        }
        
        return false;
    }
    
    // Запрос к API для получения данных расписания
    async function getData(filterType, filters) {
        try {
            const response = await fetch('/api/schedule?type=' + filterType + '&' + new URLSearchParams(filters));
            if (response.ok) {
                return await response.json();
            }
            return [];
        } catch (error) {
            console.error('Ошибка:', error);
            return [];
        }
    }
    
    // Обновление таблицы с результатами поиска
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
            for (let i = 0; i < data.length; i++) {
                const item = data[i];
                const tr = document.createElement('tr');
                
                const tdName = document.createElement('td');
                const link = document.createElement('a');
                link.href = 'templates/group-schedule.html?group=' + encodeURIComponent(item.name);
                link.textContent = item.name;
                tdName.appendChild(link);
                
                const tdSpecialty = document.createElement('td');
                tdSpecialty.textContent = item.specialty;
                
                tr.appendChild(tdName);
                tr.appendChild(tdSpecialty);
                resultsBody.appendChild(tr);
            }
        }
        
        showResults();
    }
    
    // Сбор выбранных значений фильтров
    function getSelectedFilters(filterBlock) {
        const selects = filterBlock.querySelectorAll('select');
        const inputs = filterBlock.querySelectorAll('input');
        const filters = {};
        
        selects.forEach(function(select) {
            if (select.value && select.value !== '') {
                filters[select.id || 'select'] = select.value;
            }
        });
        
        inputs.forEach(function(input) {
            if (input.value && input.value.trim() !== '') {
                filters[input.id || 'input'] = input.value.trim();
            }
        });
        
        return filters;
    }
    
    // Обработчики кнопок переключения типа поиска
    for (let i = 0; i < searchBtns.length; i++) {
        searchBtns[i].addEventListener('click', function() {
            const type = this.dataset.type;
            switchFilter(type);
        });
    }

    // Обработчики кнопки "Показать"
    const showButtons = document.querySelectorAll('.filter-show-btn');
    
    for (let i = 0; i < showButtons.length; i++) {
        showButtons[i].addEventListener('click', async function() {
            const filterBlock = this.closest('.filter-group');
            const filterType = filterBlock.dataset.filterType;
            
            if (!hasSelectedFilters(filterBlock)) {
                showMessage('Выберите или введите параметры для поиска');
                return;
            }
            
            if (resultsBody) {
                resultsBody.innerHTML = '的人<td colspan="2" style="text-align:center; padding:40px;">Загрузка...<\/td><\/tr>';
                showResults();
            }
            
            try {
                const filters = getSelectedFilters(filterBlock);
                const data = await getData(filterType, filters);
                updateTable(data);
            } catch (error) {
                showMessage('Ошибка при загрузке данных');
            }
        });
    }

    hideAll();
});

// ============================================
// ФУНКЦИИ НАВИГАЦИИ
// ============================================

// Отображение email пользователя в правом углу
function updateUserHeader() {
    const user = localStorage.getItem('user');
    const navUl = document.querySelector('header nav ul');
    
    if (!navUl) return;
    
    let userSpan = document.querySelector('.user-email-text');
    if (userSpan) {
        userSpan.remove();
    }
    
    if (user) {
        const userData = JSON.parse(user);
        
        userSpan = document.createElement('li');
        userSpan.className = 'user-email-text';
        userSpan.style.marginLeft = 'auto';
        userSpan.style.color = 'var(--white)';
        userSpan.style.padding = '8px 16px';
        userSpan.style.fontWeight = '500';
        userSpan.style.background = 'rgba(255,255,255,0.1)';
        userSpan.style.borderRadius = '30px';
        userSpan.textContent = userData.mail;
        
        navUl.appendChild(userSpan);
    }
}

// Управление пунктами меню (Вход/Выход, Панель администратора)
function updateNavigation() {
    const user = localStorage.getItem('user');
    const navUl = document.querySelector('header nav ul');
    
    if (!navUl) return;
    
    let loginLi = null;
    let adminLi = null;
    let logoutLi = null;
    
    // Поиск существующих элементов в меню
    for (let i = 0; i < navUl.children.length; i++) {
        const li = navUl.children[i];
        const link = li.querySelector('a');
        if (link) {
            if (link.getAttribute('href') === 'templates/login.html') {
                loginLi = li;
            }
            if (link.getAttribute('href') === 'templates/dashboard.html') {
                adminLi = li;
            }
            if (link.textContent === 'Выход') {
                logoutLi = li;
            }
        }
    }
    
    if (user) {
        const userData = JSON.parse(user);
        
        // Удаляем кнопку "Вход", если есть
        if (loginLi) {
            loginLi.remove();
        }
        
        // Добавляем "Панель администратора" только для оператора
        if (userData.role === 'operator') {
            if (!adminLi) {
                const newLi = document.createElement('li');
                newLi.innerHTML = '<a href="templates/dashboard.html">Панель администратора</a>';
                navUl.insertBefore(newLi, navUl.children[navUl.children.length - 2]);
            }
        } else {
            if (adminLi) {
                adminLi.remove();
            }
        }
        
        // Добавляем кнопку "Выход"
        if (!logoutLi) {
            const newLi = document.createElement('li');
            newLi.innerHTML = '<a href="#" onclick="logout()">Выход</a>';
            navUl.insertBefore(newLi, navUl.children[navUl.children.length - 1]);
        }
    } else {
        // Пользователь не залогинен - убираем админ-панель и выход
        if (adminLi) {
            adminLi.remove();
        }
        if (logoutLi) {
            logoutLi.remove();
        }
        
        // Добавляем кнопку "Вход"
        if (!loginLi) {
            const newLi = document.createElement('li');
            newLi.innerHTML = '<a href="templates/login.html">Вход</a>';
            navUl.appendChild(newLi);
        }
    }
}

// ============================================
// ФУНКЦИЯ ВЫХОДА ИЗ АККАУНТА
// ============================================

function logout() {
    const user = localStorage.getItem('user');
    let userName = '';
    if (user) {
        const userData = JSON.parse(user);
        userName = userData.mail;
    }
    
    // Удаляем данные пользователя из хранилища
    localStorage.removeItem('user');
    
    // Показываем уведомление о выходе
    showNotification('До свидания, ' + userName + '!', 'warning');
    
    // Обновляем меню
    updateNavigation();
    updateUserHeader();
    
    // Перенаправляем на главную страницу
    setTimeout(function() {
        window.location.href = '/';
    }, 1500);
}

// ============================================
// ФУНКЦИЯ ПОКАЗА УВЕДОМЛЕНИЙ
// ============================================

function showNotification(message, type) {
    const notification = document.createElement('div');
    notification.className = 'notification ' + type;
    notification.textContent = message;
    
    document.body.appendChild(notification);
    
    // Автоматическое исчезновение через 3 секунды
    setTimeout(function() {
        notification.style.animation = 'slideOut 0.3s ease';
        setTimeout(function() {
            notification.remove();
        }, 300);
    }, 3000);
}

// ============================================
// ПРОВЕРКА ПРАВ ОПЕРАТОРА
// ============================================

function checkOperator() {
    const user = localStorage.getItem('user');
    if (!user) {
        window.location.href = '/templates/login.html';
        return false;
    }
    const userData = JSON.parse(user);
    if (userData.role !== 'operator') {
        alert('У вас нет прав доступа');
        window.location.href = '/index.html';
        return false;
    }
    return true;
}

// В функции switchFilter добавьте проверку на тип 'teacher'
function switchFilter(type) {
    searchBtns.forEach(function (btn) {
        if (btn.dataset.type === type) {
            btn.classList.add('active');
        } else {
            btn.classList.remove('active');
        }
    });

    filterBlocks.forEach(function (block) {
        if (block.dataset.filterType === type) {
            block.style.display = 'block';
        } else {
            block.style.display = 'none';
        }
    });

    // Скрываем результаты при смене типа поиска
    hideAll();

    // Если переключились на преподавателей, загружаем список
    if (type === 'teacher') {
        loadTeachersList();
    }
}