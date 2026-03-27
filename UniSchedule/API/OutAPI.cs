// ┌────────────────────────────────────────────────────────────────────────────┐
// │ UniSchedule                                                                │
// │ WebAPI расширение для отслеживания расписания учебных занятий              │
// ├────────────────────────────────────────────────────────────────────────────┤
// │ Файл: OutAPI.cs                                                            │
// │ Описание: Класс для работы с внешним api                                   │
// └────────────────────────────────────────────────────────────────────────────┘
// Подключения
using UniSchedule.Json;

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

    /// <summary>
    /// Конструктор класса
    /// </summary>
    public OutAPI(IConfiguration configuration)
    {
      // Проверяем наличие строки подклюения к api
      url = configuration.GetConnectionString("OutAPI_URL")??
        throw new Exception("Out API URL not found!"); // Кидаем исключение если не нашли строку
      sharedClient.BaseAddress = new Uri(url); // Даем http клиенту точный адрес
      sharedClient.DefaultRequestHeaders.Add("accept", "*/*"); // Добавляем необходимый header
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
    /// <param name="jsp">Экземпляр парсера json</param>
    /// <returns>Список преподавателей</returns>
    public async Task<List<Json.Models.Teacher>> RequestTeachersList(JsonParser jsp){
      var raw_json = await this.getAsync("teachers");
      return jsp.ParseRaw<Json.Models.Teacher>(raw_json, "teachers");
    }

    /// <summary>
    /// Запрос на получение списка преподавателей по фильру имени
    /// </summary>
    /// <param name="jsp">Экземпляр парсера json</param>
    /// <param name="name">Часть имени преподавателя</param>
    /// <returns>Список преподавателей</returns>
    public async Task<List<Json.Models.Teacher>> RequestTeachersListByName(JsonParser jsp, string name){
      var raw_json = await this.getAsync($"teachers/search?name={name}");
      return jsp.ParseRaw<Json.Models.Teacher>(raw_json, "teachers");
    }

    /// <summary>
    /// Запрос на получение списка дат по которым у преподавателя есть занятия
    /// </summary>
    /// <param name="jsp">Экземпляр парсера json</param>
    /// <param name="uid">UID преподавателя</param>
    /// <returns>Список дат в формате "yyyy-MM-dd"</returns>
    public async Task<List<string>> RequestTeacherDates(JsonParser jsp, string uid){
      var raw_json = await this.getAsync($"teachers/{uid}/dates");
      return jsp.ParseRaw<string>(raw_json, "dates");
    }

    /// <summary>
    /// Формирование из списка дат конечных неделей
    /// </summary>
    /// <param name="jsp">Экземпляр парсера json</param>
    /// <param name="uid">UID препододавателя</param>
    /// <returns>Список недель (см. модель Week)</returns>
    public async Task<List<Json.Models.Week>> RequestTeacherWeeks(JsonParser jsp, string uid){
      List<string> dateStrings = await RequestTeacherDates(jsp, uid);
      return Json.Models.Week.GenerateWeeksByDates(dateStrings);
    }

    /// <summary>
    /// Запрос на получение расписание преподавателя на неделю
    /// </summary>
    /// <param name="jsp">Экземпляр парсера json</param>
    /// <param name="uid">UID препододавателя</param>
    /// <param name="week">Неделя расписание на которую получаем</param>
    /// <returns>Список расписания преподавателя (см. модель TeacherSchedule)</returns>
    public async Task<List<Json.Models.TeacherSchedule>> RequestTeacherSchedule(JsonParser jsp, string uid, Json.Models.Week week){
      var raw_json = await this.getAsync($"teachers/{uid}/schedule/{week.start}/{week.end}");
      return jsp.ParseRaw<Json.Models.TeacherSchedule>(raw_json, "timetable");
    }

    public async Task<List<Json.Models.Building>> RequestBuildingsList(JsonParser jsp){
      var raw_json = await this.getAsync($"buildings");
      return jsp.ParseRaw<Json.Models.Building>(raw_json, "buildings");
    }

    public async Task<List<Json.Models.Room>> RequestRoomsList(JsonParser jsp, int bui_id){
      var raw_json = await this.getAsync($"buildings/{bui_id}/rooms");
      return jsp.ParseRaw<Json.Models.Room>(raw_json, "rooms");
    }

    public async Task<List<Json.Models.TeacherSchedule>> RequestRoomSchedule(JsonParser jsp,
        int room_id, Json.Models.Week week){
      var raw_json = await this.getAsync($"rooms/{room_id}/schedule/{week.start}/{week.end}");
      return jsp.ParseRaw<Json.Models.TeacherSchedule>(raw_json, "timetable");
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
