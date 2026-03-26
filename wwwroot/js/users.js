// ============================================
// ФАЙЛ РАБОТЫ С ПОЛЬЗОВАТЕЛЯМИ
// Отвечает за все API-запросы к серверу для управления пользователями
// ============================================

// Получение всех пользователей
async function getUsers() {
    try {
        const response = await fetch('/api/users');
        if (response.ok) {
            return await response.json();
        }
        return [];
    } catch (error) {
        console.error('Ошибка getUsers:', error);
        return [];
    }
}

// Получение пользователя по ID
async function getUserById(id) {
    try {
        const response = await fetch('/api/users/' + id);
        if (response.ok) {
            return await response.json();
        } else if (response.status === 404) {
            console.log('Пользователь не найден, ID:', id);
            return null;
        }
        return null;
    } catch (error) {
        console.error('Ошибка getUserById:', error);
        return null;
    }
}

// Создание нового пользователя
async function createUser(userData) {
    try {
        const response = await fetch('/api/users', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(userData)
        });
        
        if (response.ok) {
            const result = await response.json();
            showNotification('Пользователь создан', 'success');
            return result;
        } else if (response.status === 400) {
            const error = await response.json();
            showNotification(error.message || 'Пользователь уже существует', 'error');
            return null;
        } else {
            showNotification('Ошибка при создании', 'error');
            return null;
        }
    } catch (error) {
        console.error('Ошибка createUser:', error);
        showNotification('Ошибка соединения', 'error');
        return null;
    }
}

// Обновление данных пользователя
async function updateUser(id, userData) {
    try {
        const response = await fetch('/api/users/' + id, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(userData)
        });
        
        if (response.ok) {
            const result = await response.json();
            showNotification('Пользователь обновлен', 'success');
            return true;
        } else {
            const error = await response.json();
            showNotification(error.message || 'Ошибка при обновлении', 'error');
            return false;
        }
    } catch (error) {
        console.error('Ошибка updateUser:', error);
        showNotification('Ошибка соединения', 'error');
        return false;
    }
}

// Удаление пользователя
async function deleteUser(id) {
    // Проверяем, не пытаемся ли удалить оператора
    const userToDelete = await getUserById(id);
    if (userToDelete && userToDelete.role === 'operator') {
        showNotification('Нельзя удалить оператора', 'error');
        return false;
    }
    
    try {
        const response = await fetch('/api/users/' + id, {
            method: 'DELETE'
        });
        
        if (response.ok) {
            showNotification('Пользователь удален', 'success');
            return true;
        } else {
            showNotification('Ошибка при удалении', 'error');
            return false;
        }
    } catch (error) {
        console.error('Ошибка deleteUser:', error);
        showNotification('Ошибка соединения', 'error');
        return false;
    }
}