// Базовый URL для API
const API_BASE = 'http://localhost:5000/api';
const OAPI_BASE = 'https://api-schedule.mauniver.ru';
const ALL_SLOTS = [
    "09:00 - 10:35",
    "10:45 - 12:20",
    "12:40 - 14:15",
    "14:45 - 16:20",
    "16:30 - 18:05",
    "18:15 - 19:50",
    "20:00 - 21:35"
];

// Хранилище для токена авторизации
//let authEmail = getCookie("Uni-Email") || null;
//let authPassword = getCookie("Uni-Password") || null;

// Хранилище для токена авторизации
let authEmail = null;
let authPassword = null;

// Инициализация из cookie после загрузки
try {
    const emailCookie = getCookie("Uni-Email");
    const passCookie = getCookie("Uni-Password");
    if (emailCookie && passCookie) {
        authEmail = emailCookie;
        authPassword = passCookie;
    }
} catch (e) {
    console.log("Ошибка чтения cookie:", e);
    authEmail = null;
    authPassword = null;
}

// Функция получения заголовков
function getHeaders() {
    return {
        'Uni-Email': authEmail,
        'Uni-Password': authPassword,
        'Content-Type': 'application/json'
    };
}

function getOApiHeaders() {
    return {
        'accept': '*/*',
        'Authorization': 'Bearer ' + getCookie('o-api-token')
    };
}

// Базовая функция для GET запросов
async function apiGet(endpoint) {
    try {
        const response = await fetch(`${API_BASE}${endpoint}`, {
            method: 'GET',
            headers: getHeaders()
        });

        if (response.status === 401)
            throw new Error('Неверные учетные данные');

        if (response.status === 403)
            throw new Error('Нет прав доступа');

        if (!response.ok)
            throw new Error(`Ошибка ${response.status}`);

        const data = await response.json();
        return {success: true, data};
    } catch (error) {
        console.error('API Error:', error);
        return {success: false, error: error.message};
    }
}

async function oApiGet(endpoint) {
    try {
        const response = await fetch(`${OAPI_BASE}${endpoint}`, {
            method: 'GET',
            headers: getOApiHeaders()
        });

        if (response.status === 401)
            throw new Error('Неверные учетные данные');

        if (response.status === 403)
            throw new Error('Нет прав доступа');

        if (!response.ok)
            throw new Error(`Ошибка ${response.status}`);

        const data = await response.json();
        return {success: true, data};
    } catch (error) {
        console.error('API Error:', error);
        return {success: false, error: error.message};
    }
}

// Функция установки учетных данных
function setAuth(email, password) {
    authEmail = email;
    authPassword = password;
    setCookie("Uni-Email", email, 90);
    setCookie("Uni-Password", password, 90);
}

// Функция очистки учетных данных
function clearAuth() {
    authEmail = null;
    authPassword = null;
    setCookie("Uni-Email", "", -1);
    setCookie("Uni-Password", "", -1);
}

// ============== 1. РАБОТА С ПОЛЬЗОВАТЕЛЯМИ ==============

// 1.1 Получить всех пользователей
// GET /api/Database/users
async function getUsers() {
    return await apiGet('/Database/users');
}

// 1.2 Создать пользователя
// GET /api/Database/users/create?email=&password=&name=&engName=&role=
async function createUser(email, password, name, engName, role) {
    const params = new URLSearchParams({
        email, password, name, engName, role
    });
    return await apiGet(`/Database/users/create?${params}`);
}

// 1.3 Удалить пользователя
// GET /api/Database/users/{email}/remove
async function deleteUser(email) {
    return await apiGet(`/Database/users/${encodeURIComponent(email)}/remove`);
}

// 1.4 Обновить пользователя
// GET /api/Database/users/{email}/update?new_email=&new_password=&new_name=&new_engName=&new_role=&department=
async function updateUser(email, newEmail, newPassword, newName, newEngName, newRole, department) {
    const params = new URLSearchParams();
    if (newEmail) params.append('new_email', newEmail);
    if (newPassword) params.append('new_password', newPassword);
    if (newName) params.append('new_name', newName);
    if (newEngName) params.append('new_engName', newEngName);
    if (newRole) params.append('new_role', newRole);
    if (department) params.append('department', department);

    return await apiGet(`/Database/users/${encodeURIComponent(email)}/update?${params}`);
}

// ============== 2. РАБОТА С КАФЕДРАМИ ==============

// 2.1 Получить все кафедры
// GET /api/Database/departments
async function getDepartments() {
    return await apiGet('/Database/departments');
}

// 2.2 Создать кафедру
// GET /api/Database/departments/create?name=
async function createDepartment(name) {
    return await apiGet(`/Database/departments/create?name=${encodeURIComponent(name)}`);
}

// 2.3 Обновить кафедру
// GET /api/Database/departments/{name}/update?new_name=
async function updateDepartment(name, newName) {
    return await apiGet(`/Database/departments/${encodeURIComponent(name)}/update?new_name=${encodeURIComponent(newName)}`);
}

// 2.4 Удалить кафедру
// GET /api/Database/departments/{name}/remove
async function deleteDepartment(name) {
    return await apiGet(`/Database/departments/${encodeURIComponent(name)}/remove`);
}

// ============== 3. РАБОТА С АУДИТОРИЯМИ ==============

// 3.1 Получить загруженность аудитории
// GET /api/Rooms/{room_id}/workload/{start}/{end}
async function getRoomWorkload(roomId, start, end) {
    return await apiGet(`/Rooms/${roomId}/workload/${start}/${end}`);
}

// ============== 4. РАБОТА С КОРПУСАМИ ==============

// 4.1 Получить загруженность корпуса
// GET /api/Buildings/{bui_id}/workload/{start}/{end}
async function getBuildingWorkload(buildingId, start, end) {
    return await apiGet(`/Buildings/${buildingId}/workload/${start}/${end}`);
}

// 4.2 Получить загруженность нескольких корпусов
// GET /api/Buildings/workload/{start}/{end}?bui_ids=1,2,3
async function getBuildingsWorkload(start, end, buildingIds) {
    const ids = buildingIds.join(',');
    return await apiGet(`/Buildings/workload/${start}/${end}?bui_ids=${ids}`);
}

// ============== 5. РАБОТА С РАСПИСАНИЕМ ==============

// 5.1 Получить свободные окна преподавателей
// GET /api/Schedule/teachers/{UIDs}/{start}/{end}
async function getTeachersFreeSlots(teacherUIDs, start, end) {
    const uids = teacherUIDs.join(',');
    return await apiGet(`/Schedule/teachers/${uids}/${start}/${end}`);
}

async function getTeachersList() {
    return await oApiGet(`/teachers`);
}

async function getUsersByDepartmentList(departmentId) {
    return await apiGet(`/Database/departments/${departmentId}/users`);
}

async function getTeacherSchedule(teacher, start, end) {
    return await oApiGet(`/teachers/${teacher}/schedule/${start}/${end}`);
}

async function getBuildings() {
    return await oApiGet(`/buildings`);
}

async function getRooms(bui_id) {
    return await oApiGet(`/buildings/${bui_id}/rooms`);
}
