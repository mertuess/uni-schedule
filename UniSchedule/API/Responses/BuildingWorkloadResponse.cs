using UniSchedule.Json.Models;

namespace UniSchedule.API.Responses{
  public class BuildingWorkloadResponse: Response{
    private int bui_id;
    private Week week;

    public BuildingWorkloadResponse(OutAPI o_api, int bui_id, Week week) : base (o_api){
      this.bui_id = bui_id;
      this.week = week;
    }

    public async Task<BuildingWorkload> GetBuildingWorkload(){
      List<Building> buildings = await _o_api.SendRequest<Building>(
          $"buildings", "buildings");
      List<Room> rooms = await _o_api.SendRequest<Room>(
          $"buildings/{bui_id}/rooms", "rooms");
      var result = new BuildingWorkload();

      int total = 0, count = 0;

      result.building = buildings.First(x => x.bui_id==bui_id).building;
      foreach(var r in rooms){
        var room_response = new RoomWorkloadResponse(_o_api, r.room_id, week);
        var rwl = await room_response.GetRoomWorkload();
        if(rwl == null) continue;
        foreach(var p in rwl.workload){
          foreach(bool s in p.Value.Values){
            total++;
            if(s) count++;
          }
        }
        result.workload.Add(rwl);
      }
      result.workload_percent = (int)((double)count / total * 100);
      return result;
    }
  }
}
