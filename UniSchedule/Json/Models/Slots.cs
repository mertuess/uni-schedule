namespace UniSchedule.Json.Models{
  public class Slots{
    public string date { get; set; } = string.Empty;
    public List<string> slot_list { get; set; } = new List<string>();

    public Slots(){}

    public Slots(string date, List<string> slot_list){
      this.date = date;
      this.slot_list = slot_list;
    }
  }
}
