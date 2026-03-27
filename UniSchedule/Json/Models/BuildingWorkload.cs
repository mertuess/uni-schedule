namespace UniSchedule.Json.Models{
  public class BuildingWorkload{
    public string building { get; set; } = string.Empty;
    public int workload_percent { get; set; } = 0;
    public List<RoomWorkload> workload { get; set; } = new List<RoomWorkload>();
  }
}
