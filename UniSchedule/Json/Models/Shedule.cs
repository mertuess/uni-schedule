namespace UniSchedule.Json.Models;

public class TeacherSchedule
{
    public string date { get; set; } = string.Empty;
    public string slot { get; set; } = string.Empty;
    public string day_of_week { get; set; } = string.Empty;
    public string type { get; set; } = string.Empty;
    public string disciplines { get; set; } = string.Empty;
    public string room { get; set; } = string.Empty;
    public string teacher { get; set; } = string.Empty;
    public List<string> subGroups { get; set; } = new();
    public int index { get; set; }
}

public class Schedule
{
    public string pair_date { get; set; } = string.Empty;
    public string pair_time { get; set; } = string.Empty;
    public bool pair_first { get; set; }
    public string day_of_week { get; set; } = string.Empty;
    public string pair_type { get; set; } = string.Empty;
    public string disciplines { get; set; } = string.Empty;
    public string room { get; set; } = string.Empty;
    public string teacher { get; set; } = string.Empty;
    public bool isSubGroup { get; set; }
    public List<string> subGroups { get; set; } = new();
    public int id { get; set; }
}