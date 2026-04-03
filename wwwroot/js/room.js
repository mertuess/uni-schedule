const buildings_obj = document.getElementById('buildings');
const rooms_obj = document.getElementById('rooms');
const autocomplete_list = document.getElementById('buildings-autocomplete-list');
const selected_list = document.getElementById('selected-buildings-list');
const buildings_input = document.getElementById('buildings-ids');
const date_start_obj = document.getElementById('start-date');
const date_end_obj = document.getElementById('end-date');
const results = document.getElementById('results');

let buildings = []
let rooms = []
let selected_buildings = []

function updateBuildings() {
  buildings_obj.innerHTML = '';
  buildings.forEach(x => {
    const opt = document.createElement('option');
    opt.value = x.bui_id;
    opt.textContent = x.building;
    buildings_obj.appendChild(opt);
  });
}

function updateRooms() {
  rooms_obj.innerHTML = '';
  rooms.forEach(x => {
    const opt = document.createElement('option');
    opt.value = x.room_id;
    opt.textContent = x.room;
    rooms_obj.appendChild(opt);
  });
}

function search(name_part) {
  let _lower = name_part.toLowerCase().trim();
  return buildings.filter(t => {
    return t.building.toLowerCase().includes(_lower) &&
      !selected_buildings.some(sel => sel.bui_id === t.bui_id);
  }).slice(0, 10);
}

function updateAutocomplete(buildings) {
  if (buildings_input.value == '') {
    hideAutocomplete();
  }
  autocomplete_list.innerHTML = '';
  buildings.forEach(t => {
    const item = document.createElement('div');
    item.className = 'autocomplete-item';
    item.textContent = t.building;
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
  if (selected_buildings.some(teacher => teacher.bui_id === t.bui_id)) return;
  selected_buildings.push(t);
  updateSelected();
  buildings_input.value = '';
  hideAutocomplete();
}

function removeTeacher(building_id) {
  selected_buildings = selected_buildings.filter(t => t.bui_id !== building_id);
  updateSelected();
}

function onBuildingsInput(e) {
  const searchText = e.target.value;
  const filtered = search(searchText);
  updateAutocomplete(filtered);
  showAutocomplete();
}

function updateSelected() {
  selected_list.innerHTML = '';
  selected_buildings.forEach(teacher => {
    const tag = document.createElement('div');
    tag.className = 'selected-teacher-tag';
    tag.innerHTML = `
        ${teacher.building}
        <button type="button" data-id="${teacher.bui_id}">×</button>
    `;
    tag.querySelector('button').addEventListener('click', () => removeTeacher(teacher.bui_id));
    selected_list.appendChild(tag);
  });
}

async function showRoomWorkload() {
  results.innerHTML = '';
  let wl = (await getRoomWorkload(rooms_obj.value, date_start_obj.value, date_end_obj.value)).data;
  console.log(wl);
  if (!wl) results.innerHTML = 'Данным по загруженности нет';

  let dates = Object.keys(wl.workload).sort();

  if (dates.length === 0) {
    results.innerHTML = 'Нет данных о загруженности';
    return;
  }

  let pt = getPercentTable([wl]);
  results.appendChild(pt);

  for (let date of dates) {
    let table = getTableTemplate(date);
    let row = getTableRow(wl.room, wl.workload[date]);
    table.appendChild(row);
    results.appendChild(table);
    results.appendChild(document.createElement('br'));
  }
}

async function showBuildingWorkload() {
  results.innerHTML = '';
  let wl = (await getBuildingWorkload(buildings_obj.value, date_start_obj.value, date_end_obj.value)).data;
  if (!wl) results.innerHTML = 'Данным по загруженности нет';

  let pt = getPercentTable(wl.workload);
  results.appendChild(pt);

  let allDates = new Set();
  for (let room of wl.workload) {
    for (let date in room.workload) {
      allDates.add(date);
    }
  }

  let sortedDates = Array.from(allDates).sort();

  for (let date of sortedDates) {
    let table = getTableTemplate(date);

    for (let room of wl.workload) {
      if (room.workload[date]) {
        let row = getTableRow(room.room, room.workload[date]);
        table.appendChild(row);
      } else {
        let row = getEmptyTableRow(room.room);
        table.appendChild(row);
      }
    }

    results.appendChild(table);
    results.appendChild(document.createElement('br'));
  }
}

function getEmptyTableRow(room) {
  let r = document.createElement('tr');
  let fd = document.createElement('td');
  fd.textContent = room;
  r.appendChild(fd);

  for (let i = 1; i <= 7; i++) {
    let d = document.createElement('td');
    d.textContent = '—';
    r.appendChild(d);
  }
  return r;
}

function getTableRow(room, dateWorkload) {
  let r = document.createElement('tr');
  let fd = document.createElement('td');
  fd.textContent = room;
  r.appendChild(fd);

  for (let slot = 1; slot <= 7; slot++) {
    let d = document.createElement('td');
    if (dateWorkload[slot]) {
      d.textContent = 'Занятие';
    } else {
      d.textContent = '—';
    }
    r.appendChild(d);
  }
  return r;
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
  let wl_td = document.createElement('th');
  tr_1.appendChild(wl_td);
  ALL_SLOTS.forEach(sl => {
    let sh = document.createElement('th');
    sh.textContent = sl;
    tr_1.appendChild(sh);
  });
  res.appendChild(tr_1);
  return res;
}

function getPercentTable(wl_arr) {
  let table = document.createElement('table');
  let tr_0 = document.createElement('tr');
  let th_0 = document.createElement('th');
  let th_1 = document.createElement('th');
  th_0.textContent = 'Аудитория';
  th_1.textContent = 'Загруженность';
  tr_0.appendChild(th_0);
  tr_0.appendChild(th_1);
  table.appendChild(tr_0);
  for (let wl in wl_arr) {
    let row = document.createElement('tr');
    let room_cell = document.createElement('td');
    let wl_cell = document.createElement('td');
    room_cell.textContent = wl_arr[wl].room;
    wl_cell.textContent = `${wl_arr[wl].workload_percent}%`;
    row.appendChild(room_cell);
    row.appendChild(wl_cell);
    table.appendChild(row);
  }
  return table;
}

buildings_obj.addEventListener("change", async (event) => {
  rooms = (await getRooms(buildings[event.target.value - 3].bui_id.toString())).data.rooms;
  updateRooms();
});

document.addEventListener('DOMContentLoaded', async () => {
  buildings = (await getBuildings()).data.buildings.slice(2);
  rooms = (await getRooms(buildings[0].bui_id.toString())).data.rooms;
  updateBuildings();
  updateRooms();

  autocomplete_list.style.top = `${buildings_input.getBoundingClientRect().bottom}px`;
  autocomplete_list.style.width = buildings_input.offsetWidth.toString() + 'px';
  buildings_input.addEventListener('input', onBuildingsInput);
});

window.addEventListener('scroll', () => {
  autocomplete_list.style.top = `${buildings_input.getBoundingClientRect().bottom}px`;
});

