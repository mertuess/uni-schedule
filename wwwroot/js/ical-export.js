// iCal экспорт для расписания
let lastScheduleData = null;
let lastDepartmentName = '';

// Активация кнопок после загрузки данных
window.activateExport = function (data, departmentName) {
    console.log('Активация экспорта, дней:', Object.keys(data).length);
    lastScheduleData = data;
    lastDepartmentName = departmentName || 'Расписание';

    const hasData = data && Object.keys(data).length > 0;
    const exportBtn = document.getElementById('export-ical');

    if (exportBtn && hasData) {
        exportBtn.style.display = 'inline-block';
        exportBtn.disabled = false;
        console.log('Кнопка экспорта активирована');
    }
};

// Преобразование данных в события
function convertToEvents(scheduleData) {
    const events = [];

    // Маппинг слотов на время (ALL_SLOTS из api.js)
    const slotTimes = {
        '1': {start: '09:00', end: '10:35'},
        '2': {start: '10:45', end: '12:20'},
        '3': {start: '12:40', end: '14:15'},
        '4': {start: '14:45', end: '16:20'},
        '5': {start: '16:30', end: '18:05'},
        '6': {start: '18:15', end: '19:50'}
    };

    for (let date in scheduleData) {
        const dayEvents = scheduleData[date];

        dayEvents.forEach(item => {
            const slotInfo = slotTimes[item.slot] || {start: '09:00', end: '10:35'};

            events.push({
                date: date,
                startTime: slotInfo.start,
                endTime: slotInfo.end,
                slot: item.slot,
                discipline: item.disciplines || 'Занятие',
                teacher: item.teacher || 'Не указан',
                room: item.room || 'Не указана',
                type: item.type || 'Занятие'
            });
        });
    }

    return events;
}

// Скачать .ics файл
window.exportToIcal = function () {
    if (!lastScheduleData || Object.keys(lastScheduleData).length === 0) {
        alert('Нет данных для экспорта. Сначала выполните поиск.');
        return;
    }

    const events = convertToEvents(lastScheduleData);

    if (events.length === 0) {
        alert('Нет событий для экспорта');
        return;
    }

    // Генерируем iCal файл
    let ical = 'BEGIN:VCALENDAR\r\n';
    ical += 'VERSION:2.0\r\n';
    ical += 'PRODID:-//UniSchedule//Schedule//RU\r\n';
    ical += 'CALSCALE:GREGORIAN\r\n';
    ical += 'METHOD:PUBLISH\r\n';
    ical += `X-WR-CALNAME:${lastDepartmentName}\r\n`;
    ical += 'X-WR-TIMEZONE:Europe/Moscow\r\n';
    ical += 'BEGIN:VTIMEZONE\r\n';
    ical += 'TZID:Europe/Moscow\r\n';
    ical += 'X-LIC-LOCATION:Europe/Moscow\r\n';
    ical += 'BEGIN:STANDARD\r\n';
    ical += 'TZOFFSETFROM:+0300\r\n';
    ical += 'TZOFFSETTO:+0300\r\n';
    ical += 'TZNAME:MSK\r\n';
    ical += 'DTSTART:19700101T000000\r\n';
    ical += 'END:STANDARD\r\n';
    ical += 'END:VTIMEZONE\r\n';

    events.forEach(event => {
        const dateClean = event.date.replace(/-/g, '');
        const startTimeClean = event.startTime.replace(/:/g, '');
        const endTimeClean = event.endTime.replace(/:/g, '');
        const now = new Date();
        const nowStamp = `${now.getFullYear()}${(now.getMonth() + 1).toString().padStart(2, '0')}${now.getDate().toString().padStart(2, '0')}T${now.getHours().toString().padStart(2, '0')}${now.getMinutes().toString().padStart(2, '0')}${now.getSeconds().toString().padStart(2, '0')}`;

        ical += 'BEGIN:VEVENT\r\n';
        ical += `UID:${Date.now()}-${Math.random()}@unischedule.ru\r\n`;
        ical += `DTSTAMP:${nowStamp}\r\n`;
        ical += `DTSTART;TZID=Europe/Moscow:${dateClean}T${startTimeClean}00\r\n`;
        ical += `DTEND;TZID=Europe/Moscow:${dateClean}T${endTimeClean}00\r\n`;
        ical += `SUMMARY:${event.discipline}\r\n`;
        ical += `DESCRIPTION:Преподаватель: ${event.teacher}\\nТип: ${event.type}\\nПара: ${event.slot}\r\n`;
        ical += `LOCATION:Аудитория ${event.room}\r\n`;
        ical += 'END:VEVENT\r\n';
    });

    ical += 'END:VCALENDAR\r\n';

    // Скачиваем файл
    const blob = new Blob([ical], {type: 'text/calendar; charset=utf-8'});
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `schedule_${lastDepartmentName.replace(/[^a-zа-яё0-9]/gi, '_')}_${new Date().toISOString().slice(0, 10)}.ics`;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);

    alert(`Экспортировано ${events.length} занятий в файл календаря!`);
};

console.log('iCal экспорт загружен');

// Добавьте в конец файла ical-export.js:

// Специальная функция для аудиторий
window.activateExportRoom = function (workloadData, roomName) {
    console.log('Активация экспорта для аудитории:', roomName);
    window.lastWorkloadData = workloadData;
    window.lastWorkloadRoom = roomName;
    window.lastDataType = 'room';

    const exportBtn = document.getElementById('export-ical');
    if (exportBtn && workloadData && workloadData.workload && Object.keys(workloadData.workload).length > 0) {
        exportBtn.style.display = 'inline-block';
        exportBtn.disabled = false;
        console.log('Кнопка экспорта для аудитории активирована');
    }
};

// Переопределяем exportToIcal для поддержки аудиторий
const originalExportToIcal = window.exportToIcal;
window.exportToIcal = function () {
    originalExportToIcal();
};

function exportRoomToIcal() {
    const events = [];
    const slotMapping = {
        '1': {start: '09:00', end: '10:35', slotText: '09:00 - 10:35'},
        '2': {start: '10:45', end: '12:20', slotText: '10:45 - 12:20'},
        '3': {start: '12:40', end: '14:15', slotText: '12:40 - 14:15'},
        '4': {start: '14:45', end: '16:20', slotText: '14:45 - 16:20'},
        '5': {start: '16:30', end: '18:05', slotText: '16:30 - 18:05'},
        '6': {start: '18:15', end: '19:50', slotText: '18:15 - 19:50'},
        '7': {start: '20:00', end: '21:35', slotText: '20:00 - 21:35'}
    };

    for (let date in window.lastWorkloadData.workload) {
        const dateWorkload = window.lastWorkloadData.workload[date];
        for (let slotNum in slotMapping) {
            if (dateWorkload[slotNum]) {
                events.push({
                    date: date,
                    startTime: slotMapping[slotNum].start,
                    endTime: slotMapping[slotNum].end,
                    title: `Занятость аудитории ${window.lastWorkloadRoom}`,
                    description: `Аудитория занята в ${slotMapping[slotNum].slotText}`,
                    location: window.lastWorkloadRoom
                });
            }
        }
    }

    if (events.length === 0) {
        alert('Нет данных о занятости для экспорта');
        return;
    }

    let ical = 'BEGIN:VCALENDAR\r\n';
    ical += 'VERSION:2.0\r\n';
    ical += 'PRODID:-//UniSchedule//Rooms//RU\r\n';
    ical += 'CALSCALE:GREGORIAN\r\n';
    ical += `X-WR-CALNAME:Загруженность ${window.lastWorkloadRoom}\r\n`;
    ical += 'X-WR-TIMEZONE:Europe/Moscow\r\n';

    events.forEach(event => {
        const dateClean = event.date.replace(/-/g, '');
        const startClean = event.startTime.replace(/:/g, '');
        const endClean = event.endTime.replace(/:/g, '');
        const now = new Date();
        const nowStamp = `${now.getFullYear()}${(now.getMonth() + 1).toString().padStart(2, '0')}${now.getDate().toString().padStart(2, '0')}T${now.getHours().toString().padStart(2, '0')}${now.getMinutes().toString().padStart(2, '0')}${now.getSeconds().toString().padStart(2, '0')}`;

        ical += 'BEGIN:VEVENT\r\n';
        ical += `UID:${Date.now()}-${Math.random()}@unischedule.ru\r\n`;
        ical += `DTSTAMP:${nowStamp}\r\n`;
        ical += `DTSTART;TZID=Europe/Moscow:${dateClean}T${startClean}00\r\n`;
        ical += `DTEND;TZID=Europe/Moscow:${dateClean}T${endClean}00\r\n`;
        ical += `SUMMARY:${event.title}\r\n`;
        ical += `DESCRIPTION:${event.description}\r\n`;
        ical += `LOCATION:${event.location}\r\n`;
        ical += 'END:VEVENT\r\n';
    });

    ical += 'END:VCALENDAR\r\n';

    const blob = new Blob([ical], {type: 'text/calendar; charset=utf-8'});
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `room_${window.lastWorkloadRoom}_${new Date().toISOString().slice(0, 10)}.ics`;
    a.click();
    URL.revokeObjectURL(url);

    alert(`Экспортировано ${events.length} записей о занятости!`);
}