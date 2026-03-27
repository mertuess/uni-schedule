// ┌────────────────────────────────────────────────────────────────────────────┐
// │ UniSchedule                                                                │
// │ WebAPI расширение для отслеживания расписания учебных занятий              │
// ├────────────────────────────────────────────────────────────────────────────┤
// │ Файл: API.cs                                                               │
// │ Описание: Реализация классов работы с приложением как с API                │
// └────────────────────────────────────────────────────────────────────────────┘

/// <summary>
/// Пространство имен для определения API классов
/// </summary>
namespace UniSchedule.API{
  /// <summary>
  /// Класс реализующий ответы на запросы
  /// </summary>
  public class API{
    /// <summary>
    /// Ссылка на экземпляр класса работы с внешним api
    /// </summary>
    private readonly OutAPI o_api;

    /// <summary>
    /// Конструктор класса
    /// </summary>
    /// <param name="o_api">Экземпляр класса работы с внешним api</param>
    public API(OutAPI o_api){
      this.o_api = o_api;
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

    public async Task<string> GetTeachersMassSchedule(Json.JsonParser jsp, string[] UIDs, Json.Models.Week week){
      List<List<Json.Models.TeacherSchedule>> schedules = new List<List<Json.Models.TeacherSchedule>>();
      foreach(string uid in UIDs){
        List<Json.Models.TeacherSchedule> s = await o_api.RequestTeacherSchedule(jsp, uid, week);
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
      
      return jsp.Serialize(common_free_slots);
    }

    public async Task<Json.Models.RoomWorkload> GetRoomWorkload(
      Json.JsonParser jsp,
      int room_id,
      Json.Models.Week week){
      Json.Models.RoomWorkload result = new Json.Models.RoomWorkload();
      var schedule = await o_api.RequestRoomSchedule(jsp, room_id, week);
      
      if(schedule==null||schedule.Count < 1) return new Json.Models.RoomWorkload();

      result.room = schedule.First().room;
      result.workload = await ConvertToScheduleDictionary(schedule);
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

    public async Task<Json.Models.BuildingWorkload> GetBuildingWorkload(Json.JsonParser jsp,
        int bui_id,
        Json.Models.Week week){
      var buildings = await o_api.RequestBuildingsList(jsp);
      var rooms = await o_api.RequestRoomsList(jsp, bui_id);
      var result = new Json.Models.BuildingWorkload();

      int total = 0, count = 0;

      result.building = buildings.First(x => x.bui_id==bui_id).building;
      foreach(var r in rooms){
        var rwl = await GetRoomWorkload(jsp, r.room_id, week);
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

    public async Task<List<Json.Models.BuildingWorkload>> GetBuildingsWorkload(
      Json.JsonParser jsp,
      int[] bui_ids,
      Json.Models.Week week){
      if(bui_ids.Length < 1) return new List<Json.Models.BuildingWorkload>();
      List<Json.Models.BuildingWorkload> result = new List<Json.Models.BuildingWorkload>();
      var buildings = (await o_api.RequestBuildingsList(jsp))
        .Where(x => bui_ids.ToList<int>()
        .Contains(x.bui_id)).ToList();
      
      foreach(var b in buildings)
        result.Add(await GetBuildingWorkload(jsp, b.bui_id, week));

      return result;
    }

    private async Task<Dictionary<string, bool[]>> ConvertToScheduleDictionary(
        List<Json.Models.TeacherSchedule> teacherSchedules){
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
