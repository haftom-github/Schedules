using Core.Entities;
using Core.Sequences;

namespace Core.Overlap;

public abstract class BaseOverlapDetector : IOverlapDetector {
    protected static bool OverlapIsImpossible(Schedule s1, Schedule s2) {
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
        
        var s1Splits = s1.SplitOnDayBoundary();
        var s2Splits = s2.SplitOnDayBoundary();

        foreach (var s1Split in s1Splits) {
            foreach (var s2Split in s2Splits) {
                var overlap = DetectSplit(s1Split, s2Split);
                if (overlap != null) return overlap;
            }
        }

        return null;
        
        // var overlap = DetectSplit(s1F, s2F);
        // if (overlap != null) return overlap;
        //
        // overlap = s2E != null ? DetectSplit(s1F, s2E) : null;
        // if (overlap != null) return overlap;
        //
        // overlap = s1E != null ? DetectSplit(s1E, s2F) : null;
        // if (overlap != null) return overlap;
        //
        // return s1E != null && s2E != null
        //     ? DetectSplit(s1E, s2E) : null;
    }
    protected abstract ISequence? DetectSplit(Schedule s1, Schedule s2);
}

public static class ScheduleExtensions {
    
    private static DayOfWeek ToNextDayOfWeek(this DayOfWeek day) {
        return (DayOfWeek)(((int)day + 1) % 7);
    }

    // public static (Schedule before, Schedule? after) SplitOnDayBoundary(this Schedule schedule) {
    //     if (!schedule.CrossesDayBoundary)
    //         return (schedule, null);
    //
    //     var before = new Schedule(schedule.StartDate, schedule.EndDate, schedule.StartTime, schedule.EndTime);
    //     before.EndAtMidNight();
    //     
    //     var after = new Schedule(schedule);
    //     after.UpdateStartTime(TimeOnly.MinValue);
    //     after.UpdateStartDate(schedule.StartDate.AddDays(1));
    //     after.UpdateEndDate(schedule.EndDate?.AddDays(1));
    //     
    //     var shiftedDaysOfWeek = schedule.DaysOfWeek.Select(d => d.ToNextDayOfWeek()).ToList();
    //     after.UpdateDaysOfWeek(shiftedDaysOfWeek);
    //     
    //     return (before, after);
    // }
}