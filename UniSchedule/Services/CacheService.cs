// ┌────────────────────────────────────────────────────────────────────────────┐
// │ UniSchedule                                                                │
// │ WebAPI расширение для отслеживания расписания учебных занятий              │
// ├────────────────────────────────────────────────────────────────────────────┤
// │ Файл: CacheService.cs                                                      │
// │ Описание: Сервис для кэширования статических и динамических данных         │
// └────────────────────────────────────────────────────────────────────────────┘

using System.Collections.Concurrent;

namespace UniSchedule.Services;

/// <summary>
/// Сервис для кэширования данных
/// </summary>
public class CacheService
{
    // Хранилище кэша: ключ = (данные, время истечения)
    private readonly ConcurrentDictionary<string, (object data, DateTime expiresAt)> _cache = new();

    // Время жизни кэша для статических данных (корпуса, преподаватели, аудитории)
    private static readonly TimeSpan StaticDataTtl = TimeSpan.FromHours(24);

    // Время жизни кэша для расписания (60 минут)
    private static readonly TimeSpan ScheduleDataTtl = TimeSpan.FromMinutes(60);

    /// <summary>
    /// Получить данные из кэша, если они ещё актуальны
    /// </summary>
    public bool TryGet<T>(string key, out T value)
    {
        value = default;

        if (_cache.TryGetValue(key, out var entry) && DateTime.UtcNow < entry.expiresAt)
        {
            value = (T)entry.data;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Сохранить статические данные в кэш (корпуса, преподаватели и т.д.)
    /// </summary>
    public void SetStatic<T>(string key, T value)
    {
        _cache[key] = (value, DateTime.UtcNow + StaticDataTtl);
    }

    /// <summary>
    /// Сохранить динамические данные в кэш (расписание)
    /// </summary>
    public void SetSchedule<T>(string key, T value)
    {
        _cache[key] = (value, DateTime.UtcNow + ScheduleDataTtl);
    }

    /// <summary>
    /// Удалить конкретный ключ из кэша
    /// </summary>
    public void Invalidate(string key)
    {
        _cache.TryRemove(key, out _);
    }

    /// <summary>
    /// Очистить весь кэш статических данных
    /// </summary>
    public void ClearStatic()
    {
        foreach (var key in _cache.Keys.Where(k => k.StartsWith("static:")).ToList())
        {
            _cache.TryRemove(key, out _);
        }
    }

    /// <summary>
    /// Очистить весь кэш расписания
    /// </summary>
    public void ClearSchedule()
    {
        foreach (var key in _cache.Keys.Where(k => k.StartsWith("schedule:")).ToList())
        {
            _cache.TryRemove(key, out _);
        }
    }
}