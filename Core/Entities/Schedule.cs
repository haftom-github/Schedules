using Core.Options;
using Core.Sequences;
using Core.ValueObjects;

namespace Core.Entities;

public class Schedule {
    public DateOnly StartDate { get; }
    public DateOnly? EndDate { get; }
    public TimeOnly StartTime { get; }
    public TimeOnly EndTime { get; }

    public RecurrenceType RecurrenceType { get; private set; } = RecurrenceType.Daily;
    public int RecurrenceInterval { get; private set; } = 1;
    public HashSet<DayOfWeek> DaysOfWeek { get; } = [];

    public bool IsForever => EndDate is null;
    public bool CrossesDayBoundary => StartTime > EndTime;
    private bool StartsAtMidnight => StartTime == TimeOnly.MinValue;
    private bool EndsAtMidnight => EndTime == TimeOnly.MaxValue;

    public  Schedule(DateOnly startDate, DateOnly? endDate = null, TimeOnly? startTime = null, TimeOnly? endTime = null) {
        
        if (startDate > endDate)
            throw new ArgumentException("end of schedule should not come before its start");
        
        StartDate = startDate;
        EndDate = endDate;
        StartTime = startTime ?? TimeOnly.MinValue;
        EndTime = endTime ?? TimeOnly.MaxValue;
        
        if (StartTime == EndTime)
            throw new ArgumentException("start time and end time cannot be equal");
    }

    public void UpdateRecurrence(RecurrenceType? type = null, int? interval = null, HashSet<DayOfWeek>? daysOfWeek = null) {
        
        if (interval is not null)
        {
            if (interval <= 0)
                throw new ArgumentException("recurrence interval must be positive");

            RecurrenceInterval = interval.Value;
        }

        if (type is not null)
            RecurrenceType = type.Value;

        if (daysOfWeek is null) return;
        DaysOfWeek.Clear();
        foreach (var day in daysOfWeek)
            DaysOfWeek.Add(day);
    }

    public List<Slot> SlotsAtDate(DateOnly date) 
    {
        var sequences = ToSequencesList();
        var periods = new List<Slot>();

        foreach (var sequence in sequences) {
            if (sequence.IsMember(date.DayNumber)) {
                periods.Add(sequence.Tag == "before"
                    ? PeriodBeforeMidnight()
                    : PeriodAfterMidnight());
            }
        }

        return periods;
    }

    public Schedule[] SplitOnDayBoundary() 
    {
        if (!CrossesDayBoundary)
            return [this];

        var beforeMidnight = new Schedule(StartDate, EndDate, StartTime, TimeOnly.MaxValue);
        beforeMidnight.UpdateRecurrence(type: RecurrenceType, daysOfWeek: DaysOfWeek, interval: RecurrenceInterval);

        var afterMidnight = new Schedule(
            StartDate.AddDays(1),
            EndDate?.AddDays(1),
            TimeOnly.MinValue, EndTime);

        var shiftedDaysOfWeek = DaysOfWeek.Select(d => d.ToNextDayOfWeek()).ToHashSet();
        afterMidnight.UpdateRecurrence(type: RecurrenceType, daysOfWeek: shiftedDaysOfWeek, interval: RecurrenceInterval);
        return [beforeMidnight, afterMidnight];
    }

    private List<ISequence> ToSequencesList() {
        var splits = SplitOnDayBoundary();
        string[] tags = ["before", "after"];
        List<ISequence> sequences = [];

        for (var i = 0; i < splits.Length; i++) {
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

    private Slot PeriodBeforeMidnight() =>
        CrossesDayBoundary ? new Slot(StartTime) : new Slot(StartTime, EndTime);

    private Slot PeriodAfterMidnight() =>
        CrossesDayBoundary ? new Slot(end: EndTime) : new Slot(StartTime, EndTime);
    
    public Schedule? OverlapScheduleWith(Schedule other) {
        var ownSequences = ToSequencesList();
        var otherSequences = other.ToSequencesList();

        List<Schedule> overlapSchedules = [];
        foreach(var ownSequence in  ownSequences) {
            foreach (var otherSequence in otherSequences) {
                var overlapSeq = ownSequence.FindOverlapWith(otherSequence);
                if (overlapSeq is null || overlapSeq.IsEmpty)
                    continue;

                var ownTimeRange = ownSequence.Tag == "before"
                    ? PeriodBeforeMidnight() : PeriodAfterMidnight();
                
                var otherTimeRange = otherSequence.Tag == "before"
                    ? other.PeriodBeforeMidnight() : other.PeriodAfterMidnight();

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

        return overlapSchedules.Count == 0 ? null : Merge(overlapSchedules)[0];
    }

    private static List<Schedule> Merge(List<Schedule> schedules) {
        for (var i = 0; i < schedules.Count; i++) {
            
            for (var j = i + 1; j < schedules.Count; j++) {
                var shift = schedules[i].FindShiftWith(schedules[j]);
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
                
                schedules[i].DaysOfWeek.Add(schedules[j].StartDate.DayOfWeek);
                schedules.RemoveAt(j);
                j--;
            }
        }

        return schedules;
    }

    private int? FindShiftWith(Schedule other) {
        if (RecurrenceInterval != other.RecurrenceInterval) return null;

        var seq = ToSequencesList().First(s => s.Tag == "before");
        var otherSeq = other.ToSequencesList().First(s => s.Tag == "before");

        if (seq.IsFinite != otherSeq.IsFinite) return null;
        
        var shift = otherSeq.Start - seq.Start;
        if (otherSeq.End is null) return shift;
        if (otherSeq.End - seq.End != shift)
            return null;
        
        return shift;
    }

    private Slot CommonRange(Slot slot1, Slot slot2) {
        return new Slot(Max(slot1.Start, slot2.Start), Min(slot1.End, slot2.End));
    }
    private TimeOnly Max(TimeOnly t1, TimeOnly t2) => t1 > t2 ? t1 : t2;
    private TimeOnly Min(TimeOnly t1, TimeOnly t2) => t1 < t2 ? t1 : t2;
}

public enum RecurrenceType
{
    Daily,
    Weekly
}