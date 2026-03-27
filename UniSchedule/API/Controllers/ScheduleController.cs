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
    /// <summary>
    /// Расписание
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ScheduleController : ControllerBase
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
        public ScheduleController(API api, JsonParser jsonParser)
        {
            _api = api;
            _jsonParser = jsonParser;
        }

        /// <summary>
        /// Получить пересечение окон у списка преподавателей за период
        /// </summary>
        /// <param name="UIDs">UID преподавателей через запятую</param>
        /// <param name="start">Дата от</param>
        /// <param name="end">Дата по</param>
        /// <returns>Json список с датами и временем когда все преподаватели свободны</returns>
        [HttpGet("teachers/{UIDs}/{start}/{end}")]
        [Authorize("operator", "teacher")]
        public async Task<IActionResult> GetTeachersMassSchedule(
            string UIDs, 
            string start, 
            string end)
        {
            var uidArray = UIDs.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var data = await _api.GetTeachersMassSchedule( 
                uidArray, 
                new Week(start, end)
            );
            
            return Content(data, "application/json");
        }
    }
}
