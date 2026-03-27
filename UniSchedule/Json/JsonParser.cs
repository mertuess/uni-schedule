// ┌────────────────────────────────────────────────────────────────────────────┐
// │ UniSchedule                                                                │
// │ WebAPI расширение для отслеживания расписания учебных занятий              │
// ├────────────────────────────────────────────────────────────────────────────┤
// │ Файл: JsonParser.cs                                                        │
// │ Описание: Класс отвечающий за обработку json строк                         │
// └────────────────────────────────────────────────────────────────────────────┘

// Использования
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

/// <summary>
/// Пространство имен для работы с Json строками
/// </summary>
namespace UniSchedule.Json{
  /// <summary>
  /// Класс для работы с Json
  /// </summary>
  public class JsonParser{
    /// <summary>
    /// Конструктор
    /// </summary>
    public JsonParser(IConfiguration configuration){}

    /// <summary>
    /// Метод преобразующий сырую json строку с внешнего api в список моделей
    /// </summary>
    /// <param name="raw_json">Сырая строка json</param>
    /// <param name="requested_item">Запрашиваемый объект</param>
    /// <returns>Список моделей</returns>
    public List<T> ParseRaw<T>(string raw_json, string requested_item){
      List<T> result = new List<T>(); // Создаем новый список необходимых моделей
      var data = JObject.Parse(raw_json); // Получаем объект Json из сырых данных
      var requested = data[requested_item] as JArray; // Конвертируем в массив JSON по запрашиваему объекту
      if(requested==null) return new List<T>(); // Возвращаем пустой список если объета нет
      // Возвращаем список моделей
      return requested.ToObject<List<T>>()?? throw new Exception("Ошибка парсирования json таблицы");
    }

    /// <summary>
    /// Метод преобразующий объект в json строку
    /// </summary>
    /// <param name="list">Объект для сериализации</param>
    /// <returns>Json строка</returns>
    public string Serialize(object? list){
      return JsonConvert.SerializeObject(list);
    }
  }
}
