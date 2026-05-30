// ┌────────────────────────────────────────────────────────────────────────────┐
// │ UniSchedule - Frontend API Client                                          │
// │ Все запросы идут только на наш бэкенд через /api/*                         │
// └────────────────────────────────────────────────────────────────────────────┘

const API_BASE = '/api';

const ALL_SLOTS = [
    "09:00 - 10:35",
    "10:45 - 12:20",
    "12:40 - 14:15",
    "14:45 - 16:20",
    "16:30 - 18:05",
    "18:15 - 19:50",
    "20:00 - 21:35"
];

// Авторизация
let authEmail = null;
let authPassword = null;

try {
    const emailCookie = getCookie("Uni-Email");
    const passCookie = getCookie("Uni-Password");
    if (emailCookie && passCookie) {
        authEmail = emailCookie;
        authPassword = passCookie;
    }
} catch (e) {
    console.warn("Ошибка чтения cookie авторизации:", e);
}

function getHeaders() {
    const headers = { 'Content-Type': 'application/json' };
    if (authEmail && authPassword) {
        headers['Uni-Email'] = authEmail;
        headers['Uni-Password'] = authPassword;
    }
    return headers;
}

async function apiGet(endpoint) {
    try {
        const response = await fetch(API_BASE + endpoint, {
            method: 'GET',
            headers: getHeaders()
        });

        if (response.status === 401)
            throw new Error('Неверные учетные данные');
        if (response.status === 403)
            throw new Error('Нет прав доступа');
        if (!response.ok)
            throw new Error('Ошибка ' + response.status);

        const data = await response.json();
        return { success: true, data: data };
        
    } catch (error) {
        console.error('API Error:', error);
        return { success: false, error: error.message };
    }
}

function setAuth(email, password) {
    authEmail = email;
    authPassword = password;
    setCookie("Uni-Email", email, 90);
    setCookie("Uni-Password", password, 90);
}

function clearAuth() {
    authEmail = null;
    authPassword = null;
    setCookie("Uni-Email", "", -1);
    setCookie("Uni-Password", "", -1);
}

// Cookie helper
function setCookie(cname, cvalue, exdays) {
    const d = new Date();
    d.setTime(d.getTime() + (exdays * 24 * 60 * 60 * 1000));
    const expires = "expires=" + d.toUTCString();
    document.cookie = cname + "=" + cvalue + ";" + expires + ";path=/";
}

function getCookie(cname) {
    const name = cname + "=";
    const cookies = document.cookie.split(';');
    for (let c of cookies) {
        c = c.trim();
        if (c.indexOf(name) === 0) {
            return c.substring(name.length);
        }
    }
    return null;
}

// Пользователи
async function getUsers() { return await apiGet('/Database/users'); }
async function createUser(email, password, name, engName, role) {
    const params = new URLSearchParams({ email, password, name, engName, role });
    return await apiGet('/Database/users/create?' + params);
}
async function deleteUser(email) {
    return await apiGet('/Database/users/' + encodeURIComponent(email) + '/remove');
}
async function updateUser(email, newEmail, newPassword, newName, newEngName, newRole, department) {
    const params = new URLSearchParams();
    if (newEmail) params.append('new_email', newEmail);
    if (newPassword) params.append('new_password', newPassword);
    if (newName) params.append('new_name', newName);
    if (newEngName) params.append('new_engName', newEngName);
    if (newRole) params.append('new_role', newRole);
    if (department !== null && department !== undefined) params.append('department', department);
    return await apiGet('/Database/users/' + encodeURIComponent(email) + '/update?' + params);
}
async function getUserRole(email) {
    return await apiGet('/Database/users/' + encodeURIComponent(email) + '/role');
}
async function tryAuth(email, password) {
    return await apiGet('/Database/tryauth?email=' + encodeURIComponent(email) + '&password=' + encodeURIComponent(password));
}

// Кафедры
async function getDepartments() { return await apiGet('/Database/departments'); }
async function createDepartment(name) {
    return await apiGet('/Database/departments/create?name=' + encodeURIComponent(name));
}
async function updateDepartment(name, newName) {
    return await apiGet('/Database/departments/' + encodeURIComponent(name) + '/update?new_name=' + encodeURIComponent(newName));
}
async function deleteDepartment(name) {
    return await apiGet('/Database/departments/' + encodeURIComponent(name) + '/remove');
}
async function getUsersByDepartment(departmentId) {
    return await apiGet('/Database/departments/' + departmentId + '/users');
}

// Корпуса и аудитории
async function getBuildings() { return await apiGet('/buildings'); }
async function getRooms(buildingId) {
    return await apiGet('/Rooms/' + buildingId + '/rooms');
}
async function getRoomWorkload(roomId, start, end) {
    return await apiGet('/Rooms/' + roomId + '/workload/' + start + '/' + end);
}
async function getBuildingWorkload(buildingId, start, end) {
    return await apiGet('/Buildings/' + buildingId + '/workload/' + start + '/' + end);
}
async function getBuildingsWorkload(start, end, buildingIds) {
    const ids = Array.isArray(buildingIds) ? buildingIds.join(',') : buildingIds;
    return await apiGet('/Buildings/workload/' + start + '/' + end + '?bui_ids=' + ids);
}

// Преподаватели
async function getTeachersList() { return await apiGet('/teachers'); }
async function searchTeachers(query) {
    return await apiGet('/teachers/search?query=' + encodeURIComponent(query));
}
async function getTeacherSchedule(teacherUid, start, end) {
    return await apiGet('/teachers/' + encodeURIComponent(teacherUid) + '/schedule/' + encodeURIComponent(start) + '/' + encodeURIComponent(end));
}

// Расписание
async function getTeachersFreeSlots(teacherUIDs, start, end) {
    const uids = Array.isArray(teacherUIDs) ? teacherUIDs.join(',') : teacherUIDs;
    return await apiGet('/Schedule/teachers/' + uids + '/' + start + '/' + end);
}

// iCal
async function getAvailableGroups() { return await apiGet('/calendar/groups'); }
async function exportGroupToIcal(groupName) {
    return await apiGet('/calendar/group/' + encodeURIComponent(groupName));
}
async function exportTeacherToIcal(teacherName) {
    return await apiGet('/calendar/teacher/' + encodeURIComponent(teacherName));
}
async function getGroupSubscriptionUrl(groupName) {
    return await apiGet('/calendar/subscribe/group/' + encodeURIComponent(groupName));
}
async function getTestIcal(simple) {
    const endpoint = simple ? '/calendar/test/simple' : '/calendar/test/download';
    return await apiGet(endpoint);
}

// Экспорт функций
window.UniAPI = {
    setAuth, clearAuth, getHeaders,
    getUsers, createUser, deleteUser, updateUser, getUserRole, tryAuth,
    getDepartments, createDepartment, updateDepartment, deleteDepartment, getUsersByDepartment,
    getBuildings, getRooms, getRoomWorkload, getBuildingWorkload, getBuildingsWorkload,
    getTeachersList, searchTeachers, getTeacherSchedule,
    getTeachersFreeSlots,
    getAvailableGroups, exportGroupToIcal, exportTeacherToIcal, getGroupSubscriptionUrl, getTestIcal,
    apiGet, ALL_SLOTS
};