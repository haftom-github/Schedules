using Core.Entities;
using Core.Enums;
using Core.Options;
using Core.Sequences;

namespace Core.Overlap;

public class WeeklyVsDailyOverlapDetector : BaseOverlapDetector {
    protected override ISequence? DetectSplit(Schedule s1, Schedule s2) {
        if (s1.RecurrenceType != RecurrenceType.Weekly)
            (s1, s2) = (s2, s1);
        
        if (s1.RecurrenceType != RecurrenceType.Weekly || s2.RecurrenceType != RecurrenceType.Daily)
            throw new ArgumentException("one of the schedules must be weekly and the other daily");

        if (OverlapIsImpossible(s1, s2)) return null;

        var s2Sequence =
            SequenceFactory.Create(s2.StartDate.DayNumber, s2.EndDate?.DayNumber, s2.RecurrenceInterval);
        
        foreach (var day in s1.DaysOfWeek) {
            var s1Start = s1.StartDate.ToFirstDayOfWeek();
            while (s1Start.DayOfWeek != day) s1Start = s1Start.AddDays(1);

            var s1Sequence =
                SequenceFactory.Create(s1Start.DayNumber, s1.EndDate?.DayNumber, s1.RecurrenceInterval * 7);
            
            var overlap = s1Sequence.FindOverlapWith(s2Sequence);
            if (overlap?.Start < s1.StartDate.DayNumber)
                overlap = overlap.StartFromNext();
            
            if (overlap != null) return overlap;
        }

        return null;
    }
}