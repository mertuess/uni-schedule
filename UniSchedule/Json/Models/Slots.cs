namespace UniSchedule.Json.Models;

public class Slots
{
    public Slots()
    {
    }

    public Slots(string date, Dictionary<byte, string> slot_list)
    {
        this.date = date;
        this.slot_list = slot_list;
    }

    public string date { get; set; } = string.Empty;
    public Dictionary<byte, string> slot_list { get; set; } = new();
}