// ┌────────────────────────────────────────────────────────────────────────────┐
// │ UniSchedule                                                                │
// │ WebAPI расширение для отслеживания расписания учебных занятий              │
// ├────────────────────────────────────────────────────────────────────────────┤
// │ Файл: Roomscontroller.cs                                                   │
// │ Описание: Контроллер для работы с аудиториями                              │
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
    public class RoomsController : ControllerBase
    {
        private readonly API _api;
        private readonly JsonParser _jsonParser;

        public RoomsController(API api, JsonParser jsonParser)
        {
            _api = api;
            _jsonParser = jsonParser;
        }

        [HttpGet("{room_id}/workload/{start}/{end}")]
        [Authorize("operator", "teacher")]
        public async Task<IActionResult> GetRoomWorkload(
            int room_id, 
            string start, 
            string end)
        {
            var data = await _api.GetRoomWorkload(
                _jsonParser, 
                room_id, 
                new Week(start, end)
            );
            
            return Content(_jsonParser.Serialize(data), "application/json");
        }
    }
}
