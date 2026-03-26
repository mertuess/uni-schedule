namespace UniSchedule.Models{
  class TeacherSchedule{
    public string pair_date { get; set; } = string.Empty;
    public string pair_time { get; set; } = string.Empty;
    public bool pair_first { get; set; }
    public string day_of_week { get; set; } = string.Empty;
    public string pair_type { get; set; } = string.Empty;
    public string disciplines { get; set; } = string.Empty;
    public string room { get; set; } = string.Empty;
    public string teacher { get; set; } = string.Empty;
    public List<string> subGroups { get; set; } = new List<string>();
  }

  class Schedule{
    public string pair_date { get; set; } = string.Empty;
    public string pair_time { get; set; } = string.Empty;
    public bool pair_first { get; set; }
    public string day_of_week { get; set; } = string.Empty;
    public string pair_type { get; set; } = string.Empty;
    public string disciplines { get; set; } = string.Empty;
    public string room { get; set; } = string.Empty;
    public string teacher { get; set; } = string.Empty;
    public bool isSubGroup { get; set; }
    public List<string> subGroups { get; set; } = new List<string>();
    public int id { get; set; }
  }
}
