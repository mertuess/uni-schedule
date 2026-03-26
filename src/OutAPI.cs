namespace UniSchedule.API{
  public class OutAPI{
    private readonly string url;
    private static HttpClient sharedClient = new HttpClient();

    public OutAPI(IConfiguration configuration)
    {
      url = configuration.GetConnectionString("OutAPI_URL")?? throw new Exception("Out API URL not found!");
      sharedClient.BaseAddress = new Uri(url);
      sharedClient.DefaultRequestHeaders.Add("accept", "*/*");
      sharedClient.DefaultRequestHeaders.Add("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI19347593j");
    }

    public async Task<string> GetCoursesAsync()
    {
        return await getAsync("courses");
    }
    
    public async Task<string> GetFacultiesAsync()
    {
        return await getAsync("faculties");
    }
    
    public async Task<string> GetAllGroupsAsync(int fac_ID)
    {
        return await getAsync($"{fac_ID}/groups");
    }

    public async Task<string> GetDatesAsync(string groupUID)
    {
        return await getAsync($"groups/{groupUID}/dates");
    }

    public async Task<string> GetMainGroupAsync(int fac_ID)
    {
        return await getAsync($"faculties/{fac_ID}/groups/main");
    }
    public async Task<string> GetAllGroupsCourseAsync(int fac_ID, int course_id)
    {
        return await getAsync($"faculties/{fac_ID}/courses/{course_id}/groups");
    }

    public async Task<string> GetMainGroupsCourseAsync(int fac_ID, int course_id)
    {
        return await getAsync($"faculties/{fac_ID}/courses/{course_id}/groups/main");
    }

    public async Task<string> GetSubbgrupsAsync(string UID)
    {
        return await getAsync($"/groups/{UID}/subgroups");
    }

    public async Task<string> GetDataGroupAsync(string UID)
    {
        return await getAsync($"grous/{UID}/date");
    }
    
    public async Task<string> GetScheduleTodayAsync(string UID)
    {
        return await getAsync($"grous/{UID}/schedule/tuday");
    }

    public async Task<string> GetScheduleRangeAsync(string UID, string start, string end)
    {
       //format start, end = “ $yyyy-mm-dd”
       return await getAsync($"grous/{UID}/schedule/{start}/{end}");
    }

    public async Task<string> GetTeachersAsync()
    {
       return await getAsync($"/teachers");
    }

    public async Task<string> GetTeachersSearchAsync(string name)
    {
       return await getAsync($"/teachers/search");
    }

    public async Task<string> GetTeachersDateAsync(string UID_t)
    {
       return await getAsync($"/teachers/{UID_t}/dates");
    }

    public async Task<string> GetTeacherScheduleToday( string UID_t )
    {
       return await getAsync($"/teachers/{UID_t}/schedule/today");
    }

    public async Task<string> GetTeacherScheduleRange( string UID_t, string start, string end)
    {
       return await getAsync($"/teachers/{UID_t}/schedule/{start}/{end}");
    }

    private async Task<string> getAsync(string path)
    {
        using HttpResponseMessage response = await sharedClient.GetAsync(path);
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync();
        Console.WriteLine(jsonResponse);
        return jsonResponse;
    }
  }
}
