// ┌────────────────────────────────────────────────────────────────────────────┐
// │ UniSchedule                                                                │
// │ WebAPI расширение для отслеживания расписания учебных занятий              │
// ├────────────────────────────────────────────────────────────────────────────┤
// │ Файл: BuildingsController.cs                                               │
// │ Описание: Контроллер для работы с корпусами                                │
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
    public class BuildingsController : ControllerBase
    {
        private readonly API _api;
        private readonly JsonParser _jsonParser;

        public BuildingsController(API api, JsonParser jsonParser)
        {
            _api = api;
            _jsonParser = jsonParser;
        }

        [HttpGet("{bui_id}/workload/{start}/{end}")]
        [Authorize("operator", "teacher")]
        public async Task<IActionResult> GetBuildingWorkload(
            int bui_id, 
            string start, 
            string end)
        {
            var data = await _api.GetBuildingWorkload(
                _jsonParser, 
                bui_id, 
                new Week(start, end)
            );
            
            return Content(_jsonParser.Serialize(data), "application/json");
        }

        [HttpGet("workload/{start}/{end}")]
        [Authorize("operator", "teacher")]
        public async Task<IActionResult> GetBuildingsWorkload( 
            string start, 
            string end,
            [FromQuery] string bui_ids)
        {
            List<int> ids = new List<int>();
            foreach(var s in bui_ids.Split(',', StringSplitOptions.RemoveEmptyEntries)){
                ids.Add(Convert.ToInt32(s));
            }
            var data = await _api.GetBuildingsWorkload(
                _jsonParser, 
                ids.ToArray(), 
                new Week(start, end)
            );
            
            return Content(_jsonParser.Serialize(data), "application/json");
        }
    }
}
