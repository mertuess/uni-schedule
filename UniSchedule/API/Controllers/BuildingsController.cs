// ┌────────────────────────────────────────────────────────────────────────────┐
// │ UniSchedule                                                                │
// │ WebAPI расширение для отслеживания расписания учебных занятий              │
// ├────────────────────────────────────────────────────────────────────────────┤
// │ Файл: BuildingsController.cs                                               │
// │ Описание: Контроллер для работы с корпусами                                │
// └────────────────────────────────────────────────────────────────────────────┘

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniSchedule.API.Responses;
using UniSchedule.Json;
using UniSchedule.Json.Models;
using UniSchedule.Services;

namespace UniSchedule.API.Controllers;

/// <summary>
/// Корпуса
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BuildingsController : ControllerBase
{
    private readonly JsonParser _jsonParser;
    private readonly OutAPI _o_api;
    private readonly CacheService _cache;

    public BuildingsController(OutAPI o_api, JsonParser jsonParser, CacheService cache)
    {
        _o_api = o_api;
        _jsonParser = jsonParser;
        _cache = cache;
    }

    /// <summary>
    /// Получить список всех корпусов (с кэшированием)
    /// </summary>
    [HttpGet]
    [AllowAnonymous]  
    public async Task<IActionResult> GetBuildings()
    {
        const string cacheKey = "static:buildings";

        if (_cache.TryGet<List<Building>>(cacheKey, out var cached))
            return Ok(cached);

        var buildings = await _o_api.SendRequest<Building>("/buildings", "buildings");
        _cache.SetStatic(cacheKey, buildings);

        return Ok(buildings);
    }

    /// <summary>
    /// Получить информацию о нагруженности корпуса
    /// </summary>
    [HttpGet("{bui_id}/workload/{start}/{end}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetBuildingWorkload(int bui_id, string start, string end)
    {
        string key = $"schedule:building:{bui_id}:{start}:{end}";
        if (_cache.TryGet<object>(key, out var cached))
            return Content(_jsonParser.Serialize(cached), "application/json");

        var response = new BuildingWorkloadResponse(_o_api, bui_id, new Week(start, end));
        var result = await response.GetBuildingWorkload();
        _cache.SetSchedule(key, result);
        return Content(_jsonParser.Serialize(result), "application/json");
    }

    /// <summary>
    /// Получить информацию о нагруженности корпусов
    /// </summary>
    [HttpGet("workload/{start}/{end}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetBuildingsWorkload(string start, string end, [FromQuery] string bui_ids)
    {
        string key = $"schedule:buildings:{bui_ids}:{start}:{end}";
        if (_cache.TryGet<object>(key, out var cached))
            return Content(_jsonParser.Serialize(cached), "application/json");

        var ids = bui_ids.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
        var response = new BuildingsWorkloadResponse(_o_api, ids, new Week(start, end));
        var result = await response.GetBuildingsWorkload();
        _cache.SetSchedule(key, result);
        return Content(_jsonParser.Serialize(result), "application/json");
    }
}