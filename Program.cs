using UniSchedule;
using System.Data.SQLite;
using uni_schedule.src;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));

var app = builder.Build();

//Объявление БД
var db = new DBManager();


db.AddTestUsers();

// Все пользователи в бд
Console.WriteLine("\nВсе пользователи в БД:");
var users = db.GetAllUsers();
foreach (var user in users)
{
    Console.WriteLine($"ID: {user.Id}, Почта: {user.Mail}, Роль: {user.Role}");
}

// Проверяем вход
Console.WriteLine("\nПроверка входа:");
Console.WriteLine($"admin@mauniver.ru / admin123: {db.CheckPassword("admin@mauniver.ru", "admin123")}");
Console.WriteLine($"admin@mauniver.ru / wrong: {db.CheckPassword("admin@mauniver.ru", "wrong")}");
Console.WriteLine($"user@mauniver.ru / user123: {db.CheckPassword("user@mauniver.ru", "user123")}");

// Ищем пользователя
var user1 = db.FindUser("admin@mauniver.ru");
if (user1 != null)
{
    Console.WriteLine($"\nНайден пользователь: {user1.Mail} (роль: {user1.Role})");
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

app.Run();
