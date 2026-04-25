using System.Text;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using UniSchedule.Json.Models;
using UniSchedule.System;

namespace UniSchedule.Services
{
    public interface ICalendarService
    {
        byte[] GenerateScheduleIcal(List<Schedule> schedules, string title);
        byte[] GenerateTeacherIcal(List<TeacherSchedule> schedules, string teacherName);
        string GenerateSubscriptionToken(string entityType, string entityId);
        bool ValidateSubscriptionToken(string token, string expectedEntityType, string expectedEntityId);
    }

    public class CalendarService : ICalendarService
    {
        public byte[] GenerateScheduleIcal(List<Schedule> schedules, string title)
        {
            var calendar = new Calendar();
            calendar.ProductId = "-//UniSchedule//Schedule//RU";
            calendar.Method = "PUBLISH";
            calendar.Name = title;
            
            foreach (var schedule in schedules)
            {
                if (!DateTime.TryParse(schedule.pair_date, out var date))
                    continue;
                    
                var (startTime, endTime) = ParsePairTime(schedule.pair_time);
                
                var startDateTime = date.Date + startTime;
                var endDateTime = date.Date + endTime;
                
                var calendarEvent = new CalendarEvent
                {
                    Uid = $"{schedule.id}@unischedule.ru",
                    Summary = schedule.disciplines ?? "Занятие",
                    Description = BuildScheduleDescription(schedule),
                    Location = schedule.room ?? "",
                    DtStart = new CalDateTime(startDateTime),
                    DtEnd = new CalDateTime(endDateTime),
                    DtStamp = new CalDateTime(DateTime.UtcNow),
                    Created = new CalDateTime(DateTime.UtcNow),
                    LastModified = new CalDateTime(DateTime.UtcNow)
                };
                
                calendar.Events.Add(calendarEvent);
            }
            
            var serializer = new CalendarSerializer();
            var icalString = serializer.SerializeToString(calendar);
            return Encoding.UTF8.GetBytes(icalString ?? "");
        }
        
        public byte[] GenerateTeacherIcal(List<TeacherSchedule> schedules, string teacherName)
        {
            var calendar = new Calendar();
            calendar.ProductId = "-//UniSchedule//Teacher//RU";
            calendar.Name = $"Расписание преподавателя - {teacherName}";
            
            foreach (var schedule in schedules)
            {
                if (!DateTime.TryParse(schedule.date, out var date))
                    continue;
                    
                var (startTime, endTime) = ParseSlotTime(schedule.slot);
                
                var calendarEvent = new CalendarEvent
                {
                    Uid = $"{schedule.index}@unischedule.ru",
                    Summary = schedule.disciplines ?? "Занятие",
                    Description = BuildTeacherDescription(schedule),
                    Location = schedule.room ?? "",
                    DtStart = new CalDateTime(date.Date + startTime),
                    DtEnd = new CalDateTime(date.Date + endTime),
                    DtStamp = new CalDateTime(DateTime.UtcNow)
                };
                
                calendar.Events.Add(calendarEvent);
            }
            
            var serializer = new CalendarSerializer();
            var icalString = serializer.SerializeToString(calendar);
            return Encoding.UTF8.GetBytes(icalString ?? "");
        }
        
        private (TimeSpan start, TimeSpan end) ParsePairTime(string pairTime)
        {
            if (string.IsNullOrEmpty(pairTime))
                return (TimeSpan.Zero, TimeSpan.FromHours(1.5));
                
            var parts = pairTime.Split('-');
            if (parts.Length != 2) 
                return (TimeSpan.Zero, TimeSpan.FromHours(1.5));
            
            if (TimeSpan.TryParse(parts[0], out var start) && TimeSpan.TryParse(parts[1], out var end))
                return (start, end);
                
            return (TimeSpan.Zero, TimeSpan.FromHours(1.5));
        }
        
        private (TimeSpan start, TimeSpan end) ParseSlotTime(string slot)
        {
            var slotMap = new Dictionary<string, (TimeSpan, TimeSpan)>
            {
                ["1"] = (TimeSpan.FromHours(9), TimeSpan.FromHours(10).Add(TimeSpan.FromMinutes(30))),
                ["2"] = (TimeSpan.FromHours(10).Add(TimeSpan.FromMinutes(40)), TimeSpan.FromHours(12).Add(TimeSpan.FromMinutes(10))),
                ["3"] = (TimeSpan.FromHours(12).Add(TimeSpan.FromMinutes(20)), TimeSpan.FromHours(13).Add(TimeSpan.FromMinutes(50))),
                ["4"] = (TimeSpan.FromHours(14), TimeSpan.FromHours(15).Add(TimeSpan.FromMinutes(30))),
                ["5"] = (TimeSpan.FromHours(15).Add(TimeSpan.FromMinutes(40)), TimeSpan.FromHours(17).Add(TimeSpan.FromMinutes(10))),
                ["6"] = (TimeSpan.FromHours(17).Add(TimeSpan.FromMinutes(20)), TimeSpan.FromHours(18).Add(TimeSpan.FromMinutes(50)))
            };
            
            if (!string.IsNullOrEmpty(slot) && slotMap.TryGetValue(slot, out var times))
                return times;
                
            return (TimeSpan.Zero, TimeSpan.FromHours(1.5));
        }
        
        private string BuildScheduleDescription(Schedule schedule)
        {
            var parts = new List<string>();
            parts.Add($"Тип пары: {schedule.pair_type ?? "не указан"}");
            parts.Add($"Преподаватель: {schedule.teacher ?? "не указан"}");
            parts.Add($"День недели: {schedule.day_of_week ?? "не указан"}");
            
            if (schedule.isSubGroup && schedule.subGroups != null && schedule.subGroups.Any())
                parts.Add($"Подгруппы: {string.Join(", ", schedule.subGroups)}");
                
            return string.Join("\n", parts);
        }
        
        private string BuildTeacherDescription(TeacherSchedule schedule)
        {
            var parts = new List<string>();
            parts.Add($"Тип: {schedule.type ?? "не указан"}");
            parts.Add($"День недели: {schedule.day_of_week ?? "не указан"}");
            
            if (schedule.subGroups != null && schedule.subGroups.Any())
                parts.Add($"Группы: {string.Join(", ", schedule.subGroups)}");
                
            return string.Join("\n", parts);
        }
        
        public string GenerateSubscriptionToken(string entityType, string entityId)
        {
            var data = $"{entityType}:{entityId}:{DateTime.UtcNow.Ticks}";
            var encryptedBytes = Crypto.Encrypt(data);
            return Convert.ToBase64String(encryptedBytes);
        }
        
        public bool ValidateSubscriptionToken(string token, string expectedEntityType, string expectedEntityId)
        {
            try
            {
                var encryptedBytes = Convert.FromBase64String(token);
                var decrypted = Crypto.Decrypt(encryptedBytes);
                var parts = decrypted.Split(':');
                return parts.Length >= 2 && parts[0] == expectedEntityType && parts[1] == expectedEntityId;
            }
            catch
            {
                return false;
            }
        }
    }
}