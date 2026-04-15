using UniSchedule.Json.Models;

namespace UniSchedule.API.Responses;

public class TeachersScheduleResponse : Response
{
    private readonly string[] UIDs;
    private readonly Week week;

    public TeachersScheduleResponse(OutAPI o_api, string[] uids, Week week) : base(o_api)
    {
        UIDs = uids;
        this.week = week;
    }

    public async Task<Dictionary<string, List<string>>> GetFreeSlots()
    {
        var schedules = new List<List<TeacherSchedule>>();
        foreach (var uid in UIDs)
        {
            var s = await _o_api.SendRequest<TeacherSchedule>(
                $"teachers/{uid}/schedule/{week.start}/{week.end}", "timetable");
            schedules.Add(s);
        }

        var common_free_slots = new Dictionary<string, List<string>>();

        var allDates = schedules
            .SelectMany(teacherSchedule => teacherSchedule.Select(item => item.date))
            .Distinct()
            .OrderBy(d => d)
            .ToList();

        foreach (var date in allDates)
        {
            var allOccupiedSlots = new HashSet<string>();

            foreach (var teacherSchedule in schedules)
            {
                var occupiedSlotsOnDate = teacherSchedule
                    .Where(item => item.date == date)
                    .Select(item => item.slot);

                foreach (var slot in occupiedSlotsOnDate) allOccupiedSlots.Add(slot);
            }

            var commonFreeSlots = Const.ALL_SLOTS
                .Where(slot => !allOccupiedSlots.Contains(slot))
                .ToList();

            if (commonFreeSlots.Any()) common_free_slots[date] = commonFreeSlots;
        }

        return common_free_slots;
    }
}