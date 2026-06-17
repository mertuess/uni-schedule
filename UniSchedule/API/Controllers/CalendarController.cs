// ┌────────────────────────────────────────────────────────────────────────────┐
// │ UniSchedule                                                                │
// │ WebAPI расширение для отслеживания расписания учебных занятий              │
// ├────────────────────────────────────────────────────────────────────────────┤
// │ Файл: CalendarController.cs                                                │
// │ Описание: Контроллер для работы с iCal ссылками                            │
// └────────────────────────────────────────────────────────────────────────────┘

using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniSchedule.Json.Models;
using UniSchedule.Services;
using UniSchedule.System;

namespace UniSchedule.API.Controllers;

[ApiController]
[Route("api/calendar")]
[AllowAnonymous]
public class CalendarController : ControllerBase
{
    private readonly ICalendarService _calendarService;
    private readonly Debug _dbg;
    private readonly OutAPI _outApi;

    public CalendarController(OutAPI outApi, ICalendarService calendarService, Debug dbg)
    {
        _outApi = outApi;
        _calendarService = calendarService;
        _dbg = dbg;
    }

    /// <summary>
    ///     Тестовый эндпоинт для проверки работы iCal (гарантированно рабочий)
    /// </summary>
    [HttpGet("test/download")]
    public IActionResult DownloadTestIcal()
    {
        // Создаем правильный iCal файл вручную
        var sb = new StringBuilder();

        // Заголовок календаря
        sb.AppendLine("BEGIN:VCALENDAR");
        sb.AppendLine("VERSION:2.0");
        sb.AppendLine("PRODID:-//UniSchedule//Schedule//EN");
        sb.AppendLine("CALSCALE:GREGORIAN");
        sb.AppendLine("METHOD:PUBLISH");
        sb.AppendLine("X-WR-CALNAME:Тестовое расписание");
        sb.AppendLine("X-WR-CALDESC:Тестовое расписание для проверки календаря");

        // Сегодня и завтра
        var today = DateTime.Now;
        var tomorrow = DateTime.Now.AddDays(1);
        var nowUtc = DateTime.UtcNow;

        // Событие 1 - сегодня
        sb.AppendLine("BEGIN:VEVENT");
        sb.AppendLine("UID:event1-" + Guid.NewGuid() + "@unischedule.ru");
        sb.AppendLine("DTSTAMP:" + nowUtc.ToString("yyyyMMddTHHmmssZ"));
        sb.AppendLine("DTSTART:" + today.ToString("yyyyMMdd") + "T090000");
        sb.AppendLine("DTEND:" + today.ToString("yyyyMMdd") + "T103000");
        sb.AppendLine("SUMMARY:Тестовое занятие 1 (Лекция)");
        sb.AppendLine("DESCRIPTION:Преподаватель: Иванов И.И.\\nТип: Лекция\\nКабинет: 101");
        sb.AppendLine("LOCATION:Аудитория 101");
        sb.AppendLine("STATUS:CONFIRMED");
        sb.AppendLine("SEQUENCE:0");
        sb.AppendLine("END:VEVENT");

        // Событие 2 - завтра
        sb.AppendLine("BEGIN:VEVENT");
        sb.AppendLine("UID:event2-" + Guid.NewGuid() + "@unischedule.ru");
        sb.AppendLine("DTSTAMP:" + nowUtc.ToString("yyyyMMddTHHmmssZ"));
        sb.AppendLine("DTSTART:" + tomorrow.ToString("yyyyMMdd") + "T104000");
        sb.AppendLine("DTEND:" + tomorrow.ToString("yyyyMMdd") + "T121000");
        sb.AppendLine("SUMMARY:Тестовое занятие 2 (Практика)");
        sb.AppendLine("DESCRIPTION:Преподаватель: Петров П.П.\\nТип: Практическое занятие\\nКабинет: 202");
        sb.AppendLine("LOCATION:Аудитория 202");
        sb.AppendLine("STATUS:CONFIRMED");
        sb.AppendLine("SEQUENCE:0");
        sb.AppendLine("END:VEVENT");

        // Событие 3 - послезавтра
        var afterTomorrow = DateTime.Now.AddDays(2);
        sb.AppendLine("BEGIN:VEVENT");
        sb.AppendLine("UID:event3-" + Guid.NewGuid() + "@unischedule.ru");
        sb.AppendLine("DTSTAMP:" + nowUtc.ToString("yyyyMMddTHHmmssZ"));
        sb.AppendLine("DTSTART:" + afterTomorrow.ToString("yyyyMMdd") + "T140000");
        sb.AppendLine("DTEND:" + afterTomorrow.ToString("yyyyMMdd") + "T153000");
        sb.AppendLine("SUMMARY:Тестовое занятие 3 (Лабораторная)");
        sb.AppendLine("DESCRIPTION:Преподаватель: Сидоров С.С.\\nТип: Лабораторная работа\\nКабинет: 303");
        sb.AppendLine("LOCATION:Аудитория 303");
        sb.AppendLine("STATUS:CONFIRMED");
        sb.AppendLine("SEQUENCE:0");
        sb.AppendLine("END:VEVENT");

        sb.AppendLine("END:VCALENDAR");

        var icalBytes = Encoding.UTF8.GetBytes(sb.ToString());

        // Отключаем кэширование
        Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
        Response.Headers.Add("Pragma", "no-cache");
        Response.Headers.Add("Expires", "0");

        return File(icalBytes, "text/calendar; charset=utf-8", "test_schedule.ics");
    }

    /// <summary>
    ///     Простой тестовый эндпоинт (минимальный iCal файл)
    /// </summary>
    [HttpGet("test/simple")]
    public IActionResult DownloadSimpleIcal()
    {
        var now = DateTime.UtcNow;
        var startTime = DateTime.Now;
        var endTime = DateTime.Now.AddHours(1);

        var content = $"BEGIN:VCALENDAR\r\n" +
                      $"VERSION:2.0\r\n" +
                      $"PRODID:-//Test//Simple//EN\r\n" +
                      $"BEGIN:VEVENT\r\n" +
                      $"UID:simple-{Guid.NewGuid()}@test.ru\r\n" +
                      $"DTSTAMP:{now:yyyyMMddTHHmmssZ}\r\n" +
                      $"DTSTART:{startTime:yyyyMMddTHHmmss}\r\n" +
                      $"DTEND:{endTime:yyyyMMddTHHmmss}\r\n" +
                      $"SUMMARY:Простое тестовое занятие\r\n" +
                      $"LOCATION:Аудитория 101\r\n" +
                      $"END:VEVENT\r\n" +
                      $"END:VCALENDAR\r\n";

        var bytes = Encoding.UTF8.GetBytes(content);

        Response.Headers.Add("Cache-Control", "no-cache");
        return File(bytes, "text/calendar", "simple_test.ics");
    }

    /// <summary>
    ///     Экспорт расписания группы в iCal (с обработкой ошибок)
    /// </summary>
    [HttpGet("group/{groupName}")]
    public async Task<IActionResult> ExportGroupToIcal(string groupName)
    {
        try
        {
            _dbg.Log($"Экспорт iCal для группы: {groupName}");

            List<Schedule> schedules;
            try
            {
                schedules = await _outApi.SendRequest<Schedule>("schedule", "schedule");
            }
            catch (Exception ex)
            {
                _dbg.Error($"Ошибка подключения к внешнему API: {ex.Message}");
                return StatusCode(503, new
                {
                    error = "Сервер расписания временно недоступен",
                    details = "Пожалуйста, попробуйте позже",
                    testUrl = "/api/calendar/test/download"
                });
            }

            if (schedules == null || !schedules.Any())
                return NotFound(new { error = "Расписание не загружено" });

            var groupSchedules = schedules.Where(s =>
                s.subGroups != null && s.subGroups.Any(g => g.Contains(groupName))
            ).ToList();

            if (!groupSchedules.Any())
            {
                var availableGroups = schedules
                    .Where(s => s.subGroups != null)
                    .SelectMany(s => s.subGroups)
                    .Distinct()
                    .Take(10)
                    .ToList();

                return NotFound(new
                {
                    error = $"Расписание для группы {groupName} не найдено", availableGroups
                });
            }

            _dbg.Log($"Найдено {groupSchedules.Count} занятий для группы {groupName}");

            // Генерируем iCal через сервис
            var icalBytes = _calendarService.GenerateScheduleIcal(groupSchedules, $"Группа {groupName}");

            Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
            Response.Headers.Add("Pragma", "no-cache");
            Response.Headers.Add("Expires", "0");

            return File(icalBytes, "text/calendar; charset=utf-8", $"group_{groupName}_schedule.ics");
        }
        catch (Exception ex)
        {
            _dbg.Error($"Ошибка экспорта iCal: {ex.Message}");
            return StatusCode(500, new { error = "Внутренняя ошибка сервера", details = ex.Message });
        }
    }

    /// <summary>
    ///     Получить список групп
    /// </summary>
    [HttpGet("groups")]
    public async Task<IActionResult> GetAvailableGroups()
    {
        try
        {
            try
            {
                var schedules = await _outApi.SendRequest<Schedule>("schedule", "schedule");
                var groups = schedules
                    .Where(s => s.subGroups != null)
                    .SelectMany(s => s.subGroups)
                    .Distinct()
                    .OrderBy(g => g)
                    .ToList();

                if (groups.Any())
                    return Ok(groups);
            }
            catch (Exception ex)
            {
                _dbg.Warning($"Не удалось получить группы из API: {ex.Message}");
            }

            var testGroups = new[] { "ТЕСТ-01", "ТЕСТ-02", "ПИ-101", "ПИ-102", "ИВТ-201" };
            return Ok(testGroups);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    ///     Экспорт расписания преподавателя в iCal
    /// </summary>
    [HttpGet("teacher/{teacherName}")]
    public async Task<IActionResult> ExportTeacherToIcal(string teacherName)
    {
        try
        {
            _dbg.Log($"Экспорт iCal для преподавателя: {teacherName}");

            List<TeacherSchedule> teacherSchedules;
            try
            {
                teacherSchedules = await _outApi.SendRequest<TeacherSchedule>("t_schedule", "schedule");
            }
            catch (Exception ex)
            {
                _dbg.Error($"Ошибка подключения к внешнему API: {ex.Message}");
                return StatusCode(503, new
                {
                    error = "Сервер расписания временно недоступен",
                    testUrl = "/api/calendar/test/download"
                });
            }

            if (teacherSchedules == null || !teacherSchedules.Any())
                return NotFound(new { error = "Расписание преподавателей не загружено" });

            var filteredSchedules = teacherSchedules
                .Where(s => s.teacher != null && s.teacher.Equals(teacherName, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (!filteredSchedules.Any())
                return NotFound(new { error = $"Расписание для преподавателя {teacherName} не найдено" });

            _dbg.Log($"Найдено {filteredSchedules.Count} занятий для преподавателя {teacherName}");

            var icalBytes = _calendarService.GenerateTeacherIcal(filteredSchedules, teacherName);

            Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");

            return File(icalBytes, "text/calendar; charset=utf-8", $"teacher_{teacherName}_schedule.ics");
        }
        catch (Exception ex)
        {
            _dbg.Error($"Ошибка экспорта iCal: {ex.Message}");
            return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    ///     Получить ссылку для подписки
    /// </summary>
    [HttpGet("subscribe/group/{groupName}")]
    public IActionResult GetSubscriptionUrl(string groupName)
    {
        try
        {
            var token = _calendarService.GenerateSubscriptionToken("group", groupName);
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var httpsUrl = $"{baseUrl}/api/calendar/group/{Uri.EscapeDataString(groupName)}?token={token}";
            var webcalUrl = httpsUrl.Replace("https://", "webcal://").Replace("http://", "webcal://");

            return Ok(new
            {
                url = httpsUrl,
                webcalUrl,
                token
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}