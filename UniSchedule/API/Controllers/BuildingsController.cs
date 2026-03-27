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
    /// <summary>
    /// Корпуса
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class BuildingsController : ControllerBase
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
        /// Конструктор
        /// </summary>
        /// <param name="api">Экземпляр API</param>
        /// <param name="jsonParser">"Экземпляр json парсера"</param>
        public BuildingsController(API api, JsonParser jsonParser)
        {
            _api = api;
            _jsonParser = jsonParser;
        }

        /// <summary>
        /// Получить информацию о нагруженности корпуса
        /// </summary>
        /// <param name="bui_id">ID корпуса</param>
        /// <param name="start">От даты</param>
        /// <param name="end">До даты</param>
        /// <returns>Json структуру с информацией о загруженности корпуса</returns>
        [HttpGet("{bui_id}/workload/{start}/{end}")]
        [Authorize("operator", "teacher")]
        public async Task<IActionResult> GetBuildingWorkload(
            int bui_id, 
            string start, 
            string end)
        {
            var data = await _api.GetBuildingWorkload( 
                bui_id, 
                new Week(start, end)
            );
            
            return Content(_jsonParser.Serialize(data), "application/json");
        }

        /// <summary>
        /// Получить информацию о нагруженности корпусов
        /// </summary>
        /// <param name="bui_ids">ID корпусd через запятую</param>
        /// <param name="start">От даты</param>
        /// <param name="end">До даты</param>
        /// <returns>Json структуру с информацией о загруженности корпусов</returns>
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
                ids.ToArray(), 
                new Week(start, end)
            );
            
            return Content(_jsonParser.Serialize(data), "application/json");
        }
    }
}
