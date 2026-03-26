using UniSchedule;
using UniSchedule.API;
using UniSchedule.Requests;
using UniSchedule.Models;
using Newtonsoft.Json;
using System.Text.Json;

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
    List<Facult> faculties = JsonConvert.DeserializeObject<List<Facult>>(faculties_json) ?? throw new Exception("");
    string data = "";

    using (StreamReader reader = new StreamReader(@"./wwwroot/index.html"))
    {
        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (line.Contains("UI_INST"))
            {
                faculties.ForEach(x => { data += $"<option value=\"{x.fac_id}\">{x.facultee}</option>"; });
            }
            data += line;
        }
        context.Response.ContentType = "text/html";
    }
    await context.Response.WriteAsync(data);
});

// API для логина
app.MapPost("/api/login", async (HttpContext httpContext) =>
{
    var request = await httpContext.Request.ReadFromJsonAsync<LoginRequest>();
    if (request == null) return Results.BadRequest();

    var db = httpContext.RequestServices.GetRequiredService<DBManager>();
    bool isValid = (db.TryLogin(request.Mail, request.Password) != null) ? true : false;

    if (isValid)
    {
        var user = db.FindUser(request.Mail);
        return Results.Ok(new { user?.Mail, user?.Role });
    }

    return Results.Unauthorized();
});

// API: получить всех пользователей
app.MapGet("/api/users", (DBManager db) =>
{
    var users = db.GetAllUsers();
    return Results.Ok(users);
});

// API: получить пользователя по ID (только один раз!)
app.MapGet("/api/users/{id}", (int id, DBManager db) =>
{
    var user = db.GetAllUsers().FirstOrDefault(u => u.Id == id);
    if (user == null)
    {
        return Results.NotFound(new { message = "Пользователь не найден" });
    }
    return Results.Ok(user);
});

// API: создать пользователя
app.MapPost("/api/users", async (HttpContext httpContext) =>
{
    var request = await httpContext.Request.ReadFromJsonAsync<CreateUserRequest>();
    if (request == null) return Results.BadRequest(new { message = "Неверный запрос" });

    if (string.IsNullOrEmpty(request.Mail) || string.IsNullOrEmpty(request.Password))
    {
        return Results.BadRequest(new { message = "Email и пароль обязательны" });
    }

    if (request.Password.Length < 6)
    {
        return Results.BadRequest(new { message = "Пароль должен быть не менее 6 символов" });
    }

    var db = httpContext.RequestServices.GetRequiredService<DBManager>();

    var existingUser = db.FindUser(request.Mail);
    if (existingUser != null)
    {
        return Results.BadRequest(new { message = "Пользователь с таким email уже существует" });
    }

    bool success = db.TryAddUser(request.Mail, request.Password, request.Role);

    if (success)
    {
        var newUser = db.FindUser(request.Mail);
        return Results.Ok(new { id = newUser?.Id, mail = newUser?.Mail, role = newUser?.Role });
    }

    return Results.BadRequest(new { message = "Ошибка при создании пользователя" });
});

// API: обновить пользователя
app.MapPut("/api/users/{id}", async (int id, HttpContext httpContext) =>
{
    var request = await httpContext.Request.ReadFromJsonAsync<UpdateUserRequest>();
    if (request == null) return Results.BadRequest(new { message = "Неверный запрос" });

    var db = httpContext.RequestServices.GetRequiredService<DBManager>();
    var user = db.GetAllUsers().FirstOrDefault(u => u.Id == id);

    if (user == null)
    {
        return Results.NotFound(new { message = "Пользователь не найден" });
    }

    if (user.Role == "operator")
    {
        return Results.BadRequest(new { message = "Нельзя редактировать оператора" });
    }

    if (!string.IsNullOrEmpty(request.Password) && request.Password.Length < 6)
    {
        return Results.BadRequest(new { message = "Пароль должен быть не менее 6 символов" });
    }

    bool success = db.TryUpdateUser(id, request.Mail, request.Role, request.Password);

    if (success)
    {
        var updatedUser = db.GetAllUsers().FirstOrDefault(u => u.Id == id);
        return Results.Ok(new { id = updatedUser?.Id, mail = updatedUser?.Mail, role = updatedUser?.Role });
    }

    return Results.BadRequest(new { message = "Ошибка при обновлении пользователя" });
});

// API: удалить пользователя
app.MapDelete("/api/users/{id}", (int id, DBManager db) =>
{
    var user = db.GetAllUsers().FirstOrDefault(u => u.Id == id);

    if (user == null)
    {
        return Results.NotFound(new { message = "Пользователь не найден" });
    }

    if (user.Role == "operator")
    {
        return Results.BadRequest(new { message = "Нельзя удалить оператора" });
    }

    bool success = db.TryDeleteUserById(id);

    if (success)
    {
        return Results.Ok(new { message = "Пользователь удален" });
    }

    return Results.BadRequest(new { message = "Ошибка при удалении" });
});

app.Run();

public class CreateUserRequest
{
    public string Mail { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = "user";
}

public class UpdateUserRequest
{
    public string? Mail { get; set; }
    public string? Role { get; set; }
    public string? Password { get; set; }
}