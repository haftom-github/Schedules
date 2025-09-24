using Core.Options;
using Core.Sequences;

namespace Core.Services;

public static class OverlapDetector {
    public static List<Schedule> OverlapScheduleWith(this Schedule schedule1, Schedule schedule2) {
        var ownSequences = schedule1.ToSequencesList();
        var otherSequences = schedule2.ToSequencesList();

        List<Schedule> overlapSchedules = [];
        foreach(var ownSequence in  ownSequences) {
            foreach (var otherSequence in otherSequences) {
                var overlapSeq = ownSequence.FindOverlapWith(otherSequence);
                if (overlapSeq is null || overlapSeq.IsEmpty)
                    continue;

                var ownTimeRange = ownSequence.Tag == "before"
                    ? schedule1.PeriodBeforeMidnight() : schedule1.PeriodAfterMidnight();
                
                var otherTimeRange = otherSequence.Tag == "before"
                    ? schedule2.PeriodBeforeMidnight() : schedule2.PeriodAfterMidnight();

                var commonRange = CommonRange(ownTimeRange, otherTimeRange);
                if (!commonRange.IsPositive) continue;
                var startDate = DateOnly.FromDayNumber(overlapSeq.Start);
                DateOnly? endDate = overlapSeq.End != null 
                    ? DateOnly.FromDayNumber(overlapSeq.End!.Value) 
                    : null;
                var overlapSchedule = new Schedule(startDate, endDate, commonRange.Start, commonRange.End);
                overlapSchedule.UpdateRecurrence(interval: overlapSeq.Interval);
                overlapSchedules.Add(overlapSchedule);
            }
        }

        return Merge(overlapSchedules);
    }

    private static List<Schedule> Merge(List<Schedule> schedules) {
        for (var i = 0; i < schedules.Count; i++) {
            
            for (var j = i + 1; j < schedules.Count; j++) {
                var shift = FindShiftWith(schedules[i], schedules[j]);
                if (shift is not 1) continue;
                if (!(schedules[i].EndsAtMidnight && schedules[j].StartsAtMidnight)) continue;
                var newStartDate = schedules[i].StartDate;
                var newEndDate = schedules[i].EndDate;
                var newStartTime = schedules[i].StartTime;
                var newEndTime = schedules[j].EndTime;
                var merged = new Schedule(newStartDate, newEndDate, newStartTime, newEndTime);
                merged.UpdateRecurrence(interval: schedules[i].RecurrenceInterval);
                schedules.RemoveAt(j);
                schedules[i] = merged;
                break;
            }
        }

        for (var i = 0; i < schedules.Count; i++) {
            if (schedules[i].RecurrenceInterval % 7 != 0) continue;
            var weeklyRecurrenceInterval = schedules[i].RecurrenceInterval / 7;
            schedules[i].UpdateRecurrence(RecurrenceType.Weekly, daysOfWeek: [schedules[i].StartDate.DayOfWeek], interval: weeklyRecurrenceInterval);
            for (var j = i + 1; j < schedules.Count; j++) {
                if (schedules[j].RecurrenceInterval 
                    != schedules[i].RecurrenceInterval * 7) continue;
                
                if (schedules[j].StartTime != schedules[i].StartTime
                    || schedules[j].EndTime != schedules[i].EndTime)
                    continue;
                
                schedules[i].DaysOfWeek.Add(schedules[j].StartDate.DayOfWeek);
                schedules.RemoveAt(j);
                j--;
            }
        }

        return schedules;
    }
    
    private static int? FindShiftWith(Schedule schedule1, Schedule schedule2) {
        if (schedule1.RecurrenceInterval 
            != schedule2.RecurrenceInterval) return null;

        var seq = schedule1.ToSequencesList().First(s => s.Tag == "before");
        var otherSeq = schedule2.ToSequencesList().First(s => s.Tag == "before");

        if (seq.IsFinite != otherSeq.IsFinite) return null;
        
        var shift = otherSeq.Start - seq.Start;
        if (otherSeq.End is null) return shift;
        if (otherSeq.End - seq.End != shift)
            return null;
        
        return shift;
    }
    
    private static TimeOnly Max(TimeOnly t1, TimeOnly t2) => t1 > t2 ? t1 : t2;
    private static TimeOnly Min(TimeOnly t1, TimeOnly t2) => t1 < t2 ? t1 : t2;
    
    private static Slot CommonRange(Slot slot1, Slot slot2) {
        return new Slot(Max(slot1.Start, slot2.Start), Min(slot1.End, slot2.End));
    }
}

public static class ScheduleExtensions {
    private static List<Schedule> SplitOnDayBoundary(this Schedule schedule) {
        if (!schedule.CrossesDayBoundary)
            return [schedule];

        var beforeMidnight = new Schedule(
            schedule.StartDate, 
            schedule.EndDate, 
            schedule.StartTime, 
            TimeOnly.MaxValue
        );
        
        beforeMidnight.UpdateRecurrence(
            type: schedule.RecurrenceType, 
            daysOfWeek: schedule.DaysOfWeek, 
            interval: schedule.RecurrenceInterval
        );

        var afterMidnight = new Schedule(
            schedule.StartDate.AddDays(1),
            schedule.EndDate?.AddDays(1),
            TimeOnly.MinValue, 
            schedule.EndTime
        );

        var shiftedDaysOfWeek = schedule.DaysOfWeek
            .Select(d => d.ToNextDayOfWeek())
            .ToHashSet();
        
        afterMidnight.UpdateRecurrence(
            type: schedule.RecurrenceType, 
            daysOfWeek: shiftedDaysOfWeek, 
            interval: schedule.RecurrenceInterval
        );
        
        return [beforeMidnight, afterMidnight];
    }
    public static List<ISequence> ToSequencesList(this Schedule schedule) {
        var splits = SplitOnDayBoundary(schedule);
        string[] tags = ["before", "after"];
        List<ISequence> sequences = [];

        for (var i = 0; i < splits.Count; i++) {
            switch (splits[i].RecurrenceType) {
                case RecurrenceType.Daily:
                    sequences.Add(
                        SequenceFactory.Create(
                            splits[i].StartDate.DayNumber,
                            splits[i].EndDate?.DayNumber,
                            splits[i].RecurrenceInterval, tags[i])
                    );
                    break;

                case RecurrenceType.Weekly:
                    if (splits[i].DaysOfWeek.Count == 0) break;
                    
                    foreach (var day in splits[i].DaysOfWeek)
                    {
                        var start = splits[i].StartDate.ToFirstDayOfWeek();
                        while (start.DayOfWeek != day)
                            start = start.AddDays(1);
                    
                        var sequence = SequenceFactory.Create(
                            start.DayNumber,
                            splits[i].EndDate?.DayNumber,
                            splits[i].RecurrenceInterval * 7,
                            tags[i]
                        );
                        if (sequence.Start < splits[i].StartDate.DayNumber)
                            sequence = sequence.StartFromIndex(1);

                        if (sequence.IsEmpty) continue;
                
                        sequences.Add(sequence);
                    }
                    break;
                
                default: throw new NotImplementedException();
            }
        }

        return sequences;
    }
    public static List<Slot> SlotsAtDate(this Schedule schedule, DateOnly date) {
        var sequences = schedule.ToSequencesList();
        var periods = new List<Slot>();

        foreach (var sequence in sequences) {
            if (sequence.IsMember(date.DayNumber)) {
                periods.Add(sequence.Tag == "before"
                    ? schedule.PeriodBeforeMidnight()
                    : schedule.PeriodAfterMidnight());
            }
        }

        return periods;
    }
    
    public static Slot PeriodBeforeMidnight(this Schedule schedule) =>
        schedule.CrossesDayBoundary 
            ? new Slot(schedule.StartTime) 
            : new Slot(schedule.StartTime, schedule.EndTime);

    public static Slot PeriodAfterMidnight(this Schedule schedule) =>
        schedule.CrossesDayBoundary 
            ? new Slot(end: schedule.EndTime) 
            : new Slot(schedule.StartTime, schedule.EndTime);

}