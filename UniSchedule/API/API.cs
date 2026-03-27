// ┌────────────────────────────────────────────────────────────────────────────┐
// │ UniSchedule                                                                │
// │ WebAPI расширение для отслеживания расписания учебных занятий              │
// ├────────────────────────────────────────────────────────────────────────────┤
// │ Файл: API.cs                                                               │
// │ Описание: Реализация классов работы с приложением как с API                │
// └────────────────────────────────────────────────────────────────────────────┘

using UniSchedule.Json;
using UniSchedule.Json.Models;

/// <summary>
/// Пространство имен для определения API классов
/// </summary>
namespace UniSchedule.API{
  /// <summary>
  /// Класс реализующий ответы на запросы
  /// </summary>
  public class API{
    /// <summary>
    /// Экземпляр класса работы с внешним api
    /// </summary>
    private readonly OutAPI o_api;

    /// <summary>
    /// Экземпляр класса работы с json
    /// </summary>
    private readonly JsonParser _jsonParser;

    /// <summary>
    /// Конструктор класса
    /// </summary>
    /// <param name="o_api">Экземпляр класса работы с внешним api</param>
    /// <param name="jsonParser">Экземпляр json парсера</param>
    public API(OutAPI o_api, JsonParser jsonParser){
      this.o_api = o_api;
      this._jsonParser = jsonParser;
    }

    /// <summary>
    /// Ассинхронный метод для обработки GET запроса "/index.html"
    /// </summary>
    public async Task<string> Main(){
      string data = "";

      using (StreamReader reader = new StreamReader(@"./wwwroot/index.html"))
      {
        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
          // if (line.Contains("UI_INST"))
          //   DataManager.Facultes.ForEach(x => { data += $"<option value=\"{x.fac_id}\">{x.facultee}</option>"; });
          // if (line.Contains("UI_COURSE"))
          //   DataManager.Courses.ForEach(x => { data += $"<option value=\"{x.course_id}\">{x.course}</option>"; });
          // if (line.Contains("UI_GROUP"))
          //   DataManager.Groups.ForEach(x => { data += $"<option value=\"{x.group_id}\">{x.group}</option>"; });
          // if (line.Contains("UI_WEEK"))
          //   DataManager.CurrentDates.ForEach(x => { data += $"<option value=\"{x}\">{x}</option>"; });

          data += line;
        }
      }
      return data;
    }

    /// <summary>
    /// Ассинхронный метод получения списка свободных часов у преподавателей
    /// </summary>
    /// <param name="UIDs">Массив UID преподавателей</param>
    /// <param name="week">Неделя</param>
    /// <returns>Json строка с расписанием</returns>
    public async Task<string> GetTeachersMassSchedule(string[] UIDs, Json.Models.Week week){
      List<List<TeacherSchedule>> schedules = new List<List<TeacherSchedule>>();
      foreach(string uid in UIDs){
        List<TeacherSchedule> s = await o_api.RequestTeacherSchedule(uid, week);
        schedules.Add(s);
      }

      Dictionary<string, List<string>> common_free_slots = new Dictionary<string, List<string>>();
      
      var allDates = schedules
          .SelectMany(teacherSchedule => teacherSchedule.Select(item => item.date))
          .Distinct()
          .OrderBy(d => d)
          .ToList();
      
      foreach (var date in allDates){
        var allOccupiedSlots = new HashSet<string>();
        
        foreach (var teacherSchedule in schedules){
          var occupiedSlotsOnDate = teacherSchedule
              .Where(item => item.date == date)
              .Select(item => item.slot);
          
          foreach (var slot in occupiedSlotsOnDate){
            allOccupiedSlots.Add(slot);
          }
        }
        
        var commonFreeSlots = Const.ALL_SLOTS
            .Where(slot => !allOccupiedSlots.Contains(slot))
            .ToList();
        
        if (commonFreeSlots.Any()){
          common_free_slots[date] = commonFreeSlots;
        }
      }
      
      return _jsonParser.Serialize(common_free_slots);
    }

    /// <summary>
    /// Ассинхронный метод получения загруженности аудитории
    /// </summary>
    /// <param name="room_id">ID аудитории</param>
    /// <param name="week">Неделя</param>
    /// <returns>Модель загруженности аудитории</returns>
    public async Task<RoomWorkload> GetRoomWorkload( int room_id, Week week){
      RoomWorkload result = new RoomWorkload();
      var schedule = await o_api.RequestRoomSchedule(room_id, week);
      
      if(schedule==null||schedule.Count < 1) return new RoomWorkload();

      result.room = schedule.First().room;
      result.workload = await convertToScheduleDictionary(schedule);
      int total = 0, count = 0;
      foreach(KeyValuePair<string, bool[]> p in result.workload){
        foreach(bool s in p.Value){
          total++;
          if(s) count++;
        }
      }
      result.workload_percent = (int)((double)count / total * 100);
      return result;
    }

    /// <summary>
    /// Ассинхронный метод получения загруженности корпуса
    /// </summary>
    /// <param name="bui_id">ID корпуса</param>
    /// <param name="week">Неделя</param>
    /// <returns>Модель загруженности корпуса</returns>
    public async Task<BuildingWorkload> GetBuildingWorkload(int bui_id, Week week){
      var buildings = await o_api.RequestBuildingsList();
      var rooms = await o_api.RequestRoomsList(bui_id);
      var result = new BuildingWorkload();

      int total = 0, count = 0;

      result.building = buildings.First(x => x.bui_id==bui_id).building;
      foreach(var r in rooms){
        var rwl = await GetRoomWorkload(r.room_id, week);
        foreach(KeyValuePair<string, bool[]> p in rwl.workload){
          foreach(bool s in p.Value){
            total++;
            if(s) count++;
          }
        }
        result.workload.Add(rwl);
      }
      result.workload_percent = (int)((double)count / total * 100);
      return result;
    }

    /// <summary>
    /// Ассинхронный метод получения загруженности множества корпусов
    /// </summary>
    /// <param name="bui_ids">ID корпусов</param>
    /// <param name="week">Неделя</param>
    /// <returns>Список моделей загруженности корпуса</returns>
    public async Task<List<BuildingWorkload>> GetBuildingsWorkload(int[] bui_ids, Week week){
      if(bui_ids.Length < 1) return new List<BuildingWorkload>();
      List<BuildingWorkload> result = new List<BuildingWorkload>();
      var buildings = (await o_api.RequestBuildingsList())
        .Where(x => bui_ids.ToList<int>()
        .Contains(x.bui_id)).ToList();
      
      foreach(var b in buildings)
        result.Add(await GetBuildingWorkload(b.bui_id, week));

      return result;
    }

    // Метод конвертации списка расписании в загруженность в виде словаря
    private async Task<Dictionary<string, bool[]>> convertToScheduleDictionary(List<TeacherSchedule> teacherSchedules){
      var result = new Dictionary<string, bool[]>();

      var groupedByDate = teacherSchedules
          .Where(s => !string.IsNullOrEmpty(s.date))
          .GroupBy(s => s.date);
      
      foreach (var dateGroup in groupedByDate){
          bool[] slots = new bool[7];
          
          foreach (var schedule in dateGroup){
              int slotIndex = Array.IndexOf(Const.ALL_SLOTS, schedule.slot);
              
              if (slotIndex >= 0 && slotIndex < slots.Length)
                  slots[slotIndex] = true;
          }
          
          result[dateGroup.Key] = slots;
      }
      
      return result;
    }
  }
}
