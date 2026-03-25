namespace UniSchedule.Models{
  public class Group{
    public int group_id { get; set; }
    public string group { get; set; } = "";
    public string speciality { get; set; } = "";
    public int course_id { get; set; }
    public int fac_id { get; set; }
    public int maingroup_id { get; set; }
    public string UID { get; set; } = "";
    public string UID_mg { get; set; } = "";
  }
}
