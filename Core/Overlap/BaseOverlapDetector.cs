using Core.Entities;
using Core.Enums;
using Core.Sequences;

namespace Core.Overlap;

public abstract class BaseOverlapDetector : IOverlapDetector {
    protected static bool OverlapImpossible(Schedule s1, Schedule s2) {
        return s1.StartTime >= s2.EndTime 
               || s2.StartTime >= s1.EndTime 
               || s1.StartDate > s2.EndDate
               || s2.StartDate > s1.EndDate;
    }
    
    public bool IsOverlapping(Schedule schedule1, Schedule schedule2) =>
        Detect(schedule1, schedule2) != null;

    public ISequence? Detect(Schedule s1, Schedule s2) {
        ArgumentNullException.ThrowIfNull(s1);
        ArgumentNullException.ThrowIfNull(s2);
        
        var (s1F, s1E) = s1.Split();
        var (s2F, s2E) = s2.Split();
        
        var overlap = SplitDetect(s1F, s2F);
        if (overlap != null) return overlap;
        
        overlap = s2E != null ? SplitDetect(s1F, s2E) : null;
        if (overlap != null) return overlap;
        
        overlap = s1E != null ? SplitDetect(s1E, s2F) : null;
        if (overlap != null) return overlap;
        
        return s1E != null && s2E != null
            ? SplitDetect(s1E, s2E) : null;
    }
    protected abstract ISequence? SplitDetect(Schedule s1, Schedule s2);
}

public static class ScheduleExtensions {
    
    private static List<DayOfWeek> ShiftRight(IReadOnlySet<DayOfWeek> days) {
        var shifted = days.Select(d => (DayOfWeek)(((int)d + 1) % 7)).ToList();
        return shifted;
    }

    public static (Schedule f, Schedule? e) Split(this Schedule schedule) {
        if (!schedule.CrossesBoundary)
            return (schedule, null);

        var f = new Schedule(schedule.StartTime, TimeOnly.MaxValue, schedule.StartDate, schedule.EndDate);
        if (schedule.RecurrenceType == RecurrenceType.Weekly)
            f.RecurWeekly(schedule.RecurrenceDays.ToList(), schedule.RecurrenceInterval);
        
        else f.UpdateRecurrenceInterval(schedule.RecurrenceInterval);
        
        var e = new Schedule(TimeOnly.MinValue, schedule.EndTime, schedule.StartDate.AddDays(1), schedule.EndDate?.AddDays(1));
        if (schedule.RecurrenceType == RecurrenceType.Weekly)
            e.RecurWeekly(ShiftRight(schedule.RecurrenceDays), schedule.RecurrenceInterval);
        
        else e.UpdateRecurrenceInterval(schedule.RecurrenceInterval);
        
        return (f, e);
    }
}