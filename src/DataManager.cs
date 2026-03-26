using Newtonsoft.Json;
using UniSchedule.API;

namespace UniSchedule
{
    static public class DataManager
    {
        static public List<UniSchedule.Models.Facult> Facultes = new List<Models.Facult>();
        static public List<UniSchedule.Models.Course> Courses = new List<Models.Course>();
        static public List<UniSchedule.Models.Teacher> Teachers = new List<Models.Teacher>();
        static public List<UniSchedule.Models.Group> Groups = new List<Models.Group>();
        static public List<string> CurrentDates = new List<string>();

        static public async Task UpdateGroups(OutAPI o_api, Models.Facult facult, Models.Course course){
            Groups.Clear();
            string groups_json = await o_api.GetAllGroupsAsync(facult.fac_id);
            List<UniSchedule.Models.Group> c_groups = 
                JsonConvert.DeserializeObject<List<UniSchedule.Models.Group>>(groups_json)?? throw new Exception("");
            Groups.AddRange(c_groups.Where(x => x.course_id == course.course_id));
        }

        static public async Task UpdateDates(OutAPI o_api, UniSchedule.Models.Group group){
            string json = await o_api.GetDatesAsync(group.UID);
            CurrentDates = JsonConvert.DeserializeObject<List<string>>(json)?? throw new Exception("");
        }

        static public async Task LoadAll(OutAPI o_api){
            if(Courses.Count > 0) return;

            Console.WriteLine("Get faculties list");
            string faculties_json = await o_api.GetFacultiesAsync();
            Console.WriteLine("Get courses list");
            string courses_json = await o_api.GetCoursesAsync();
            Console.WriteLine("Get teachers list");
            string teachers_json = await o_api.GetTeachersAsync();
            Facultes = JsonConvert.DeserializeObject<List<UniSchedule.Models.Facult>>(faculties_json)?? throw new Exception("");
            Courses = JsonConvert.DeserializeObject<List<UniSchedule.Models.Course>>(courses_json)?? throw new Exception("");
            Teachers = JsonConvert.DeserializeObject<List<UniSchedule.Models.Teacher>>(teachers_json)?? throw new Exception("");
        }
    }
}
