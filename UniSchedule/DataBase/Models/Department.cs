// ┌────────────────────────────────────────────────────────────────────────────┐
// │ UniSchedule                                                                │
// │ WebAPI расширение для отслеживания расписания учебных занятий              │
// ├────────────────────────────────────────────────────────────────────────────┤
// │ Файл: Department.cs                                                        │
// │ Описание: Модель кафедры                                                   │
// └────────────────────────────────────────────────────────────────────────────┘
// Подключения

using SQLite;

/// <summary>
/// Пространство имен моделей базы данных
/// </summary>
namespace UniSchedule.DataBase.Models;

/// <summary>
///     Модель "Кафедра"
/// </summary>
public class Department
{
    [PrimaryKey] [AutoIncrement] public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
}