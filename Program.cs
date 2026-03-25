using UniSchedule;
using UniSchedule.API;
using UniSchedule.Requests;
using UniSchedule.Models;
using Newtonsoft.Json;

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
    string faculties_json = await o_api.GetFacultiesAsync();
    List<Facult> faculties = JsonConvert.DeserializeObject<List<Facult>>(faculties_json)?? throw new Exception("");
    string data = "";

    using (StreamReader reader = new StreamReader(@"./wwwroot/index.html"))
    {
        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            if(line.Contains("UI_INST")){
                faculties.ForEach(x => {data += $"<option value=\"{x.fac_id}\">{x.facultee}</option>";});
            }
            data += line;
        }
    context.Response.ContentType = "text/html";
    }
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

