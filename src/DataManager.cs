using Newtonsoft.Json;
using UniSchedule.API;

namespace UniSchedule
{
    static public class DataManager
    {
        static public List<UniSchedule.Models.Facult>? Facultes;
        static public List<UniSchedule.Models.Course>? Courses;
        static public List<UniSchedule.Models.Teacher>? Teachers;
        static public List<UniSchedule.Models.Group>? Groups;
        static public List<string>? CurrentDates;

        static public async Task UpdateDates(OutAPI o_api, UniSchedule.Models.Group group){
            string json = await o_api.GetDatesAsync(group.UID);
            CurrentDates = JsonConvert.DeserializeObject<List<string>>(json)?? throw new Exception("");
        }

        static public async Task LoadAll(OutAPI o_api){
            string faculties_json = await o_api.GetFacultiesAsync();
            string courses_json = await o_api.GetCoursesAsync();
            string teachers_json = await o_api.GetTeachersAsync();
            Facultes = JsonConvert.DeserializeObject<List<UniSchedule.Models.Facult>>(faculties_json)?? throw new Exception("");
            Courses = JsonConvert.DeserializeObject<List<UniSchedule.Models.Course>>(courses_json)?? throw new Exception("");
            Teachers = JsonConvert.DeserializeObject<List<UniSchedule.Models.Teacher>>(teachers_json)?? throw new Exception("");
    
            foreach(var f in Facultes){
                string groups_json = await o_api.GetAllGroupsAsync(f.fac_id);
                List<UniSchedule.Models.Group> c_groups = 
                    JsonConvert.DeserializeObject<List<UniSchedule.Models.Group>>(groups_json)?? throw new Exception("");
                Groups?.AddRange(c_groups);
            }
        }
    }
}
