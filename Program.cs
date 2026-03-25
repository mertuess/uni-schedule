using UniSchedule;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var app = builder.Build();

//Объявление БД
var db = new DBManager("schedule.db");

db.TryAddUser("test@yandex.ru", "test123", "user");

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
