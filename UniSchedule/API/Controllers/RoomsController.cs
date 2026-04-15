// ┌────────────────────────────────────────────────────────────────────────────┐
// │ UniSchedule                                                                │
// │ WebAPI расширение для отслеживания расписания учебных занятий              │
// ├────────────────────────────────────────────────────────────────────────────┤
// │ Файл: Roomscontroller.cs                                                   │
// │ Описание: Контроллер для работы с аудиториями                              │
// └────────────────────────────────────────────────────────────────────────────┘

using Microsoft.AspNetCore.Mvc;
using UniSchedule.API.Responses;
using UniSchedule.Json;
using UniSchedule.Json.Models;

/// <summary>
/// Пространство имен контроллеров api
/// </summary>
namespace UniSchedule.API.Controllers;

/// <summary>
///     Аудитории
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class RoomsController : ControllerBase
{
    private readonly JsonParser _jsonParser;
    private readonly OutAPI _o_api;

    /// <summary>
    ///     Конструктор
    /// </summary>
    /// <param name="o_api">Экземпляр внешнего API</param>
    /// <param name="jsonParser">"Экземпляр json парсера"</param>
    public RoomsController(OutAPI o_api, JsonParser jsonParser)
    {
        _o_api = o_api;
        _jsonParser = jsonParser;
    }

    /// <summary>
    ///     Получить загруженность аудитори за определенный период
    /// </summary>
    /// <param name="room_id">ID Аудитории</param>
    /// <param name="start">Дата от</param>
    /// <param name="end">Дата по</param>
    /// <returns>Cформированный json с загруженностью аудиории</returns>
    [HttpGet("{room_id}/workload/{start}/{end}")]
    [Authorize("operator", "teacher")]
    public async Task<IActionResult> GetRoomWorkload(
        int room_id,
        string start,
        string end)
    {
        var response = new RoomWorkloadResponse(_o_api, room_id, new Week(start, end));
        return Content(_jsonParser.Serialize(await response.GetRoomWorkload()), "application/json");
    }
}