// Конфигурация API
const CONFIG = {
    
    // Swagger
    API_URL: 'https://api-schedule.mauniver.ru/api/', 
    // токен
    TOKEN: 'eyJhbGciOiJIUzI1NiIsInR5cCI19347593j', 
    // Версия API
    API_VERSION: 'v1',
    // Таймаут запросов (мс)
    TIMEOUT: 10000
};

// Эндпоинты API 
const ENDPOINTS = {
    // Дата обновления
    UPDATE_INFO: '/',
    
    // Институты/факультеты
    FACULTIES: '/faculties',
    FACULTY_BY_ID: (id) => `/faculties/${id}`,
    
    // Курсы
    COURSES: '/courses',
    COURSE_BY_ID: (id) => `/courses/${id}`,
    
    // Группы
    GROUPS: '/groups',
    GROUPS_BY_FACULTY: (facId) => `/faculties/${facId}/groups`,
    GROUPS_MAIN_BY_FACULTY: (facId) => `/faculties/${facId}/groups/main`,
    GROUPS_BY_FACULTY_AND_COURSE: (facId, courseId) => `/faculties/${facId}/courses/${courseId}/groups`,
    GROUPS_MAIN_BY_FACULTY_AND_COURSE: (facId, courseId) => `/faculties/${facId}/courses/${courseId}/groups/main`,
    GROUP_SUBGROUPS: (uid) => `/groups/${uid}/subgroups`,
    GROUP_BY_UID: (uid) => `/groups/${uid}`,
    
    // Расписание группы
    GROUP_DATES: (uid) => `/groups/${uid}/dates`,
    GROUP_SCHEDULE_TODAY: (uid) => `/groups/${uid}/schedule/today`,
    GROUP_SCHEDULE_PERIOD: (uid, start, end) => `/groups/${uid}/schedule/${start}/${end}`,
    
    // Преподаватели
    TEACHERS: '/teachers',
    TEACHERS_SEARCH: '/teachers/search',
    TEACHER_BY_UID: (uid) => `/teachers/${uid}`,
    
    // Расписание преподавателя
    TEACHER_DATES: (uid) => `/teachers/${uid}/dates`,
    TEACHER_SCHEDULE_TODAY: (uid) => `/teachers/${uid}/schedule/today`,
    TEACHER_SCHEDULE_PERIOD: (uid, start, end) => `/teachers/${uid}/schedule/${start}/${end}`,
    
    
};