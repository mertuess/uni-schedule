using SQLite;

namespace UniSchedule.DataBase.Models
{

    public class TDR
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int teacher_id { get; set; }

        public string Department {  get; set; } = string.Empty;

    }
}

