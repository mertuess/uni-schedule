using UniSchedule.Json.Models;

namespace UniSchedule.API.Responses;

public class RoomWorkloadResponse : Response
{
    private readonly int room_id;
    private readonly Week week;

    public RoomWorkloadResponse(OutAPI o_api, int room_id, Week week) : base(o_api)
    {
        this.room_id = room_id;
        this.week = week;
    }

    public async Task<RoomWorkload?> GetRoomWorkload()
    {
        var result = new RoomWorkload();
        var schedule = await _o_api.SendRequest<TeacherSchedule>(
            $"rooms/{room_id}/schedule/{week.start}/{week.end}", "timetable");

        if (schedule == null || schedule.Count < 1) return null;

        result.room = schedule.First().room;
        result.workload = await convertToScheduleDictionary(schedule);
        int total = 0, count = 0;
        foreach (var p in result.workload)
        foreach (var s in p.Value.Values)
        {
            total++;
            if (s) count++;
        }

        result.workload_percent = (int)((double)count / total * 100);
        return result;
    }

    // Метод конвертации списка расписания в загруженность в виде словаря
    private async Task<Dictionary<string, Dictionary<int, bool>>> convertToScheduleDictionary(
        List<TeacherSchedule> teacherSchedules)
    {
        var result = new Dictionary<string, Dictionary<int, bool>>();

        var groupedByDate = teacherSchedules
            .Where(s => !string.IsNullOrEmpty(s.date))
            .GroupBy(s => s.date);

        foreach (var dateGroup in groupedByDate)
        {
            var slots = new Dictionary<int, bool>
            {
                { 1, false },
                { 2, false },
                { 3, false },
                { 4, false },
                { 5, false },
                { 6, false },
                { 7, false }
            };

            foreach (var schedule in dateGroup)
            {
                var slotIndex = Array.IndexOf(Const.ALL_SLOTS, schedule.slot) + 1;

                if (slotIndex > 0 && slotIndex <= slots.Values.Count)
                    slots[slotIndex] = true;
            }

            result[dateGroup.Key] = slots;
        }

        return result;
    }
}