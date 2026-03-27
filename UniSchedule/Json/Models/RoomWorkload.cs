namespace UniSchedule.Json.Models{
  public class RoomWorkload{
    public string room { get; set; } = string.Empty;
    public int workload_percent { get; set; } = 0;
    public Dictionary<string, bool[]> workload { get; set; } = new Dictionary<string, bool[]>();
  }
}
