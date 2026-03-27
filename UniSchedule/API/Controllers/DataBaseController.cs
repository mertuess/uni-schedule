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
    [ApiController]
    [Route("api/[controller]")]
    public class DatabaseController : ControllerBase
    {
        private readonly API _api;
        private readonly JsonParser _jsonParser;
        private readonly DataBaseManager _dbm;

        public DatabaseController(API api, JsonParser jsonParser, DataBaseManager dbm)
        {
            _api = api;
            _jsonParser = jsonParser;
            _dbm = dbm;
        }

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

        [HttpGet("departments/{name}/remove")]
        [Authorize("operator")]
        public async Task<IActionResult> RemoveDepartment(
            string name)
        {
            var res = await _dbm.TryRemoveDepartmentAsync(name);
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

        [HttpGet("departments")]
        [Authorize("operator")]
        public async Task<IActionResult> ShowAllDepartments()
        {
            var result = await _dbm.GetAllDepartmentsAsync();
            return Content(_jsonParser.Serialize(result), "application/json");
        }
    }
}
