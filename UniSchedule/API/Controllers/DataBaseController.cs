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
namespace UniSchedule.API.Controllers
{
    /// <summary>
    /// База данных
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DatabaseController : ControllerBase
    {
        /// <summary>
        /// Экземпляр API
        /// </summary>
        private readonly API _api;
        /// <summary>
        /// Экземпляр json парсера
        /// </summary>
        private readonly JsonParser _jsonParser;
        /// <summary>
        /// Экземпляр менеджера базы данных
        /// </summary>
        private readonly DataBaseManager _dbm;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="api">Экземпляр API</param>
        /// <param name="jsonParser">"Экземпляр json парсера"</param>
        /// <param name="dbm">"Экземпляр менеджера базы данных"</param>
        public DatabaseController(API api, JsonParser jsonParser, DataBaseManager dbm)
        {
            _api = api;
            _jsonParser = jsonParser;
            _dbm = dbm;
        }

        /// <summary>
        /// Создать нового пользователя
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

            if(res){
                return Ok(new{
                        success = true,
                        message = "User created successfully"
                        });
            }
            else{
                return BadRequest(new{
                        success = false,
                        message = "Failed to create user. User may already exist"
                        });
            }
        }

        /// <summary>
        /// Вывести всех пользователей
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
        /// Удалить пользователя
        /// </summary>
        /// <param name="email">Электронная почта</param>
        /// <returns>Ok, если все нормально. BadRequest если что-то пошло не так</returns>
        [HttpGet("users/{email}/remove")]
        [Authorize("operator")]
        public async Task<IActionResult> RemoveUser(string email)
        {
            var res = await _dbm.TryRemoveUserAsync(email);

            if(res){
                return Ok(new{
                        success = true,
                        message = "User created successfully"
                        });
            }
            else{
                return BadRequest(new{
                        success = false,
                        message = "Failed to create user. User may already exist"
                        });
            }
        }

        /// <summary>
        /// Обновить пользователя
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
            var res = await _dbm.TryUpdateUserAsync(email, new_email, new_password, new_name, new_engName, new_role, department);

            if(res){
                return Ok(new{
                        success = true,
                        message = "User updated successfully"
                        });
            }
            else{
                return BadRequest(new{
                        success = false,
                        message = "Failed to update user"
                        });
            }
        }

        /// <summary>
        /// Создать кафедру
        /// </summary>
        /// <param name="name">Наименование</param>
        /// <returns>Ok, если все нормально. BadRequest если что-то пошло не так</returns>
        [HttpGet("departments/create")]
        [Authorize("operator")]
        public async Task<IActionResult> CreateDepartment(
            [FromQuery] string name)
        {
            var res = await _dbm.TryCreateDepartmentAsync(name);
            if(res){
                return Ok(new{
                        success = true,
                        message = "Department created successfully"
                        });
            }
            else{
                return BadRequest(new{
                        success = false,
                        message = "Failed to create department"
                        });
            }
        }

        /// <summary>
        /// Обновить кафедру
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
            if(res){
                return Ok(new{
                        success = true,
                        message = "Department updated successfully"
                        });
            }
            else{
                return BadRequest(new{
                        success = false,
                        message = "Failed to update department"
                        });
            }
        }

        /// <summary>
        /// Удалить кафедру
        /// </summary>
        /// <param name="name">Наименование</param>
        /// <returns>Ok, если все нормально. BadRequest если что-то пошло не так</returns>
        [HttpGet("departments/{name}/remove")]
        [Authorize("operator")]
        public async Task<IActionResult> RemoveDepartment(
            string name)
        {
            var res = await _dbm.TryRemoveDepartmentAsync(name);
            if(res){
                return Ok(new{
                        success = true,
                        message = "Department removed successfully"
                        });
            }
            else{
                return BadRequest(new{
                        success = false,
                        message = "Failed to remove department"
                        });
            }
        }

        /// <summary>
        /// Отобразить все кафедры
        /// </summary>
        /// <returns>Json строку со всеми кафедрами</returns>
        [HttpGet("departments")]
        [Authorize("operator")]
        public async Task<IActionResult> ShowAllDepartments()
        {
            var result = await _dbm.GetAllDepartmentsAsync();
            return Content(_jsonParser.Serialize(result), "application/json");
        }
    }
}
