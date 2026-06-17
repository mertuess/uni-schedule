// ┌────────────────────────────────────────────────────────────────────────────┐
// │ UniSchedule                                                                │
// │ WebAPI расширение для отслеживания расписания учебных занятий              │
// ├────────────────────────────────────────────────────────────────────────────┤
// │ Файл: DataBaseController.cs                                                │
// │ Описание: Контроллер для взаимодействия с внутренней БД                    │
// └────────────────────────────────────────────────────────────────────────────┘

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniSchedule.DataBase;
using UniSchedule.DataBase.Models;
using UniSchedule.Json;
using UniSchedule.Services;

namespace UniSchedule.API.Controllers;

/// <summary>
///     Контроллер базы данных
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DatabaseController : ControllerBase
{
    private readonly CacheService _cache;
    private readonly DataBaseManager _dbm;
    private readonly JsonParser _jsonParser;

    public DatabaseController(JsonParser jsonParser, DataBaseManager dbm, CacheService cache)
    {
        _jsonParser = jsonParser;
        _dbm = dbm;
        _cache = cache;
    }

    // ==================== ПОЛЬЗОВАТЕЛИ ====================

    [HttpGet("users/create")]
    [Authorize("operator")]
    public async Task<IActionResult> CreateUser([FromQuery] string email, [FromQuery] string password,
        [FromQuery] string name, [FromQuery] string engName, [FromQuery] string role)
    {
        var forcedRole = "operator";
        var res = await _dbm.TryCreateUserAsync(email, password, name, engName, forcedRole);
        if (res) return Ok(new { success = true, message = "User created successfully" });
        return BadRequest(new { success = false, message = "Failed to create user. User may already exist" });
    }

    [HttpGet("users")]
    [Authorize("operator")]
    public async Task<IActionResult> ShowAllUsers()
    {
        var result = await _dbm.GetAllUsersAsync();
        return Content(_jsonParser.Serialize(result), "application/json");
    }

    [HttpGet("users/{email}/remove")]
    [Authorize("operator")]
    public async Task<IActionResult> RemoveUser(string email)
    {
        var res = await _dbm.TryRemoveUserAsync(email);
        if (res) return Ok(new { success = true, message = "User removed successfully" });
        return BadRequest(new { success = false, message = "Failed to remove user. Can't find user with this email" });
    }

    [HttpGet("users/{email}/update")]
    [Authorize("operator")]
    public async Task<IActionResult> UpdateUser(string email, [FromQuery] string? new_email,
        [FromQuery] string? new_password, [FromQuery] string? new_name, [FromQuery] string? new_engName,
        [FromQuery] string? new_role, [FromQuery] int? department)
    {
        var res = await _dbm.TryUpdateUserAsync(email, new_email, new_password, new_name, new_engName, null,
            department);
        if (res) return Ok(new { success = true, message = "User updated successfully" });
        return BadRequest(new { success = false, message = "Failed to update user" });
    }

    [HttpGet("tryauth")]
    public async Task<IActionResult> TryAuth([FromQuery] string email, [FromQuery] string password)
    {
        var users = _dbm.GetAllUsers();
        if (users.Count(x => x.Mail == email) < 1) return Content("Пользователь не найден", "text/text");
        var user = users.First(x => x.Mail == email);
        if (!_dbm.VerifyPassword(password, user.Password)) return Content("Неверный пароль", "text/text");
        return Content("Успешно", "text/text");
    }

    [HttpGet("users/{email}/role")]
    [Authorize("operator", "teacher")]
    public async Task<IActionResult> GetRole(string email)
    {
        var users = _dbm.GetAllUsers();
        var user = users.FirstOrDefault(x => x.Mail == email);
        if (user == null) return NotFound();
        return Content(_jsonParser.Serialize(user.Role), "text/text");
    }

    // ==================== КАФЕДРЫ ====================

    [HttpGet("departments/create")]
    [Authorize("operator")]
    public async Task<IActionResult> CreateDepartment([FromQuery] string name)
    {
        var res = await _dbm.TryCreateDepartmentAsync(name);
        if (res)
        {
            _cache.Invalidate("static:departments");
            return Ok(new { success = true, message = "Department created successfully" });
        }

        return BadRequest(new { success = false, message = "Failed to create department" });
    }

    [HttpGet("departments/{name}/update")]
    [Authorize("operator")]
    public async Task<IActionResult> UpdateDepartment(string name, [FromQuery] string new_name)
    {
        var res = await _dbm.TryUpdateDepartmentAsync(name, new_name);
        if (res)
        {
            _cache.Invalidate("static:departments");
            return Ok(new { success = true, message = "Department updated successfully" });
        }

        return BadRequest(new { success = false, message = "Failed to update department" });
    }

    [HttpGet("departments/{name}/remove")]
    [Authorize("operator")]
    public async Task<IActionResult> RemoveDepartment(string name)
    {
        var res = await _dbm.TryRemoveDepartmentAsync(name);
        if (res)
        {
            _cache.Invalidate("static:departments");
            return Ok(new { success = true, message = "Department removed successfully" });
        }

        return BadRequest(new { success = false, message = "Failed to remove department" });
    }

    [HttpGet("departments")]
    [AllowAnonymous]
    public async Task<IActionResult> ShowAllDepartments()
    {
        if (_cache.TryGet<List<Department>>("static:departments", out var cached))
            return Content(_jsonParser.Serialize(cached), "application/json");

        var result = await _dbm.GetAllDepartmentsAsync();
        _cache.SetStatic("static:departments", result);
        return Content(_jsonParser.Serialize(result), "application/json");
    }

    [HttpGet("departments/{departmentId}/users")]
    [Authorize("operator", "teacher")]
    public async Task<IActionResult> GetAllUsersByDepartment(int departmentId)
    {
        var users = await _dbm.GetAllUsersAsync();
        var result = users.Where(x => x.DepartmentId == departmentId);
        return Content(_jsonParser.Serialize(result), "application/json");
    }

    // ==================== ПРИВЯЗКА ПРЕПОДАВАТЕЛЕЙ ====================

    [HttpGet("teachers/all")]
    [Authorize("operator")]
    public async Task<IActionResult> GetAllTeachersExternal()
    {
        if (_cache.TryGet<object>("static:teachers:external", out var cached))
            return Ok(cached);

        var teachers = await _dbm.GetAllTeachersExternalAsync();
        var formatted = teachers.Select(t => new { uid = t.UID, name = t.teacher, t.faculty }).ToList();

        _cache.SetStatic("static:teachers:external", formatted);
        return Ok(formatted);
    }

    [HttpPost("teachers/{uid}/bind")]
    [Authorize("operator")]
    public IActionResult BindTeacher(string uid, [FromBody] BindTeacherRequest request)
    {
        if (string.IsNullOrWhiteSpace(uid)) return BadRequest(new { error = "UID обязателен" });
        var success = _dbm.BindTeacher(uid, request.name ?? "", request.departmentId);
        if (success)
        {
            _cache.Invalidate("static:teachers:external");
            return Ok(new { success = true, message = "Привязано" });
        }

        return BadRequest(new { error = "Ошибка привязки (см. лог сервера)" });
    }

    [HttpDelete("teachers/{uid}/unbind")]
    [Authorize("operator")]
    public IActionResult UnbindTeacher(string uid, [FromQuery] int departmentId)
    {
        if (string.IsNullOrWhiteSpace(uid))
            return BadRequest(new { error = "UID обязателен" });

        if (departmentId <= 0)
            return BadRequest(new { error = "ID кафедры обязателен" });

        var success = _dbm.UnbindTeacher(uid, departmentId);

        if (success)
            return Ok(new { success = true, message = "Привязка удалена" });

        return NotFound(new { error = "Привязка не найдена" });
    }

    [HttpGet("departments/{departmentId}/teachers")]
    [AllowAnonymous]
    public IActionResult GetTeachersByDepartment(int departmentId)
    {
        var teachers = _dbm.GetTeachersByDepartment(departmentId);
        return Ok(teachers.Select(t => new { uid = t.UniversityUid, name = t.Name, departmentId = t.DepartmentId }));
    }

    [HttpGet("departments/{departmentId}/schedule")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDepartmentSchedule(int departmentId, [FromQuery] string start,
        [FromQuery] string end)
    {
        var key = $"schedule:dept:{departmentId}:{start}:{end}";
        if (_cache.TryGet<List<object>>(key, out var cached))
            return Ok(cached);

        var schedule = await _dbm.GetDepartmentScheduleAsync(departmentId, start, end);
        _cache.SetSchedule(key, schedule);
        return Ok(schedule);
    }

    public class BindTeacherRequest
    {
        public string name { get; set; } = string.Empty;
        public int? departmentId { get; set; }
    }
}