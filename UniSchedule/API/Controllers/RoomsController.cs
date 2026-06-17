// ┌────────────────────────────────────────────────────────────────────────────┐
// │ UniSchedule                                                                │
// │ WebAPI расширение для отслеживания расписания учебных занятий              │
// ├────────────────────────────────────────────────────────────────────────────┤
// │ Файл: RoomsController.cs                                                   │
// │ Описание: Контроллер для работы с аудиториями                              │
// └────────────────────────────────────────────────────────────────────────────┘

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniSchedule.API.Responses;
using UniSchedule.Json;
using UniSchedule.Json.Models;
using UniSchedule.Services;

namespace UniSchedule.API.Controllers;

/// <summary>
///     Аудитории
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class RoomsController : ControllerBase
{
    private readonly CacheService _cache;
    private readonly JsonParser _jsonParser;
    private readonly OutAPI _o_api;

    public RoomsController(OutAPI o_api, JsonParser jsonParser, CacheService cache)
    {
        _o_api = o_api;
        _jsonParser = jsonParser;
        _cache = cache;
    }

    /// <summary>
    ///     Получить аудитории корпуса (с кэшированием)
    /// </summary>
    [HttpGet("{bui_id}/rooms")]
    [AllowAnonymous]
    public async Task<IActionResult> GetRooms(int bui_id)
    {
        var key = $"static:rooms:{bui_id}";

        if (_cache.TryGet<List<Room>>(key, out var cached))
            return Ok(cached);

        var rooms = await _o_api.SendRequest<Room>($"/buildings/{bui_id}/rooms", "rooms");
        _cache.SetStatic(key, rooms);

        return Ok(rooms);
    }

    /// <summary>
    ///     Получить загруженность аудитории за определенный период
    /// </summary>
    [HttpGet("{room_id}/workload/{start}/{end}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetRoomWorkload(int room_id, string start, string end)
    {
        var key = $"schedule:room:{room_id}:{start}:{end}";
        if (_cache.TryGet<object>(key, out var cached))
            return Content(_jsonParser.Serialize(cached), "application/json");

        var response = new RoomWorkloadResponse(_o_api, room_id, new Week(start, end));
        var result = await response.GetRoomWorkload();
        _cache.SetSchedule(key, result);
        return Content(_jsonParser.Serialize(result), "application/json");
    }
}