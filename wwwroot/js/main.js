document.addEventListener('DOMContentLoaded', function() {
   
    // Переключение между типами поиска
    const searchBtns = document.querySelectorAll('.search-btn');
    const filterBlocks = document.querySelectorAll('.filter-group');
    
    // Функция переключения фильтров
    function switchFilter(type) {
        // Обновляем кнопки
        searchBtns.forEach(btn => {
            if (btn.dataset.type === type) {
                btn.classList.add('active');
            } else {
                btn.classList.remove('active');
            }
        });
        
        // Обновляем блоки фильтров
        filterBlocks.forEach(block => {
            if (block.dataset.filterType === type) {
                block.style.display = 'block';
            } else {
                block.style.display = 'none';
            }
        });
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
    
    function collectFilterData(filterBlock) {
        const selects = filterBlock.querySelectorAll('select');
        const inputs = filterBlock.querySelectorAll('input');
        
        let message = 'Поиск по:\n';
        let hasValue = false;
        const params = new URLSearchParams();
        
        selects.forEach(select => {
            if (select.value) {
                const label = select.previousElementSibling?.textContent || 'Параметр';
                const selectedText = select.options[select.selectedIndex]?.text || select.value;
                message += `• ${label}: ${selectedText}\n`;
                hasValue = true;
                
                // Сохраняем для возможного запроса
                params.append(select.id || 'select', select.value);
            }
        });
        
        inputs.forEach(input => {
            if (input.value.trim()) {
                const label = input.previousElementSibling?.textContent || 'Параметр';
                message += `• ${label}: ${input.value.trim()}\n`;
                hasValue = true;
                
                // Сохраняем для возможного запроса
                params.append(input.id || 'input', input.value.trim());
            }
        });
        
        return { hasValue, message, params };
    }
    
    showButtons.forEach(btn => {
        btn.addEventListener('click', function() {
            const filterBlock = this.closest('.filter-group');
            const { hasValue, message } = collectFilterData(filterBlock);
            
            if (hasValue) {
                alert(message);
                // Здесь потом будет запрос к API или переход на страницу результатов
                // window.location.href = 'search-results.html?' + params.toString();
            } else {
                alert('Выберите или введите параметры для поиска');
            }
        });
    });

    const activeType = document.querySelector('.search-btn.active')?.dataset.type || 'group';
    switchFilter(activeType);
});