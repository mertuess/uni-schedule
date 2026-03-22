using Microsoft.AspNetCore.Identity.Data;
using System.Data.SQLite;
using uni_schedule.src;
using UniSchedule;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));
builder.Services.AddSingleton<DBManager>();

var app = builder.Build();

// Получаем DBManager
var db = app.Services.GetRequiredService<DBManager>();

db.AddTestUsers();

// Все пользователи в бд
Console.WriteLine("\nВсе пользователи в БД:");
var users = db.GetAllUsers();
foreach (var user in users)
{
    Console.WriteLine($"ID: {user.Id}, Почта: {user.Mail}, Роль: {user.Role}");
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.MapGet("/", async context =>
{
    string data = "";

    using (StreamReader reader = new StreamReader(@"./wwwroot/index.html"))
    {
        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            data += line;
        }
    }
    context.Response.ContentType = "text/html";
    await context.Response.WriteAsync(data);
});

// API для логина
app.MapPost("/api/login", async (HttpContext httpContext) =>
{
    var request = await httpContext.Request.ReadFromJsonAsync<LoginRequest>();
    if (request == null) return Results.BadRequest();

    var db = httpContext.RequestServices.GetRequiredService<DBManager>();
    var isValid = db.CheckPassword(request.Mail, request.Password);

    if (isValid)
    {
        var user = db.FindUser(request.Mail);
        return Results.Ok(new { user?.Mail, user?.Role });
    }

    return Results.Unauthorized();
});

app.Run();

public class LoginRequest
{
    public string Mail { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
