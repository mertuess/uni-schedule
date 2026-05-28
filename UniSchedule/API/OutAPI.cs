// ┌────────────────────────────────────────────────────────────────────────────┐
// │ UniSchedule                                                                │
// │ WebAPI расширение для отслеживания расписания учебных занятий              │
// ├────────────────────────────────────────────────────────────────────────────┤
// │ Файл: OutAPI.cs                                                            │
// │ Описание: Класс для работы с внешним api                                   │
// └────────────────────────────────────────────────────────────────────────────┘

using UniSchedule.Json;
using UniSchedule.System;

namespace UniSchedule.API;

/// <summary>
/// Класс для работы с внешним API
/// </summary>
public class OutAPI
{
    private static readonly HttpClient sharedClient = new();
    private readonly Debug _dbg;
    private readonly JsonParser _jsonParser;
    private readonly Localization _loc;
    private readonly string url;
    private readonly IConfiguration _configuration;

    public OutAPI(IConfiguration configuration, JsonParser jsonParser, Debug dbg, Localization loc)
    {
        _configuration = configuration;
        _jsonParser = jsonParser;
        _dbg = dbg;
        _loc = loc;

        url = configuration.GetConnectionString("OutAPI_URL") ??
              throw new Exception("Out API URL not found!");

        sharedClient.BaseAddress = new Uri(url);
        sharedClient.DefaultRequestHeaders.Add("accept", "*/*");
        loadToken();
    }

    /// <summary>
    /// Отправляет запрос на внешний api и возвращает список моделей
    /// </summary>
    public async Task<List<T>> SendRequest<T>(string path, string item)
    {
        return _jsonParser.ParseRaw<T>(await getAsync(path), item);
    }

    /// <summary>
    /// Выполняет GET-запрос и возвращает сырой JSON-ответ
    /// </summary>
    public async Task<string> GetRawAsync(string path)
    {
        using var response = await sharedClient.GetAsync(path);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    // Внутренний метод выполнения запроса
    private async Task<string> getAsync(string path)
    {
        _dbg.Log(string.Format(_loc.Text["out_api_send"], path));

        using var response = await sharedClient.GetAsync(path);

        if (response.IsSuccessStatusCode)
            _dbg.Log(string.Format(_loc.Text["out_api_succ"], response.StatusCode));
        else
            _dbg.Error(string.Format(_loc.Text["out_api_badr"], response.StatusCode));

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    // Загрузка токена авторизации из конфигурации
    private void loadToken()
    {
        var token = _configuration["ApiSettings:Token"];

        if (string.IsNullOrEmpty(token))
            throw new Exception("Token not found in configuration!");

        sharedClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        _dbg.Log(string.Format(_loc.Text["out_api_token_succ"], "token.uni.s"));
    }
}