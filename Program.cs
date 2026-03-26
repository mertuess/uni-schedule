using UniSchedule;
using UniSchedule.API;
using UniSchedule.Requests;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSingleton<DBManager>();
builder.Services.AddSingleton<OutAPI>();

var app = builder.Build();

var db = app.Services.GetRequiredService<DBManager>();
var o_api = app.Services.GetRequiredService<OutAPI>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.MapGet("/", async context =>
{
    await UniSchedule.DataManager.LoadAll(o_api);
    await UniSchedule.DataManager.UpdateGroups(o_api,
            DataManager.Facultes.Where(x=>x.fac_id==3).First(),
            DataManager.Courses.Where(x=>x.course_id == 3).First());
    await UniSchedule.DataManager.UpdateDates(o_api, DataManager.Groups.First());
    string data = "";

    using (StreamReader reader = new StreamReader(@"./wwwroot/index.html"))
    {
        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            if(line.Contains("UI_INST")){
                DataManager.Facultes.ForEach(x => {data += $"<option value=\"{x.fac_id}\">{x.facultee}</option>";});
            }
            if(line.Contains("UI_COURSE")){
                DataManager.Courses.ForEach(x => {data += $"<option value=\"{x.course_id}\">{x.course}</option>";});
            }
            if(line.Contains("UI_GROUP")){
                DataManager.Groups.ForEach(x => {data += $"<option value=\"{x.group_id}\">{x.group}</option>";});
            }
            if(line.Contains("UI_WEEK")){
                DataManager.CurrentDates.ForEach(x => {data += $"<option value=\"{x}\">{x}</option>";});
            }

            data += line;
        }
    }
    context.Response.ContentType = "text/html";
    await context.Response.WriteAsync(data);
});

app.MapPost("/api/login", async (HttpContext httpContext) =>
{
    var request = await httpContext.Request.ReadFromJsonAsync<LoginRequest>();
    if (request == null) return Results.BadRequest();

    var db = httpContext.RequestServices.GetRequiredService<DBManager>();
    bool isValid = (db.TryLogin(request.Mail, request.Password) != null)? true : false;

    if (isValid)
    {
        var user = db.FindUser(request.Mail);
        return Results.Ok(new { user?.Mail, user?.Role });
    }

    return Results.Unauthorized();
});

// app.MapGet("/courses", async context => {
//     string json = await o_api.GetCoursesAsync();
//     context.Response.ContentType = "text/json";
//     await context.Response.WriteAsync(json);
// });

app.Run();

