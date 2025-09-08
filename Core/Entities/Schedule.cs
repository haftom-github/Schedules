using Core.Sequences;
using Core.ValueObjects;

namespace Core.Entities;

public class Schedule {
    public DateOnly StartDate { get; }
    public DateOnly? EndDate { get; }
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }

    public RecurrenceType RecurrenceType { get; private set; } = RecurrenceType.Daily;
    public int RecurrenceInterval { get; private set; } = 1;
    public HashSet<DayOfWeek> DaysOfWeek { get; } = [];

    public bool IsForever => EndDate is null;
    public bool CrossesDayBoundary => StartTime > EndTime;

    public Schedule(DateOnly startDate, DateOnly? endDate = null, TimeOnly? startTime = null, TimeOnly? endTime = null) {
        
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
        var sequenceMap = ToSequencesMap();
        var periods = new List<Slot>();

        foreach (var (key, sequences) in sequenceMap) {
            if (sequences.Any(sequence => sequence.IsMember(date.DayNumber))) {
                periods.Add(key == "before"
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
        beforeMidnight.UpdateRecurrence(RecurrenceType, RecurrenceInterval);

        var afterMidnight = new Schedule(
            StartDate.AddDays(1),
            EndDate?.AddDays(1),
            TimeOnly.MinValue, EndTime);

        afterMidnight.UpdateRecurrence(RecurrenceType, RecurrenceInterval);

        return [beforeMidnight, afterMidnight];
    }

    private Dictionary<string, ISequence[]> ToSequencesMap()
    {
        var splits = SplitOnDayBoundary();
        var keys = new[] { "before", "after" };
        var map = new Dictionary<string, ISequence[]>();

        for (var i = 0; i < splits.Length; i++)
        {
            map[keys[i]] =
            [
                SequenceFactory.Create(
                    splits[i].StartDate.DayNumber,
                    splits[i].EndDate?.DayNumber,
                    splits[i].RecurrenceInterval
                )
            ];
        }

        return map;
    }

    private Slot PeriodBeforeMidnight() =>
        CrossesDayBoundary ? new Slot(StartTime) : new Slot(StartTime, EndTime);

    private Slot PeriodAfterMidnight() =>
        CrossesDayBoundary ? new Slot(end: EndTime) : new Slot(StartTime, EndTime);

    public void EndAtMidNight() {
        if (StartTime == TimeOnly.MaxValue)
            throw new Exception("invalid state: cannot stretch end time when start time is max value");
        
        EndTime = TimeOnly.MaxValue;
    }
    
    public void StartAtMidNight() {
        if (EndTime == TimeOnly.MinValue)
            throw new Exception("invalid state: cannot start at midnight when end time is min value");
        
        StartTime = TimeOnly.MinValue;
    }
}

public enum RecurrenceType
{
    Daily,
    Weekly
}