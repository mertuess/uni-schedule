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

    public async Task<string> GetTeachersAsync()
    {
        return await getAsync("teachers");
    }
    
    public async Task<string> GetAllGroupsAsync(int fac_ID)
    {
        return await getAsync($"faculties/{fac_ID}/groups");
    }

    public async Task<string> GetDatesAsync(string groupUID)
    {
        return await getAsync($"groups/{groupUID}/dates");
    }

    private async Task<string> getAsync(string path)
    {
        using HttpResponseMessage response = await sharedClient.GetAsync(path);
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync();
        return jsonResponse;
    }
  }
}
