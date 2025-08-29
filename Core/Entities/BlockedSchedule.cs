using Core.Enums;

namespace Core.Entities;

public class BlockedSchedule(
    DateOnly startDate,
    TimeOnly? startTime,
    TimeOnly? endTime,
    DateOnly? endDate = null)
    : Schedule(startTime ?? TimeOnly.MinValue,
        endTime ?? TimeOnly.MaxValue,
        startDate,
        endDate) {
    
    public bool IsWholeDayBlocked(DateOnly date) {
        if (StartDate > date || EndDate < date) return false;
        if (StartTime != TimeOnly.MinValue && EndTime != TimeOnly.MaxValue) return false;

        switch (RecurrenceType) {
            case RecurrenceType.Daily:
                if (RecurrenceInterval <= 1) return true;
                var days = date.DayNumber - StartDate.DayNumber;
                return (days % RecurrenceInterval) == 0;

            case RecurrenceType.Weekly:
                if (RecurrenceDays.Count == 0) return false;
                if (!RecurrenceDays.Contains(date.DayOfWeek)) return false;
                if (RecurrenceInterval <= 1) return true;
                var daysSinceStart = date.DayNumber - StartDate.DayNumber;
                var weeksSinceStart = daysSinceStart / 7;
                return (weeksSinceStart % RecurrenceInterval) == 0;

            default:
                throw new NotImplementedException($"Recurrence type {RecurrenceType} is not implemented.");
        }
    }
    
    public bool IsBlocked(DateOnly date, TimeOnly startTime, TimeOnly endTime) {
        if (startTime >= endTime)
            throw new ArgumentException("Start time must be earlier than end time.", nameof(startTime));
        if (StartDate > date || EndDate < date) return false;
        if (startTime >= EndTime || endTime <= StartTime) return false;

        switch (RecurrenceType) {
            case RecurrenceType.Daily:
                if (RecurrenceInterval <= 1) return true;
                var days = date.DayNumber - StartDate.DayNumber;
                return (days % RecurrenceInterval) == 0;

            case RecurrenceType.Weekly:
                if (RecurrenceDays.Count == 0) return false;
                if (!RecurrenceDays.Contains(date.DayOfWeek)) return false;
                if (RecurrenceInterval <= 1) return true;
                var daysSinceStart = date.DayNumber - StartDate.DayNumber;
                var weeksSinceStart = daysSinceStart / 7;
                return (weeksSinceStart % RecurrenceInterval) == 0;

            default:
                throw new NotImplementedException($"Recurrence type {RecurrenceType} is not implemented.");
        }
    }
}