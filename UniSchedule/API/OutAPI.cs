// ┌────────────────────────────────────────────────────────────────────────────┐
// │ UniSchedule                                                                │
// │ WebAPI расширение для отслеживания расписания учебных занятий              │
// ├────────────────────────────────────────────────────────────────────────────┤
// │ Файл: OutAPI.cs                                                            │
// │ Описание: Класс для работы с внешним api                                   │
// └────────────────────────────────────────────────────────────────────────────┘

using UniSchedule.Json;
using UniSchedule.System;

namespace UniSchedule.API{
  /// <summary>
  /// Класс для работы с внешним api
  /// </summary>
  public class OutAPI{
    private static HttpClient sharedClient = new HttpClient();
    private readonly string url;
    private readonly JsonParser _jsonParser;
    private readonly Debug _dbg;
    private readonly Localization _loc;

    /// <summary>
    /// Конструктор класса
    /// </summary>
    public OutAPI(IConfiguration configuration, JsonParser jsonParser, Debug dbg, Localization loc)
    {
      _jsonParser = jsonParser;
      _dbg = dbg;
      _loc = loc;
      // Проверяем наличие строки подклюения к api
      url = configuration.GetConnectionString("OutAPI_URL")??
        throw new Exception("Out API URL not found!"); // Кидаем исключение если не нашли строку
      sharedClient.BaseAddress = new Uri(url); // Даем http клиенту точный адрес
      sharedClient.DefaultRequestHeaders.Add("accept", "*/*"); // Добавляем необходимый header
      this.loadToken(); // Загружаем токен из файла
    }

    /// <summary>
    /// Отправляет запрос на внешний api
    /// </summary>
    /// <param name="path">Путь запроса</param>
    /// <param name="item">Элемент json</param>
    /// <returns>Список моделей json</returns>
    public async Task<List<T>> SendRequest<T>(string path, string item) =>
      _jsonParser.ParseRaw<T>((await this.getAsync(path)), item);

    // Общий метод отправки запросов на внешний api
    private async Task<string> getAsync(string path){
      // Отправляем запрос
      _dbg.Log(string.Format(_loc.Text["out_api_send"], path));
      using HttpResponseMessage response = await sharedClient.GetAsync(path);

      // Информируем о результате запроса во внешний api
      if(response.IsSuccessStatusCode)
        _dbg.Log(string.Format(_loc.Text["out_api_succ"], response.StatusCode));
      else
        _dbg.Error(string.Format(_loc.Text["out_api_badr"], response.StatusCode));

      response.EnsureSuccessStatusCode(); // Ожидаем положительный ответ от сервера
      // Считываем данные ответа
      var jsonResponse = await response.Content.ReadAsStringAsync(); 
      
      return jsonResponse; // Возвращаем результат
    }

    /// Загрузка токена из внешнего файла token.uni.s
    private void loadToken(){
      if(!File.Exists("token.uni.s")) // Проверяем наличие файла
        throw new FileNotFoundException(string.Format(_loc.Text["out_api_token_file_not_found"], "token.uni.s")); 
      // Считываем байты из файла
      byte[] data = File.ReadAllBytes("token.uni.s");
      var token = Crypto.Decrypt(data); // Дешифруем токен
      // Заполняем заголовок клиента ключом
      sharedClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}"); 
      _dbg.Log(string.Format(_loc.Text["out_api_token_succ"], "token.uni.s"));
    }
  }
}
