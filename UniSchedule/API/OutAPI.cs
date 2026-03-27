// ┌────────────────────────────────────────────────────────────────────────────┐
// │ UniSchedule                                                                │
// │ WebAPI расширение для отслеживания расписания учебных занятий              │
// ├────────────────────────────────────────────────────────────────────────────┤
// │ Файл: OutAPI.cs                                                            │
// │ Описание: Класс для работы с внешним api                                   │
// └────────────────────────────────────────────────────────────────────────────┘

using UniSchedule.Json;
using UniSchedule.Json.Models;

/// <summary>
/// Пространство имен для определения API классов
/// </summary>
namespace UniSchedule.API{
  /// <summary>
  /// Класс для работы с внешним api
  /// </summary>
  public class OutAPI{
    private readonly string url;
    private static HttpClient sharedClient = new HttpClient();
    private readonly JsonParser _jsonParser;

    /// <summary>
    /// Конструктор класса
    /// </summary>
    public OutAPI(IConfiguration configuration, JsonParser jsonParser)
    {
      // Проверяем наличие строки подклюения к api
      url = configuration.GetConnectionString("OutAPI_URL")??
        throw new Exception("Out API URL not found!"); // Кидаем исключение если не нашли строку
      sharedClient.BaseAddress = new Uri(url); // Даем http клиенту точный адрес
      sharedClient.DefaultRequestHeaders.Add("accept", "*/*"); // Добавляем необходимый header
      _jsonParser = jsonParser;
      LoadToken(); // Загружаем токен из файла
    }

    /// <summary>
    /// Загрузка токена из внешнего файла token.uni.s
    /// </summary>
    public void LoadToken(){
      if(!File.Exists("token.uni.s")) // Проверяем наличие файла
        throw new Exception("Файл токена не найден"); // Выбрасываем исключение если не нашли файл
      // Считываем байты из файла
      byte[] data = File.ReadAllBytes("token.uni.s");
      var token = Crypto.Decrypt(data); // Дешифруем токен
      // Заполняем заголовок клиента ключом
      sharedClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}"); 
    }

    /// <summary>
    /// Запрос на получения всего списка преподавателей
    /// </summary>
    /// <returns>Список преподавателей</returns>
    public async Task<List<Teacher>> RequestTeachersList(){
      var raw_json = await this.getAsync("teachers");
      return _jsonParser.ParseRaw<Teacher>(raw_json, "teachers");
    }

    /// <summary>
    /// Запрос на получение списка преподавателей по фильру имени
    /// </summary>
    /// <param name="name">Часть имени преподавателя</param>
    /// <returns>Список преподавателей</returns>
    public async Task<List<Teacher>> RequestTeachersListByName(string name){
      var raw_json = await this.getAsync($"teachers/search?name={name}");
      return _jsonParser.ParseRaw<Teacher>(raw_json, "teachers");
    }

    /// <summary>
    /// Запрос на получение списка дат по которым у преподавателя есть занятия
    /// </summary>
    /// <param name="uid">UID преподавателя</param>
    /// <returns>Список дат в формате "yyyy-MM-dd"</returns>
    public async Task<List<string>> RequestTeacherDates(string uid){
      var raw_json = await this.getAsync($"teachers/{uid}/dates");
      return _jsonParser.ParseRaw<string>(raw_json, "dates");
    }

    /// <summary>
    /// Формирование из списка дат конечных неделей
    /// </summary>
    /// <param name="uid">UID препододавателя</param>
    /// <returns>Список недель (см. модель Week)</returns>
    public async Task<List<Week>> RequestTeacherWeeks(string uid){
      List<string> dateStrings = await RequestTeacherDates(uid);
      return Week.GenerateWeeksByDates(dateStrings);
    }

    /// <summary>
    /// Запрос на получение расписание преподавателя на неделю
    /// </summary>
    /// <param name="uid">UID препододавателя</param>
    /// <param name="week">Неделя расписание на которую получаем</param>
    /// <returns>Список расписания преподавателя (см. модель TeacherSchedule)</returns>
    public async Task<List<TeacherSchedule>> RequestTeacherSchedule(string uid, Week week){
      var raw_json = await this.getAsync($"teachers/{uid}/schedule/{week.start}/{week.end}");
      return _jsonParser.ParseRaw<TeacherSchedule>(raw_json, "timetable");
    }

    public async Task<List<Building>> RequestBuildingsList(){
      var raw_json = await this.getAsync($"buildings");
      return _jsonParser.ParseRaw<Building>(raw_json, "buildings");
    }

    public async Task<List<Room>> RequestRoomsList(int bui_id){
      var raw_json = await this.getAsync($"buildings/{bui_id}/rooms");
      return _jsonParser.ParseRaw<Room>(raw_json, "rooms");
    }

    public async Task<List<TeacherSchedule>> RequestRoomSchedule(int room_id, Week week){
      var raw_json = await this.getAsync($"rooms/{room_id}/schedule/{week.start}/{week.end}");
      return _jsonParser.ParseRaw<TeacherSchedule>(raw_json, "timetable");
    }

    // Общий метод отправки запросов на внешний api
    private async Task<string> getAsync(string path){
      // Отправляем запрос
      using HttpResponseMessage response = await sharedClient.GetAsync(path); 
      response.EnsureSuccessStatusCode(); // Ожидаем положительный ответ от сервера
      // Считываем данные ответа
      var jsonResponse = await response.Content.ReadAsStringAsync(); 
      return jsonResponse; // Возвращаем результат
    }
  }
}
