const autocomplete_list = document.getElementById('teacher-autocomplete-list');
const selected_list = document.getElementById('selected-teachers-list');
const teacher_input = document.getElementById('teacher-ids');
const date_start_obj = document.getElementById('start-date');
const date_end_obj = document.getElementById('end-date');
const results_obj = document.getElementById('results');

let teachers_list = [];
let selected_teachers = [];
let free_slots = [];

function search(name_part) {
    let _lower = name_part.toLowerCase().trim();
    return teachers_list.filter(t => {
        return t.teacher.toLowerCase().includes(_lower) &&
            !selected_teachers.some(sel => sel.teacher_id === t.teacher_id);
    }).slice(0, 10);
}

function updateAutocomplete(teachers) {
    if (teacher_input.value === '') {
        hideAutocomplete();
    }
    autocomplete_list.innerHTML = '';
    teachers.forEach(t => {
        const item = document.createElement('div');
        item.className = 'autocomplete-item';
        item.textContent = t.teacher;
        item.addEventListener('click', () => addTeacher(t));
        autocomplete_list.appendChild(item);
    });
}

function showAutocomplete() {
    autocomplete_list.classList.add('show');
}

function hideAutocomplete() {
    setTimeout(() => {
        autocomplete_list.classList.remove('show');
    }, 200);
}

function addTeacher(t) {
    if (selected_teachers.some(teacher => teacher.teacher_id === t.teacher_id)) return;
    selected_teachers.push(t);
    updateSelected();
    teacher_input.value = '';
    hideAutocomplete();
}

function removeTeacher(teacherId) {
    selected_teachers = selected_teachers.filter(t => t.teacher_id !== teacherId);
    updateSelected();
}

function onTeacherInput(e) {
    const searchText = e.target.value;
    const filtered = search(searchText);
    updateAutocomplete(filtered);
    showAutocomplete();
}

function updateSelected() {
    selected_list.innerHTML = '';
    selected_teachers.forEach(teacher => {
        const tag = document.createElement('div');
        tag.className = 'selected-teacher-tag';
        tag.innerHTML = `
        ${teacher.teacher}
        <button type="button" data-id="${teacher.teacher_id}">×</button>
    `;
        tag.querySelector('button').addEventListener('click', () => removeTeacher(teacher.teacher_id));
        selected_list.appendChild(tag);
    });
}

// ИСПРАВЛЕННАЯ ФУНКЦИЯ teachersSchedulesSearch
function teachersSchedulesSearch() {
    getSchedules().then((res) => {
        generateTables(res);
        
        // Активация кнопки экспорта
        if (typeof window.activateExport === 'function') {
            window.activateExport(res, 'Расписание преподавателей');
        }
    }).catch(error => {
        console.error('Ошибка загрузки расписания:', error);
        const messageBox = document.getElementById('message');
        if (messageBox) {
            messageBox.innerHTML = '<div class="error">Ошибка загрузки расписания. Проверьте подключение к интернету.</div>';
        }
    });
}

async function getSchedules() {
    let uids = [];
    selected_teachers.forEach(t => {
        uids.push(t.UID);
    });
    
    if (uids.length === 0) {
        alert('Пожалуйста, выберите преподавателей');
        return {};
    }
    
    free_slots = (await getTeachersFreeSlots(uids, date_start_obj.value, date_end_obj.value)).data;

    console.log(free_slots);
    const promises = selected_teachers.map(async t => {
        let sh = await getTeacherSchedule(t.UID, date_start_obj.value, date_end_obj.value);
        return sh.data.timetable;
    });

    const results = await Promise.all(promises);
    const flatResults = results.flat();

    const groupedByDate = {};

    flatResults.forEach(item => {
        const date = item.date;

        if (!groupedByDate[date]) {
            groupedByDate[date] = [];
        }

        groupedByDate[date].push(item);
    });

    return groupedByDate;
}

function generateTables(sch) {
    results_obj.innerHTML = '';
    for (let date in sch) {
        let table = getTableTemplate(date);

        const groupedByTeacher = {};
        sch[date].forEach(item => {
            const teacher = item.teacher;

            if (!groupedByTeacher[teacher]) {
                groupedByTeacher[teacher] = []
            }

            groupedByTeacher[teacher].push(item);
        });

        console.log(groupedByTeacher);

        for (let teacher in groupedByTeacher) {
            table.appendChild(getTableRow(teacher, groupedByTeacher[teacher], date));
        }

        results_obj.appendChild(table);
    }
}

function getTableRow(head, slots, date) {
    let final_row = document.createElement('tr');
    let nd = document.createElement('td');
    nd.textContent = head;
    final_row.appendChild(nd);
    ALL_SLOTS.forEach(slot => {
        let d = document.createElement('td');
        if (free_slots[date] && free_slots[date].includes(slot)) d.classList.add('free');
        slots.forEach(s => {
            if (s.slot === slot) {
                d.textContent = s.disciplines;
            }
        });
        final_row.appendChild(d);
    });
    return final_row;
}

function getTableTemplate(name) {
    let res = document.createElement('table');
    res.style.tableLayout = 'fixed';
    let tr_0 = document.createElement('tr');
    let tr_1 = document.createElement('tr');
    let fh = document.createElement('th');
    fh.colSpan = 8;
    fh.textContent = name;
    tr_0.appendChild(fh);
    res.appendChild(tr_0);
    tr_1.appendChild(document.createElement('th'));
    ALL_SLOTS.forEach(sl => {
        let sh = document.createElement('th');
        sh.textContent = sl;
        tr_1.appendChild(sh);
    });
    res.appendChild(tr_1);
    return res;
}

window.addEventListener('scroll', () => {
    autocomplete_list.style.top = `${teacher_input.getBoundingClientRect().bottom}px`;
});

document.addEventListener('DOMContentLoaded', async () => {
    try {
        let teachers_data = await getTeachersList();
        teachers_list = teachers_data.data.teachers;

        autocomplete_list.style.top = `${teacher_input.getBoundingClientRect().bottom}px`;
        autocomplete_list.style.width = teacher_input.offsetWidth.toString() + 'px';
        teacher_input.addEventListener('input', onTeacherInput);
    } catch (error) {
        console.error('Ошибка загрузки списка преподавателей:', error);
        const messageBox = document.getElementById('message');
        if (messageBox) {
            messageBox.innerHTML = '<div class="error">Ошибка загрузки списка преподавателей. Проверьте подключение к интернету.</div>';
        }
    }
});