// ┌────────────────────────────────────────────────────────────────────────────┐
// │ UniSchedule                                                                │
// │ WebAPI расширение для отслеживания расписания учебных занятий              │
// ├────────────────────────────────────────────────────────────────────────────┤
// │ Файл: ScheduleController.cs                                                │
// │ Описание: Контроллер для работы с расписанием                              │
// └────────────────────────────────────────────────────────────────────────────┘

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniSchedule.API.Responses;
using UniSchedule.Json;
using UniSchedule.Json.Models;
using UniSchedule.Services;

namespace UniSchedule.API.Controllers;

/// <summary>
///     Расписание
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ScheduleController : ControllerBase
{
    private readonly CacheService _cache;
    private readonly JsonParser _jsonParser;
    private readonly OutAPI _o_api;

    public ScheduleController(OutAPI o_api, JsonParser jsonParser, CacheService cache)
    {
        _o_api = o_api;
        _jsonParser = jsonParser;
        _cache = cache;
    }

    /// <summary>
    ///     Получить пересечение окон у списка преподавателей за период
    /// </summary>
    [HttpGet("teachers/{UIDs}/{start}/{end}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTeachersMassSchedule(string UIDs, string start, string end)
    {
        var key = $"schedule:free-slots:{UIDs}:{start}:{end}";
        if (_cache.TryGet<object>(key, out var cached))
            return Content(_jsonParser.Serialize(cached), "application/json");

        var uidArray = UIDs.Split(',', StringSplitOptions.RemoveEmptyEntries);
        var response = new TeachersScheduleResponse(_o_api, uidArray, new Week(start, end));
        var result = await response.GetFreeSlots();
        _cache.SetSchedule(key, result);
        return Content(_jsonParser.Serialize(result), "application/json");
    }
}