using Core.Entities;
using Core.Sequences;

namespace Core.Overlap;

public class DailyOverlapDetector : BaseOverlapDetector {
    protected override ISequence? DetectSplit(Schedule s1, Schedule s2) {
        if (OverlapIsImpossible(s1, s2)) return null;

        var s1Sequence = SequenceFactory.Create(s1.StartDate.DayNumber, s1.EndDate?.DayNumber, s1.RecurrenceInterval);
        
        var s2Sequence = SequenceFactory.Create(s2.StartDate.DayNumber, s2.EndDate?.DayNumber, s2.RecurrenceInterval);
        
        return s1Sequence.FindOverlapWith(s2Sequence);
    }
}