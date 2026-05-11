// ┌────────────────────────────────────────────────────────────────────────────┐
// │ UniSchedule                                                                │
// │ WebAPI расширение для отслеживания расписания учебных занятий              │
// ├────────────────────────────────────────────────────────────────────────────┤
// │ Файл: User.cs                                                              │
// │ Описание: Модель пользователя                                              │
// └────────────────────────────────────────────────────────────────────────────┘
// Подключения

using SQLite;

/// <summary>
/// Пространство имен моделей базы данных
/// </summary>
namespace UniSchedule.DataBase.Models;

/// <summary>
///     Модель "Пользователь"
/// </summary>
public class User
{
    [PrimaryKey] [AutoIncrement] public int Id { get; set; }

    public string Mail { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string EngName { get; set; } = string.Empty;
    public string Role { get; set; } = "user";
    public int? DepartmentId { get; set; }
}