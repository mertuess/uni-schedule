const buildings_obj = document.getElementById('buildings');
const rooms_obj = document.getElementById('rooms');
const autocomplete_list = document.getElementById('buildings-autocomplete-list');
const selected_list = document.getElementById('selected-buildings-list');
const buildings_input = document.getElementById('buildings-ids');
const date_start_obj = document.getElementById('start-date');
const date_end_obj = document.getElementById('end-date');
const results = document.getElementById('results');

let buildings = [];
let rooms = [];
let selected_buildings = [];

// Форматирование даты с днём недели
function formatDateWithWeekday(dateStr) {
    if (!dateStr) return '';
    const date = new Date(dateStr);
    const weekday = date.toLocaleDateString('ru-RU', { weekday: 'long' });
    const weekdayCapitalized = weekday.charAt(0).toUpperCase() + weekday.slice(1);
    return `${dateStr} (${weekdayCapitalized})`;
}

function extractData(response, primaryField, secondaryField) {
    if (!response?.success || !response.data) return [];
    let data = response.data;
    if (data[primaryField] && Array.isArray(data[primaryField])) {
        return data[primaryField];
    }
    if (data[secondaryField] && Array.isArray(data[secondaryField])) {
        return data[secondaryField];
    }
    if (Array.isArray(data)) {
        return data;
    }
    return [];
}

function getField(obj, primary, secondary, fallback) {
    return obj?.[primary] || obj?.[secondary] || fallback || '';
}

function updateBuildings() {
    if (!buildings_obj) return;
    buildings_obj.innerHTML = '<option value="">-- Выберите корпус --</option>';
    buildings.forEach(function(x) {
        const id = getField(x, 'Id', 'bui_id', null);
        const name = getField(x, 'Name', 'building', 'Корпус ' + id);
        if (!id) return;
        var opt = document.createElement('option');
        opt.value = id;
        opt.textContent = name;
        buildings_obj.appendChild(opt);
    });
}

function updateRooms() {
    if (!rooms_obj) return;
    rooms_obj.innerHTML = '<option value="">-- Выберите аудиторию --</option>';
    rooms.forEach(function(x) {
        const id = getField(x, 'Id', 'room_id', null);
        const name = getField(x, 'Name', 'room', 'Аудитория ' + id);
        if (!id) return;
        var opt = document.createElement('option');
        opt.value = id;
        opt.textContent = name;
        rooms_obj.appendChild(opt);
    });
}

function search(name_part) {
    if (!Array.isArray(buildings)) return [];
    var _lower = name_part.toLowerCase().trim();
    return buildings.filter(function(t) {
        return t && getField(t, 'Name', 'building', '') && getField(t, 'Name', 'building', '').toLowerCase().includes(_lower) &&
            !selected_buildings.some(function(sel) { return getField(sel, 'Id', 'bui_id') === getField(t, 'Id', 'bui_id'); });
    }).slice(0, 10);
}

function updateAutocomplete(items) {
    if (!autocomplete_list) return;
    if (buildings_input && buildings_input.value == '') { hideAutocomplete(); return; }
    autocomplete_list.innerHTML = '';
    items.forEach(function(t) {
        var item = document.createElement('div');
        item.className = 'autocomplete-item';
        item.textContent = getField(t, 'Name', 'building', '');
        item.addEventListener('click', function() { addTeacher(t); });
        autocomplete_list.appendChild(item);
    });
}

function showAutocomplete() { if (autocomplete_list) autocomplete_list.classList.add('show'); }
function hideAutocomplete() {
    setTimeout(function() { if (autocomplete_list) autocomplete_list.classList.remove('show'); }, 200);
}

function addTeacher(t) {
    if (selected_buildings.some(function(teacher) { return getField(teacher, 'Id', 'bui_id') === getField(t, 'Id', 'bui_id'); })) return;
    selected_buildings.push(t);
    updateSelected();
    if (buildings_input) buildings_input.value = '';
    hideAutocomplete();
}

function removeTeacher(building_id) {
    selected_buildings = selected_buildings.filter(function(t) { return getField(t, 'Id', 'bui_id') !== building_id; });
    updateSelected();
}

function onBuildingsInput(e) {
    var searchText = e.target.value;
    var filtered = search(searchText);
    updateAutocomplete(filtered);
    showAutocomplete();
}

function updateSelected() {
    if (!selected_list) return;
    selected_list.innerHTML = '';
    selected_buildings.forEach(function(teacher) {
        var tag = document.createElement('div');
        tag.className = 'selected-teacher-tag';
        tag.innerHTML = getField(teacher, 'Name', 'building', '') + '<button type="button" data-id="' + getField(teacher, 'Id', 'bui_id') + '">×</button>';
        var btn = tag.querySelector('button');
        if (btn) btn.addEventListener('click', function() { removeTeacher(getField(teacher, 'Id', 'bui_id')); });
        selected_list.appendChild(tag);
    });
}

function extractWorkload(response) {
    if (!response?.success || !response.data) return null;
    const data = response.data;
    if (data.workload) return data;
    if (data.data?.workload) return data.data;
    return null;
}

async function showRoomWorkload() {
    if (!results) return;
    var roomId = rooms_obj ? rooms_obj.value : null;
    var start = date_start_obj ? date_start_obj.value : null;
    var end = date_end_obj ? date_end_obj.value : null;

    if (!roomId || !start || !end) {
        results.innerHTML = '<div class="error">Пожалуйста, выберите аудиторию и даты</div>';
        return;
    }

    results.innerHTML = '<div style="text-align:center; padding:40px; color:#666;"><div style="font-size:48px; margin-bottom:10px;"></div><div>Загрузка данных...</div></div>';

    results.innerHTML = '<div class="loading">Загрузка данных...</div>';
    try {
        var response = await getRoomWorkload(roomId, start, end);
        var wl = extractWorkload(response);
        
        if (!wl || !wl.workload) {
            results.innerHTML = '<div class="error">Ошибка: ' + (response && response.error ? response.error : 'Нет данных') + '</div>';
            return;
        }
        
        var pt = getPercentTable([wl]);
        results.innerHTML = '';
        results.appendChild(pt);

        var dates = Object.keys(wl.workload).sort();
        if (dates.length === 0) { results.innerHTML = 'Нет данных о загруженности'; return; }

        for (var i = 0; i < dates.length; i++) {
            var date = dates[i];
            var table = getTableTemplate(`${getField(wl, 'room', 'Room', 'Аудитория')} — ${formatDateWithWeekday(date)}`);
            var row = getTableRow(getField(wl, 'room', 'Room', ''), wl.workload[date]);
            table.appendChild(row);
            results.appendChild(table);
            results.appendChild(document.createElement('br'));
}
        var roomName = rooms_obj && rooms_obj.options[rooms_obj.selectedIndex] ? rooms_obj.options[rooms_obj.selectedIndex].textContent : 'Аудитория';
        activateRoomExport(wl, roomName);
    } catch (error) {
        console.error('Ошибка в showRoomWorkload:', error);
        results.innerHTML = '<div class="error">Ошибка: ' + error.message + '</div>';
    }
}

async function showBuildingWorkload() {
    if (!results) return;
    var buildingId = buildings_obj ? buildings_obj.value : null;
    var start = date_start_obj ? date_start_obj.value : null;
    var end = date_end_obj ? date_end_obj.value : null;

    if (!buildingId || !start || !end) {
        results.innerHTML = '<div class="error">Пожалуйста, выберите корпус и даты</div>';
        return;
    }

    results.innerHTML = '<div style="text-align:center; padding:40px; color:#666;"><div style="font-size:48px; margin-bottom:10px;"></div><div>Загрузка данных...</div></div>';

    results.innerHTML = '<div class="loading">Загрузка данных...</div>';
    try {
        var response = await getBuildingWorkload(buildingId, start, end);
        var wl = extractWorkload(response);
        
        if (!wl || !wl.workload) {
            results.innerHTML = '<div class="error">Ошибка: ' + (response && response.error ? response.error : 'Нет данных') + '</div>';
            return;
        }
        
        var pt = getPercentTable(wl.workload);
        results.innerHTML = '';
        results.appendChild(pt);

        var allDates = new Set();
        for (var i = 0; i < wl.workload.length; i++) {
            var room = wl.workload[i];
            for (var date in room.workload) allDates.add(date);
        }
        var sortedDates = Array.from(allDates).sort();

        for (var j = 0; j < sortedDates.length; j++) {
            var date = sortedDates[j];
            var table = getTableTemplate(`Корпус — ${formatDateWithWeekday(date)}`);
            for (var k = 0; k < wl.workload.length; k++) {
                var room = wl.workload[k];
                if (room.workload[date]) {
                    var row = getTableRow(getField(room, 'room', 'Room', ''), room.workload[date]);
                    table.appendChild(row);
                } else {
                    var row = getEmptyTableRow(getField(room, 'room', 'Room', 'Аудитория'));
                    table.appendChild(row);
                }
            }
            results.appendChild(table);
            results.appendChild(document.createElement('br'));
        }
    } catch (error) {
        console.error('Ошибка в showBuildingWorkload:', error);
        results.innerHTML = '<div class="error">Ошибка: ' + error.message + '</div>';
    }
}

function getEmptyTableRow(room) {
    var r = document.createElement('tr');
    var fd = document.createElement('td');
    fd.textContent = room;
    r.appendChild(fd);
    for (var i = 1; i <= 7; i++) { var d = document.createElement('td'); d.textContent = '—'; r.appendChild(d); }
    return r;
}

function getTableRow(room, dateWorkload) {
    var r = document.createElement('tr');
    var fd = document.createElement('td');
    fd.textContent = room;
    r.appendChild(fd);
    for (var slot = 1; slot <= 7; slot++) {
        var d = document.createElement('td');
        if (dateWorkload && dateWorkload[slot]) d.textContent = 'Занятие';
        else d.textContent = '—';
        r.appendChild(d);
    }
    return r;
}

function getTableTemplate(name) {
    var res = document.createElement('table');
    res.style.tableLayout = 'fixed';
    var tr_0 = document.createElement('tr');
    var tr_1 = document.createElement('tr');
    var fh = document.createElement('th');
    fh.colSpan = 8;
    fh.textContent = name;
    tr_0.appendChild(fh);
    res.appendChild(tr_0);
    var wl_td = document.createElement('th');
    tr_1.appendChild(wl_td);
    ALL_SLOTS.forEach(function(sl) { var sh = document.createElement('th'); sh.textContent = sl; tr_1.appendChild(sh); });
    res.appendChild(tr_1);
    return res;
}

function getPercentTable(wl_arr) {
    var table = document.createElement('table');
    var tr_0 = document.createElement('tr');
    var th_0 = document.createElement('th');
    var th_1 = document.createElement('th');
    th_0.textContent = 'Аудитория';
    th_1.textContent = 'Загруженность';
    tr_0.appendChild(th_0);
    tr_0.appendChild(th_1);
    table.appendChild(tr_0);
    for (var wl in wl_arr) {
        var row = document.createElement('tr');
        var room_cell = document.createElement('td');
        var wl_cell = document.createElement('td');
        room_cell.textContent = getField(wl_arr[wl], 'room', 'Room', 'Аудитория');
        wl_cell.textContent = (wl_arr[wl].workload_percent || 0) + '%';
        row.appendChild(room_cell);
        row.appendChild(wl_cell);
        table.appendChild(row);
    }
    return table;
}

// Обработчик изменения корпуса
if (buildings_obj) {
    buildings_obj.addEventListener("change", async function(event) {
        var selectedBuiId = event.target.value;
        if (!selectedBuiId) return;
        try {
            var roomsResponse = await getRooms(selectedBuiId);
            if (roomsResponse && roomsResponse.success && roomsResponse.data) {
                rooms = extractData(roomsResponse, 'rooms', 'rooms');
                updateRooms();
            }
        } catch (error) { console.error('Ошибка загрузки аудиторий:', error); }
    });
}

document.addEventListener('DOMContentLoaded', async function() {
    // Инициализируем только то, что нужно
    if (!buildings_obj || !rooms_obj) return;
    
    try {
        var buildingsResponse = await getBuildings();
        if (buildingsResponse && buildingsResponse.success && buildingsResponse.data) {
            buildings = extractData(buildingsResponse, 'buildings', 'buildings');
        }
        
        // Загружаем аудитории для первого корпуса, если есть
        if (buildings.length > 0) {
            const firstBuildingId = getField(buildings[0], 'Id', 'bui_id', null);
            if (firstBuildingId) {
                var roomsResponse = await getRooms(firstBuildingId.toString());
                if (roomsResponse && roomsResponse.success && roomsResponse.data) {
                    rooms = extractData(roomsResponse, 'rooms', 'rooms');
                }
            }
        }
        
        updateBuildings();
        updateRooms();
        
        // Оставляем обработчики для совместимости, но они не активны
        if (autocomplete_list && buildings_input) {
            autocomplete_list.style.top = buildings_input.getBoundingClientRect().bottom + 'px';
            autocomplete_list.style.width = buildings_input.offsetWidth.toString() + 'px';
            // buildings_input.addEventListener('input', onBuildingsInput); // ← Отключено, т.к. поле скрыто
        }
    } catch (error) {
        console.error('Ошибка загрузки корпусов:', error);
        buildings = []; rooms = [];
    }
});

window.addEventListener('scroll', function() {
    if (autocomplete_list && buildings_input) {
        autocomplete_list.style.top = buildings_input.getBoundingClientRect().bottom + 'px';
    }
});

// Экспорт в iCal (оставляем как есть)
function convertWorkloadToEvents(workloadData, roomName, date) {
    var events = [];
    var slotMapping = {
        '09:00 - 10:35': { start: '09:00', end: '10:35', num: 1 },
        '10:45 - 12:20': { start: '10:45', end: '12:20', num: 2 },
        '12:40 - 14:15': { start: '12:40', end: '14:15', num: 3 },
        '14:45 - 16:20': { start: '14:45', end: '16:20', num: 4 },
        '16:30 - 18:05': { start: '16:30', end: '18:05', num: 5 },
        '18:15 - 19:50': { start: '18:15', end: '19:50', num: 6 },
        '20:00 - 21:35': { start: '20:00', end: '21:35', num: 7 }
    };
    if (workloadData && workloadData.workload && workloadData.workload[date]) {
        var dateWorkload = workloadData.workload[date];
        for (var slotKey in slotMapping) {
            var slotNum = slotMapping[slotKey].num;
            if (dateWorkload[slotNum]) {
                events.push({
                    date: date,
                    startTime: slotMapping[slotKey].start,
                    endTime: slotMapping[slotKey].end,
                    title: 'Занятость аудитории ' + roomName,
                    description: 'Аудитория занята в ' + slotKey,
                    location: roomName
                });
            }
        }
    }
    return events;
}

function activateRoomExport(workloadData, roomName) {
    if (typeof window.activateExportRoom === 'function') {
        window.activateExportRoom(workloadData, roomName);
    } else {
        window.lastWorkloadData = workloadData;
        window.lastWorkloadRoom = roomName;
        window.lastDataType = 'room';
        var exportBtn = document.getElementById('export-ical');
        if (exportBtn && workloadData && workloadData.workload) {
            exportBtn.style.display = 'inline-block';
            exportBtn.disabled = false;
        }
    }
}

window.exportRoomToIcal = function() {
    var events = [];
    if (window.lastWorkloadData && window.lastWorkloadData.workload) {
        for (var date in window.lastWorkloadData.workload) {
            var dateEvents = convertWorkloadToEvents(window.lastWorkloadData, window.lastWorkloadRoom, date);
            events.push.apply(events, dateEvents);
        }
    }
    if (events.length === 0) { alert('Нет данных для экспорта'); return; }

    var ical = 'BEGIN:VCALENDAR\r\n';
    ical += 'VERSION:2.0\r\n';
    ical += 'PRODID:-//UniSchedule//Rooms//RU\r\n';
    ical += 'CALSCALE:GREGORIAN\r\n';
    ical += 'X-WR-CALNAME:Загруженность ' + window.lastWorkloadRoom + '\r\n';
    ical += 'X-WR-TIMEZONE:Europe/Moscow\r\n';

    events.forEach(function(event) {
        var dateClean = event.date.replace(/-/g, '');
        var startTimeClean = event.startTime.replace(/:/g, '');
        var endTimeClean = event.endTime.replace(/:/g, '');
        var now = new Date();
        var nowStamp = now.getFullYear() +
            (now.getMonth() + 1).toString().padStart(2, '0') +
            now.getDate().toString().padStart(2, '0') + 'T' +
            now.getHours().toString().padStart(2, '0') +
            now.getMinutes().toString().padStart(2, '0') +
            now.getSeconds().toString().padStart(2, '0');

        ical += 'BEGIN:VEVENT\r\n';
        ical += 'UID:' + Date.now() + '-' + Math.random() + '@unischedule.ru\r\n';
        ical += 'DTSTAMP:' + nowStamp + '\r\n';
        ical += 'DTSTART;TZID=Europe/Moscow:' + dateClean + 'T' + startTimeClean + '00\r\n';
        ical += 'DTEND;TZID=Europe/Moscow:' + dateClean + 'T' + endTimeClean + '00\r\n';
        ical += 'SUMMARY:' + event.title + '\r\n';
        ical += 'DESCRIPTION:' + event.description + '\r\n';
        ical += 'LOCATION:' + event.location + '\r\n';
        ical += 'END:VEVENT\r\n';
    });
    ical += 'END:VCALENDAR\r\n';

    var blob = new Blob([ical], { type: 'text/calendar; charset=utf-8' });
    var url = URL.createObjectURL(blob);
    var a = document.createElement('a');
    a.href = url;
    a.download = 'room_' + window.lastWorkloadRoom + '_' + new Date().toISOString().slice(0, 10) + '.ics';
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
    alert('Экспортировано ' + events.length + ' записей о занятости в календарь!');
};