using System.Globalization;

namespace UniSchedule.Json.Models;

public class Week
{
    public Week()
    {
    }

    public Week(string start, string end)
    {
        this.start = start;
        this.end = end;
    }

    public Week(List<DateTime> dates)
    {
        start = dates.First().ToString("yyyy-MM-dd");
        end = dates.Last().ToString("yyyy-MM-dd");
        this.dates = dates.Select(d => d.ToString("yyyy-MM-dd")).ToList();
    }

    public string start { get; set; } = string.Empty;
    public string end { get; set; } = string.Empty;
    public List<string> dates { get; set; } = new();

    public static DateTime GetMondayOfWeek(DateTime date)
    {
        return date.AddDays(-(((int)date.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7)).Date;
    }

    public static List<Week> GenerateWeeksByDates(List<string> dateStrings)
    {
        var weeks = new List<Week>();
        var dates = dateStrings
            .Select(d => DateTime.ParseExact(d, "yyyy-MM-dd", CultureInfo.InvariantCulture))
            .OrderBy(d => d)
            .ToList();

        if (!dates.Any()) return weeks;

        var currentWeekDates = new List<DateTime>();
        var currentWeekStart = GetMondayOfWeek(dates[0]);

        foreach (var date in dates)
        {
            var weekStart = GetMondayOfWeek(date);
            if (weekStart != currentWeekStart)
            {
                if (currentWeekDates.Any()) weeks.Add(new Week(currentWeekDates));
                currentWeekStart = weekStart;
                currentWeekDates = new List<DateTime> { date };
            }
            else
            {
                currentWeekDates.Add(date);
            }
        }

        if (currentWeekDates.Any())
            weeks.Add(new Week(currentWeekDates));

        return weeks;
    }
}