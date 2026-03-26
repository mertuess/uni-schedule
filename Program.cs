using System.Globalization;
using Newtonsoft.Json;
using UniSchedule;
using UniSchedule.API;
using UniSchedule.Models;
using UniSchedule.Requests;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSingleton<DBManager>();
builder.Services.AddSingleton<OutAPI>();

var app = builder.Build();

var db = app.Services.GetRequiredService<DBManager>();
var o_api = app.Services.GetRequiredService<OutAPI>();

// Загрузка данных из внешнего API
await UniSchedule.DataManager.LoadAll(o_api);
await UniSchedule.DataManager.UpdateGroups(o_api,
    DataManager.Facultes.First(x => x.fac_id == 3),
    DataManager.Courses.First(x => x.course_id == 3));
if (DataManager.Groups.Any())
    await UniSchedule.DataManager.UpdateDates(o_api, DataManager.Groups.First());

if (!DataManager.Teachers.Any())
{
    DataManager.Teachers.Add(new Teacher { teacher_id = 1, teacher = "Иванов Иван Иванович", UID = "test_uid_1" });
    DataManager.Teachers.Add(new Teacher { teacher_id = 2, teacher = "Петрова Мария Сергеевна", UID = "test_uid_2" });
}

// Статические данные для демонстрации (размещаем до app.Run)
var buildings = new[]
{
    new { id = 1, name = "Главный корпус" },
    new { id = 2, name = "Учебный корпус №2" },
    new { id = 3, name = "Лабораторный корпус" }
};

var classrooms = new Dictionary<int, List<object>>
{
    [1] = new List<object> { new { id = 101, name = "101" }, new { id = 102, name = "102" }, new { id = 103, name = "103" }, new { id = 104, name = "104" } },
    [2] = new List<object> { new { id = 201, name = "201" }, new { id = 202, name = "202" }, new { id = 203, name = "203" } },
    [3] = new List<object> { new { id = 301, name = "301" }, new { id = 302, name = "302" } }
};

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Главная страница
app.MapGet("/", async context =>
{
    string data = "";
    using (StreamReader reader = new StreamReader(@"./wwwroot/index.html"))
    {
        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (line.Contains("UI_INST"))
            {
                foreach (var f in DataManager.Facultes)
                    data += $"<option value=\"{f.fac_id}\">{f.facultee}</option>";
            }
            else if (line.Contains("UI_COURSE"))
            {
                foreach (var c in DataManager.Courses)
                    data += $"<option value=\"{c.course_id}\">{c.course}</option>";
            }
            else if (line.Contains("UI_GROUP"))
            {
                foreach (var g in DataManager.Groups)
                    data += $"<option value=\"{g.group_id}\">{g.group}</option>";
            }
            else if (line.Contains("UI_WEEK"))
            {
                foreach (var w in DataManager.CurrentDates)
                    data += $"<option value=\"{w}\">{w}</option>";
            }
            else
            {
                data += line;
            }
        }
    }
    context.Response.ContentType = "text/html";
    await context.Response.WriteAsync(data);
});

// API для работы с пользователями
app.MapPost("/api/login", async (HttpContext httpContext) =>
{
    var request = await httpContext.Request.ReadFromJsonAsync<LoginRequest>();
    if (request == null) return Results.BadRequest();

    var db = httpContext.RequestServices.GetRequiredService<DBManager>();
    var user = db.TryLogin(request.Mail, request.Password);
    if (user != null)
        return Results.Ok(new { user.Mail, user.Role });
    return Results.Unauthorized();
});

app.MapGet("/api/users", (DBManager db) => Results.Ok(db.GetAllUsers()));

app.MapGet("/api/users/{id}", (int id, DBManager db) =>
{
    var user = db.GetAllUsers().FirstOrDefault(u => u.Id == id);
    return user == null ? Results.NotFound() : Results.Ok(user);
});

app.MapPost("/api/users", async (HttpContext httpContext) =>
{
    var request = await httpContext.Request.ReadFromJsonAsync<CreateUserRequest>();
    if (request == null) return Results.BadRequest();

    if (string.IsNullOrEmpty(request.Mail) || string.IsNullOrEmpty(request.Password))
        return Results.BadRequest(new { message = "Email и пароль обязательны" });
    if (request.Password.Length < 6)
        return Results.BadRequest(new { message = "Пароль должен быть не менее 6 символов" });

    var db = httpContext.RequestServices.GetRequiredService<DBManager>();
    if (db.FindUser(request.Mail) != null)
        return Results.BadRequest(new { message = "Пользователь с таким email уже существует" });

    bool success = db.TryAddUser(request.Mail, request.Password, request.Role);
    if (success)
    {
        var newUser = db.FindUser(request.Mail);
        return Results.Ok(new { id = newUser?.Id, mail = newUser?.Mail, role = newUser?.Role });
    }
    return Results.BadRequest(new { message = "Ошибка при создании пользователя" });
});

app.MapPut("/api/users/{id}", async (int id, HttpContext httpContext) =>
{
    var request = await httpContext.Request.ReadFromJsonAsync<UpdateUserRequest>();
    if (request == null) return Results.BadRequest();

    var db = httpContext.RequestServices.GetRequiredService<DBManager>();
    var user = db.GetAllUsers().FirstOrDefault(u => u.Id == id);
    if (user == null) return Results.NotFound();

    if (user.Role == "operator")
        return Results.BadRequest(new { message = "Нельзя редактировать оператора" });

    if (!string.IsNullOrEmpty(request.Password) && request.Password.Length < 6)
        return Results.BadRequest(new { message = "Пароль должен быть не менее 6 символов" });

    bool success = db.TryUpdateUser(id, request.Mail, request.Role, request.Password);
    if (success)
    {
        var updatedUser = db.GetAllUsers().FirstOrDefault(u => u.Id == id);
        return Results.Ok(new { id = updatedUser?.Id, mail = updatedUser?.Mail, role = updatedUser?.Role });
    }
    return Results.BadRequest(new { message = "Ошибка при обновлении пользователя" });
});

app.MapDelete("/api/users/{id}", (int id, DBManager db) =>
{
    var user = db.GetAllUsers().FirstOrDefault(u => u.Id == id);
    if (user == null) return Results.NotFound();
    if (user.Role == "operator") return Results.BadRequest(new { message = "Нельзя удалить оператора" });

    return db.TryDeleteUserById(id)
        ? Results.Ok(new { message = "Пользователь удален" })
        : Results.BadRequest(new { message = "Ошибка при удалении" });
});

// API для преподавателей
app.MapGet("/api/teachers", () => Results.Ok(DataManager.Teachers));

app.MapGet("/api/teacher/{teacherId:int}/schedule", async (int teacherId, string week, OutAPI api) =>
{
    var teacher = DataManager.Teachers.FirstOrDefault(t => t.teacher_id == teacherId);
    if (teacher == null) return Results.NotFound();

    if (!TryParseIsoWeek(week, out var startDate, out var endDate))
        return Results.BadRequest("Неверный формат недели");

    string scheduleJson = await api.GetTeacherScheduleRange(teacher.UID,
        startDate.ToString("yyyy-MM-dd"),
        endDate.ToString("yyyy-MM-dd"));

    var lessons = JsonConvert.DeserializeObject<List<Lesson>>(scheduleJson) ?? new List<Lesson>();

    var result = new Dictionary<string, object>();
    foreach (var lesson in lessons)
    {
        string dayName = lesson.Date.DayOfWeek switch
        {
            DayOfWeek.Monday => "Понедельник",
            DayOfWeek.Tuesday => "Вторник",
            DayOfWeek.Wednesday => "Среда",
            DayOfWeek.Thursday => "Четверг",
            DayOfWeek.Friday => "Пятница",
            DayOfWeek.Saturday => "Суббота",
            _ => null
        };
        if (dayName == null) continue;

        string key = $"{dayName}_{lesson.Pair}";
        result[key] = new
        {
            discipline = lesson.Discipline,
            group = lesson.GroupName,
            classroom = lesson.Classroom
        };
    }

    return Results.Ok(result);
});

// ========== API ДЛЯ АУДИТОРИЙ (ДОБАВЛЕНЫ ДО app.Run) ==========
app.MapGet("/api/weeks", () => Results.Ok(DataManager.CurrentDates));

app.MapGet("/api/buildings", () => Results.Ok(buildings));

app.MapGet("/api/buildings/{buildingId:int}/classrooms", (int buildingId) =>
{
    if (classrooms.TryGetValue(buildingId, out var list))
        return Results.Ok(list);
    return Results.NotFound();
});

app.MapGet("/api/classroom/{classroomId:int}/schedule", (int classroomId, string week) =>
{
    // Заглушка с псевдоданными
    var result = new Dictionary<string, object>();
    var days = new[] { "Понедельник", "Вторник", "Среда", "Четверг", "Пятница", "Суббота" };
    var pairs = Enumerable.Range(1, 6);
    var random = new Random();

    foreach (var day in days)
    {
        foreach (var pair in pairs)
        {
            if (random.Next(0, 3) == 0) // 1 из 3 шанс, что занято
            {
                result[$"{day}_{pair}"] = new
                {
                    discipline = $"Дисциплина {random.Next(1, 10)}",
                    group = $"Группа {random.Next(100, 999)}",
                    teacher = $"Преподаватель {random.Next(1, 10)}"
                };
            }
        }
    }
    return Results.Ok(result);
});

// ========== КОНЕЦ API ДЛЯ АУДИТОРИЙ ==========

app.Run();

// Вспомогательные методы и классы
static bool TryParseIsoWeek(string week, out DateTime startDate, out DateTime endDate)
{
    startDate = endDate = default;
    if (string.IsNullOrWhiteSpace(week)) return false;
    var parts = week.Split('-');
    if (parts.Length != 2 || !parts[1].StartsWith("W")) return false;
    if (!int.TryParse(parts[0], out int year)) return false;
    if (!int.TryParse(parts[1].AsSpan(1), out int weekNum)) return false;

    // ISO 8601: первая неделя года содержит четверг
    DateTime jan1 = new DateTime(year, 1, 1);
    int daysOffset = (int)jan1.DayOfWeek - (int)DayOfWeek.Monday;
    if (daysOffset < 0) daysOffset += 7;
    DateTime firstMonday = jan1.AddDays(-daysOffset);
    if (firstMonday.Year != year)
        firstMonday = firstMonday.AddDays(-7);

    startDate = firstMonday.AddDays((weekNum - 1) * 7);
    endDate = startDate.AddDays(6);
    return true;
}

public class Lesson
{
    public DateTime Date { get; set; }
    public int Pair { get; set; }
    public string Discipline { get; set; } = "";
    public string Classroom { get; set; } = "";
    public string GroupName { get; set; } = "";
}

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