// ┌────────────────────────────────────────────────────────────────────────────┐
// │ UniSchedule                                                                │
// │ WebAPI расширение для отслеживания расписания учебных занятий              │
// ├────────────────────────────────────────────────────────────────────────────┤
// │ Файл: TeachersController.cs                                                │
// │ Описание: Контроллер для работы с преподавателями                          │
// └────────────────────────────────────────────────────────────────────────────┘

using Microsoft.AspNetCore.Mvc;
using UniSchedule.Json;
using UniSchedule.Json.Models;
using UniSchedule.Services;
using UniSchedule.System;

namespace UniSchedule.API.Controllers;

/// <summary>
/// Контроллер для работы с преподавателями
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TeachersController : ControllerBase
{
    private readonly OutAPI _o_api;
    private readonly JsonParser _jsonParser;
    private readonly CacheService _cache;
    private readonly Debug _dbg;

    public TeachersController(OutAPI o_api, JsonParser jsonParser, CacheService cache, Debug dbg)
    {
        _o_api = o_api;
        _jsonParser = jsonParser;
        _cache = cache;
        _dbg = dbg;
    }

    /// <summary>
    /// Получить список преподавателей (с кэшированием)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetTeachers()
    {
        const string key = "static:teachers";

        if (_cache.TryGet<List<Teacher>>(key, out var cached))
            return Ok(cached);

        var teachers = await _o_api.SendRequest<Teacher>("/teachers", "teachers");
        _cache.SetStatic(key, teachers);

        return Ok(teachers);
    }

    /// <summary>
    /// Поиск преподавателя по имени
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> SearchTeachers([FromQuery] string query)
    {
        var result = await _o_api.SendRequest<Teacher>($"/teachers/search?query={query}", "teachers");
        return Ok(result);
    }

    /// <summary>
    /// Получить расписание преподавателя в диапазоне дат
    /// Параметры передаются в пути: /api/Teachers/{uid}/schedule/{start}/{end}
    /// </summary>
    [HttpGet("{uid}/schedule/{start}/{end}")]
    public async Task<IActionResult> GetTeacherSchedule(string uid, string start, string end)
    {
        try
        {
            string cacheKey = "schedule:teacher:" + uid + ":" + start + ":" + end;

            // Пробуем вернуть из кэша
            if (_cache.TryGet<List<TeacherSchedule>>(cacheKey, out var cached))
                return Ok(cached);

            // Получаем сырой JSON от внешнего API
            var rawJson = await _o_api.GetRawAsync("/teachers/" + uid + "/schedule/" + start + "/" + end);

            // Парсинг через global::System.Text.Json для обхода конфликта имён
            List<TeacherSchedule> schedule = null;
            var options = new global::System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            using var doc = global::System.Text.Json.JsonDocument.Parse(rawJson);
            var root = doc.RootElement;

            if (root.ValueKind == global::System.Text.Json.JsonValueKind.Array)
            {
                // Прямой массив: [{...}, {...}]
                schedule = global::System.Text.Json.JsonSerializer.Deserialize<List<TeacherSchedule>>(rawJson, options);
            }
            else if (root.ValueKind == global::System.Text.Json.JsonValueKind.Object)
            {
                // Обёртка с полем "timetable" (формат МАУ API)
                if (root.TryGetProperty("timetable", out var propTimetable))
                {
                    schedule = global::System.Text.Json.JsonSerializer.Deserialize<List<TeacherSchedule>>(propTimetable.GetRawText(), options);
                }
                // Обёртка с полем "schedule"
                else if (root.TryGetProperty("schedule", out var propSchedule))
                {
                    schedule = global::System.Text.Json.JsonSerializer.Deserialize<List<TeacherSchedule>>(propSchedule.GetRawText(), options);
                }
                // Обёртка с полем "data"
                else if (root.TryGetProperty("data", out var propData))
                {
                    schedule = global::System.Text.Json.JsonSerializer.Deserialize<List<TeacherSchedule>>(propData.GetRawText(), options);
                }
            }

            if (schedule == null)
                schedule = new List<TeacherSchedule>();

            // Сохраняем в кэш
            _cache.SetSchedule(cacheKey, schedule);

            return Ok(schedule);
        }
        catch (Exception ex)
        {
            _dbg.Log("Ошибка GetTeacherSchedule: " + ex.Message);
            return StatusCode(500, new { error = ex.Message });
        }
    }
}