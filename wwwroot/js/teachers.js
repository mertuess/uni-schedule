const autocomplete_list = document.getElementById('teacher-autocomplete-list');
const selected_list = document.getElementById('selected-teachers-list');
const teacher_input = document.getElementById('teacher-ids');
const date_start_obj = document.getElementById('start-date');
const date_end_obj = document.getElementById('end-date');
const results_obj = document.getElementById('results');

let teachers_list = [];
let selected_teachers = [];
let free_slots = {};
let teacherBindings = {};

async function loadTeacherBindings() {
    try {
        teacherBindings = await getTeacherBindings();
    } catch (e) {
        console.warn('Не удалось загрузить привязки:', e);
        teacherBindings = {};
    }
}

function search(name_part) {
    if (!Array.isArray(teachers_list)) return [];
    var _lower = name_part.toLowerCase().trim();
    return teachers_list.filter(function(t) {
        var teacherName = t.teacher || t.name || t.fullName || '';
        var teacherId = t.UID || t.uid || t.id || t.teacher_id;
        return teacherName.toLowerCase().includes(_lower) &&
            !selected_teachers.some(function(sel) { return sel.UID === teacherId; });
    }).slice(0, 10);
}

function updateAutocomplete(teachers) {
    if (!autocomplete_list) return;
    if (teacher_input && teacher_input.value === '') {
        hideAutocomplete();
        return;
    }
    autocomplete_list.innerHTML = '';
    teachers.forEach(function(t) {
        var item = document.createElement('div');
        item.className = 'autocomplete-item';
        
        var uid = t.UID || t.uid || t.id || t.teacher_id;
        var binding = teacherBindings[uid];
        var bindingBadge = binding 
            ? `<span class="binding-badge" title="Кафедра: ${binding.departmentName}">${binding.departmentName}</span>` 
            : '';
        
        item.innerHTML = `<span class="teacher-name">${t.teacher || t.name || ''}</span>${bindingBadge}`;
        item.addEventListener('click', function() { addTeacher(t); });
        autocomplete_list.appendChild(item);
    });
}

function showAutocomplete() {
    if (autocomplete_list) autocomplete_list.classList.add('show');
}
function hideAutocomplete() {
    setTimeout(function() {
        if (autocomplete_list) autocomplete_list.classList.remove('show');
    }, 200);
}

function addTeacher(t) {
    var uid = t.UID || t.uid || t.id || t.teacher_id;
    var name = t.teacher || t.name || t.fullName || '';
    var teacherId = t.teacher_id || t.id || uid;
    
    if (!uid) return;
    if (selected_teachers.some(function(teacher) { return teacher.UID === uid; })) return;
    
    selected_teachers.push({ UID: uid, teacher_id: teacherId, teacher: name });
    updateSelected();
    if (teacher_input) teacher_input.value = '';
    hideAutocomplete();
}

function removeTeacher(teacherId) {
    selected_teachers = selected_teachers.filter(function(t) { 
        return t.UID !== teacherId && t.teacher_id !== teacherId; 
    });
    updateSelected();
}

function onTeacherInput(e) {
    var searchText = e.target.value;
    var filtered = search(searchText);
    updateAutocomplete(filtered);
    showAutocomplete();
}

function updateSelected() {
    if (!selected_list) return;
    selected_list.innerHTML = '';
    selected_teachers.forEach(function(teacher) {
        var teacherId = teacher.UID || teacher.teacher_id;
        var tag = document.createElement('div');
        tag.className = 'selected-teacher-tag';
        
        var binding = teacherBindings[teacherId];
        var bindingBadge = binding 
            ? `<span class="binding-badge-small" title="Кафедра: ${binding.departmentName}"> ${binding.departmentName}</span>` 
            : '';
        
        tag.innerHTML = `<span class="selected-teacher-name">${teacher.teacher}</span>${bindingBadge}<button type="button" data-id="${teacherId}">×</button>`;
        var btn = tag.querySelector('button');
        if (btn) btn.addEventListener('click', function() { removeTeacher(teacherId); });
        selected_list.appendChild(tag);
    });
}

function teachersSchedulesSearch() {
    if (!selected_teachers || selected_teachers.length === 0) {
        if (results_obj) results_obj.innerHTML = '<div class="error">Пожалуйста, выберите хотя бы одного преподавателя</div>';
        return;
    }
    var start = date_start_obj ? date_start_obj.value : null;
    var end = date_end_obj ? date_end_obj.value : null;
    if (!start || !end) {
        if (results_obj) results_obj.innerHTML = '<div class="error">Пожалуйста, укажите период дат</div>';
        return;
    }

    getSchedules().then(function(res) {
        if (res && Object.keys(res).length > 0) {
            generateTables(res);
            if (typeof window.activateExport === 'function') {
                window.activateExport(res, 'Расписание преподавателей');
            }
        } else {
            if (results_obj) results_obj.innerHTML = '<div class="info">Нет данных для отображения</div>';
        }
    }).catch(function(error) {
        console.error('Ошибка загрузки расписания:', error);
        if (results_obj) results_obj.innerHTML = '<div class="error">Ошибка загрузки расписания</div>';
    });
}

async function getSchedules() {
    var uids = [];
    selected_teachers.forEach(function(t) { if (t && t.UID) uids.push(t.UID); });
    if (uids.length === 0) return {};

    var start = date_start_obj ? date_start_obj.value : null;
    var end = date_end_obj ? date_end_obj.value : null;
    if (!start || !end) return {};

    try {
        var freeSlotsResponse = await getTeachersFreeSlots(uids, start, end);
        if (freeSlotsResponse && freeSlotsResponse.success && freeSlotsResponse.data) {
            free_slots = freeSlotsResponse.data;
        }

        var promises = selected_teachers.map(async function(t) {
            if (!t || !t.UID) return [];
            var scheduleResponse = await getTeacherSchedule(t.UID, start, end);
            if (scheduleResponse && scheduleResponse.success && scheduleResponse.data) {
                var scheduleData = scheduleResponse.data;
                if (scheduleData && scheduleData.timetable && Array.isArray(scheduleData.timetable)) {
                    return scheduleData.timetable;
                }
                if (scheduleData && scheduleData.schedule && Array.isArray(scheduleData.schedule)) {
                    return scheduleData.schedule;
                }
                if (Array.isArray(scheduleData)) return scheduleData;
                if (scheduleData && typeof scheduleData === 'object') return [scheduleData];
            }
            return [];
        });

        var results = await Promise.all(promises);
        var flatResults = [];
        for (var i = 0; i < results.length; i++) {
            if (Array.isArray(results[i])) flatResults = flatResults.concat(results[i]);
        }

        var groupedByDate = {};
        flatResults.forEach(function(item) {
            if (!item || !item.date) return;
            var date = item.date;
            if (!groupedByDate[date]) groupedByDate[date] = [];
            groupedByDate[date].push(item);
        });
        return groupedByDate;
    } catch (error) {
        console.error('Ошибка в getSchedules:', error);
        return {};
    }
}

function generateTables(sch) {
    if (!results_obj) return;
    results_obj.innerHTML = '';
    if (!sch || Object.keys(sch).length === 0) {
        results_obj.innerHTML = '<div class="info">Расписание не найдено на выбранный период</div>';
        return;
    }

    for (var date in sch) {
        var table = getTableTemplate(date);
        var dayItems = sch[date];
        if (!Array.isArray(dayItems)) continue;

        var groupedByTeacher = {};
        dayItems.forEach(function(item) {
            if (!item || !item.teacher) return;
            var teacher = item.teacher;
            if (!groupedByTeacher[teacher]) groupedByTeacher[teacher] = [];
            groupedByTeacher[teacher].push(item);
        });

        for (var teacher in groupedByTeacher) {
            var uid = selected_teachers.find(t => t.teacher === teacher)?.UID;
            var binding = uid ? teacherBindings[uid] : null;
            var bindingBadge = binding ? ` <span class="binding-badge-table"> ${binding.departmentName}</span>` : '';
            
            table.appendChild(getTableRow(teacher + bindingBadge, groupedByTeacher[teacher], date));
        }
        results_obj.appendChild(table);
        results_obj.appendChild(document.createElement('br'));
    }
}

function getTableRow(head, slots, date) {
    var final_row = document.createElement('tr');
    var nd = document.createElement('td');
    nd.innerHTML = head; 
    final_row.appendChild(nd);

    ALL_SLOTS.forEach(function(slot) {
        var d = document.createElement('td');
        if (free_slots && free_slots[date] && Array.isArray(free_slots[date]) && free_slots[date].includes(slot)) {
            d.classList.add('free');
        }
        slots.forEach(function(s) {
            if (s && s.slot === slot) d.textContent = s.disciplines || '';
        });
        final_row.appendChild(d);
    });
    return final_row;
}

function getTableTemplate(name) {
    var res = document.createElement('table');
    res.style.tableLayout = 'fixed';
    var tr_0 = document.createElement('tr');
    var tr_1 = document.createElement('tr');
    var fh = document.createElement('th');
    fh.colSpan = 8;
    fh.innerHTML = name; 
    tr_0.appendChild(fh);
    res.appendChild(tr_0);
    tr_1.appendChild(document.createElement('th'));
    ALL_SLOTS.forEach(function(sl) {
        var sh = document.createElement('th');
        sh.textContent = sl;
        tr_1.appendChild(sh);
    });
    res.appendChild(tr_1);
    return res;
}

window.addEventListener('scroll', function() {
    if (autocomplete_list && teacher_input) {
        autocomplete_list.style.top = teacher_input.getBoundingClientRect().bottom + 'px';
    }
});

document.addEventListener('DOMContentLoaded', async function() {
    if (!teacher_input || !autocomplete_list) return;
    
    try {
        await Promise.all([
            (async () => {
                var teachers_data = await getTeachersList();
                if (teachers_data && teachers_data.success && teachers_data.data) {
                    var data = teachers_data.data;
                    if (Array.isArray(data)) teachers_list = data;
                    else if (data && data.teachers && Array.isArray(data.teachers)) teachers_list = data.teachers;
                }
            })(),
            loadTeacherBindings()
        ]);
        
        autocomplete_list.style.top = teacher_input.getBoundingClientRect().bottom + 'px';
        autocomplete_list.style.width = teacher_input.offsetWidth.toString() + 'px';
        teacher_input.addEventListener('input', onTeacherInput);
    } catch (error) {
        console.error('Ошибка загрузки данных:', error);
        teachers_list = [];
        teacherBindings = {};
    }
});