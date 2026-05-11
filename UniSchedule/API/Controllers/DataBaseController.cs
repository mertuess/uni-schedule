// ┌────────────────────────────────────────────────────────────────────────────┐
// │ UniSchedule                                                                │
// │ WebAPI расширение для отслеживания расписания учебных занятий              │
// ├────────────────────────────────────────────────────────────────────────────┤
// │ Файл: DataBaseController.cs                                                │
// │ Описание: Контроллер для взаимодействия с внутренней БД                    │
// └────────────────────────────────────────────────────────────────────────────┘

using Microsoft.AspNetCore.Mvc;
using UniSchedule.DataBase;
using UniSchedule.Json;

/// <summary>
/// Пространство имен контроллеров api
/// </summary>
namespace UniSchedule.API.Controllers;

/// <summary>
///     База данных
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DatabaseController : ControllerBase
{
    private readonly DataBaseManager _dbm;
    private readonly JsonParser _jsonParser;

    /// <summary>
    ///     Конструктор
    /// </summary>
    /// <param name="jsonParser">"Экземпляр json парсера"</param>
    /// <param name="dbm">"Экземпляр менеджера базы данных"</param>
    public DatabaseController(JsonParser jsonParser, DataBaseManager dbm)
    {
        _jsonParser = jsonParser;
        _dbm = dbm;
    }

    /// <summary>
    ///     Создать нового пользователя
    /// </summary>
    /// <param name="email">Электронная почта</param>
    /// <param name="password">Пароль</param>
    /// <param name="name">Фамилия Имя Отчество</param>
    /// <param name="engName">Фамилия Имя Отчество на английском</param>
    /// <param name="role">Права доступа</param>
    /// <returns>Ok, если все нормально. BadRequest если что-то пошло не так</returns>
    [HttpGet("users/create")]
    [Authorize("operator")]
    public async Task<IActionResult> CreateUser(
        [FromQuery] string email,
        [FromQuery] string password,
        [FromQuery] string name,
        [FromQuery] string engName,
        [FromQuery] string role)
    {
        var res = await _dbm.TryCreateUserAsync(email, password, name, engName, role);

        if (res)
            return Ok(new
            {
                success = true,
                message = "User created successfully"
            });

        return BadRequest(new
        {
            success = false,
            message = "Failed to create user. User may already exist"
        });
    }

    /// <summary>
    ///     Вывести всех пользователей
    /// </summary>
    /// <returns>Список пользователей в формате json</returns>
    [HttpGet("users")]
    [Authorize("operator")]
    public async Task<IActionResult> ShowAllUsers()
    {
        var result = await _dbm.GetAllUsersAsync();
        return Content(_jsonParser.Serialize(result), "application/json");
    }

    /// <summary>
    ///     Удалить пользователя
    /// </summary>
    /// <param name="email">Электронная почта</param>
    /// <returns>Ok, если все нормально. BadRequest если что-то пошло не так</returns>
    [HttpGet("users/{email}/remove")]
    [Authorize("operator")]
    public async Task<IActionResult> RemoveUser(string email)
    {
        var res = await _dbm.TryRemoveUserAsync(email);

        if (res)
            return Ok(new
            {
                success = true,
                message = "User removed successfully"
            });

        return BadRequest(new
        {
            success = false,
            message = "Failed to remove user. Can't find user with this email"
        });
    }

    /// <summary>
    ///     Обновить пользователя
    /// </summary>
    /// <param name="email">Электронная почта</param>
    /// <param name="new_email">Новая электронная почта</param>
    /// <param name="new_password">Новый пароль</param>
    /// <param name="new_name">Фамилия Имя Отчество</param>
    /// <param name="new_engName">Фамилия Имя Отчество на английском</param>
    /// <param name="new_role">Новые права доступа</param>
    /// <param name="department">Кафедра</param>
    /// <returns>Ok, если все нормально. BadRequest если что-то пошло не так</returns>
    [HttpGet("users/{email}/update")]
    [Authorize("operator")]
    public async Task<IActionResult> UpdateUser(
        string email,
        [FromQuery] string? new_email,
        [FromQuery] string? new_password,
        [FromQuery] string? new_name,
        [FromQuery] string? new_engName,
        [FromQuery] string? new_role,
        [FromQuery] int? department)
    {
        var res = await _dbm.TryUpdateUserAsync(email, new_email, new_password, new_name, new_engName, new_role,
            department);

        if (res)
            return Ok(new
            {
                success = true,
                message = "User updated successfully"
            });

        return BadRequest(new
        {
            success = false,
            message = "Failed to update user"
        });
    }

    /// <summary>
    ///     Создать кафедру
    /// </summary>
    /// <param name="name">Наименование</param>
    /// <returns>Ok, если все нормально. BadRequest если что-то пошло не так</returns>
    [HttpGet("departments/create")]
    [Authorize("operator")]
    public async Task<IActionResult> CreateDepartment(
        [FromQuery] string name)
    {
        var res = await _dbm.TryCreateDepartmentAsync(name);
        if (res)
            return Ok(new
            {
                success = true,
                message = "Department created successfully"
            });

        return BadRequest(new
        {
            success = false,
            message = "Failed to create department"
        });
    }

    /// <summary>
    ///     Обновить кафедру
    /// </summary>
    /// <param name="name">Наименование</param>
    /// <param name="new_name">Новое наименование</param>
    /// <returns>Ok, если все нормально. BadRequest если что-то пошло не так</returns>
    [HttpGet("departments/{name}/update")]
    [Authorize("operator")]
    public async Task<IActionResult> UpdateDepartment(
        string name,
        [FromQuery] string new_name)
    {
        var res = await _dbm.TryUpdateDepartmentAsync(name, new_name);
        if (res)
            return Ok(new
            {
                success = true,
                message = "Department updated successfully"
            });

        return BadRequest(new
        {
            success = false,
            message = "Failed to update department"
        });
    }

    /// <summary>
    ///     Удалить кафедру
    /// </summary>
    /// <param name="name">Наименование</param>
    /// <returns>Ok, если все нормально. BadRequest если что-то пошло не так</returns>
    [HttpGet("departments/{name}/remove")]
    [Authorize("operator")]
    public async Task<IActionResult> RemoveDepartment(
        string name)
    {
        var res = await _dbm.TryRemoveDepartmentAsync(name);
        if (res)
            return Ok(new
            {
                success = true,
                message = "Department removed successfully"
            });

        return BadRequest(new
        {
            success = false,
            message = "Failed to remove department"
        });
    }

    /// <summary>
    ///     Отобразить все кафедры
    /// </summary>
    /// <returns>Json строку со всеми кафедрами</returns>
    [HttpGet("departments")]
    [Authorize("operator", "teacher")]
    public async Task<IActionResult> ShowAllDepartments()
    {
        var result = await _dbm.GetAllDepartmentsAsync();
        return Content(_jsonParser.Serialize(result), "application/json");
    }

    /// <summary>
    ///     Отобразить всех пользователей одной кафедры
    /// </summary>
    /// <returns>Json строка со всеми пользователями</returns>
    [HttpGet("departments/{departmentId}/users")]
    [Authorize("operator", "teacher")]
    public async Task<IActionResult> GetAllUsersByDepartment(int departmentId)
    {
        var users = await _dbm.GetAllUsersAsync();
        var result = users.Where(x => x.DepartmentId == departmentId);
        return Content(_jsonParser.Serialize(result), "application/json");
    }

    /// <summary>
    ///     Попытка подключения
    /// </summary>
    /// <returns>Статус подключения</returns>
    [HttpGet("tryauth")]
    public async Task<IActionResult> TryAuth(
        [FromQuery] string email,
        [FromQuery] string password)
    {
        var users = _dbm.GetAllUsers();
        if (users.Where(x => x.Mail == email).Count() < 1)
            return Content("Пользователь не найден", "text/text");
        var user = users.Where(x => x.Mail == email).First();
        if (!_dbm.VerifyPassword(password, user.Password))
            return Content("Неверный пароль", "text/text");
        return Content("Успешно", "text/text");
    }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    [HttpGet("users/{email}/role")]
    [Authorize("operator", "teacher")]
    public async Task<IActionResult> GetRole(
        string email)
    {
        var users = _dbm.GetAllUsers();
        var user = users.Where(x => x.Mail == email).First();
        return Content(_jsonParser.Serialize(user.Role), "text/text");
    }
}