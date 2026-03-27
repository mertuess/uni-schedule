// ┌────────────────────────────────────────────────────────────────────────────┐
// │ UniSchedule                                                                │
// │ WebAPI расширение для отслеживания расписания учебных занятий              │
// ├────────────────────────────────────────────────────────────────────────────┤
// │ Файл: ScheduleController.cs                                                │
// │ Описание: Контроллер для работы с расписанием                              │
// └────────────────────────────────────────────────────────────────────────────┘
using Microsoft.AspNetCore.Mvc;
using UniSchedule.Json;
using UniSchedule.Json.Models;

/// <summary>
/// Пространство имен контроллеров api
/// </summary>
namespace UniSchedule.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScheduleController : ControllerBase
    {
        private readonly API _api;
        private readonly JsonParser _jsonParser;

        public ScheduleController(API api, JsonParser jsonParser)
        {
            _api = api;
            _jsonParser = jsonParser;
        }

        [HttpGet("teachers/{UIDs}/{start}/{end}")]
        [Authorize("operator", "teacher")]
        public async Task<IActionResult> GetTeachersMassSchedule(
            string UIDs, 
            string start, 
            string end)
        {
            var uidArray = UIDs.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var data = await _api.GetTeachersMassSchedule(
                _jsonParser, 
                uidArray, 
                new Week(start, end)
            );
            
            return Content(data, "application/json");
        }
    }
}
