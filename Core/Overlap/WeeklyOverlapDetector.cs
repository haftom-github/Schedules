using Core.Entities;
using Core.Enums;
using Core.Options;
using Core.Sequences;

namespace Core.Overlap;

public class WeeklyOverlapDetector : BaseOverlapDetector {
    protected override ISequence? DetectSplit(Schedule s1, Schedule s2) {
        if (s1.RecurrenceType != RecurrenceType.Weekly
            || s2.RecurrenceType != RecurrenceType.Weekly)
            throw new ArgumentException("can only detect overlaps between two weekly recurrent schedules");

        if (OverlapIsImpossible(s1, s2)) return null;

        var commonDaysOfWeek = s1.DaysOfWeek.Intersect(s2.DaysOfWeek).ToHashSet();
        if (commonDaysOfWeek.Count == 0)
            return null;
        
        var s1Start = s1.StartDate.ToFirstDayOfWeek();
        var s2Start = s2.StartDate.ToFirstDayOfWeek();

        foreach (var day in commonDaysOfWeek) {
            while (s1Start.DayOfWeek != day)
                (s1Start, s2Start) = (s1Start.AddDays(1), s2Start.AddDays(1));
        
            var s1Sequence = SequenceFactory.Create(s1Start.DayNumber, s1.EndDate?.DayNumber, s1.RecurrenceInterval * 7);
            var s2Sequence = SequenceFactory.Create(s2Start.DayNumber, s2.EndDate?.DayNumber, s2.RecurrenceInterval * 7);
            var overlap = s1Sequence.FindOverlapWith(s2Sequence);
            
            overlap =  overlap?.Start < s1.StartDate.DayNumber 
                   || overlap?.Start < s2.StartDate.DayNumber ? overlap.StartFromIndex(1) : overlap;
        
            if (overlap != null) return overlap;
            s1Start = s1.StartDate.ToFirstDayOfWeek();
            s2Start = s2.StartDate.ToFirstDayOfWeek();
        }
        
        return null;
    }
}