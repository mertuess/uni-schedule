// ┌────────────────────────────────────────────────────────────────────────────┐
// │ UniSchedule                                                                │
// │ WebAPI расширение для отслеживания расписания учебных занятий              │
// ├────────────────────────────────────────────────────────────────────────────┤
// │ Файл: BuildingsController.cs                                               │
// │ Описание: Контроллер для работы с корпусами                                │
// └────────────────────────────────────────────────────────────────────────────┘

using Microsoft.AspNetCore.Mvc;
using UniSchedule.API.Responses;
using UniSchedule.Json;
using UniSchedule.Json.Models;
using UniSchedule.Services;

/// <summary>
/// Пространство имен контроллеров api
/// </summary>
namespace UniSchedule.API.Controllers;

/// <summary>
///     Корпуса
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BuildingsController : ControllerBase
{
    private readonly JsonParser _jsonParser;
    private readonly OutAPI _o_api;
    private readonly CacheService _cache;

    /// <summary>
    ///     Конструктор
    /// </summary>
    /// <param name="o_api">Экземпляр API</param>
    /// <param name="jsonParser">"Экземпляр json парсера"</param>
    public BuildingsController(OutAPI o_api, JsonParser jsonParser, CacheService cache)
    {
        _o_api = o_api;
        _jsonParser = jsonParser;
        _cache = cache;
    }

    /// <summary>
    ///     Получить список всех корпусов (с кэшированием) ← НОВЫЙ МЕТОД
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetBuildings()
    {
        const string cacheKey = "static:buildings";

        // Пробуем получить из кэша
        if (_cache.TryGet<List<Building>>(cacheKey, out var cached))
            return Ok(cached);

        // Если нет в кэше — запрашиваем у внешнего API
        var buildings = await _o_api.SendRequest<Building>("/buildings", "buildings");

        // Сохраняем в кэш
        _cache.SetStatic(cacheKey, buildings);

        return Ok(buildings);
    }

    /// <summary>
    ///     Получить информацию о нагруженности корпуса
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
        var response = new BuildingWorkloadResponse(_o_api, bui_id, new Week(start, end));
        return Content(_jsonParser.Serialize(await response.GetBuildingWorkload()), "application/json");
    }

    /// <summary>
    ///     Получить информацию о нагруженности корпусов
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
        var ids = new List<int>();
        foreach (var s in bui_ids.Split(',', StringSplitOptions.RemoveEmptyEntries)) ids.Add(Convert.ToInt32(s));
        var response = new BuildingsWorkloadResponse(_o_api, ids.ToArray(), new Week(start, end));
        return Content(_jsonParser.Serialize(await response.GetBuildingsWorkload()), "application/json");
    }
}