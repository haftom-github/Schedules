namespace Core;

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
    public bool StartsAtMidnight => StartTime == TimeOnly.MinValue;
    public bool EndsAtMidnight => EndTime == TimeOnly.MaxValue;

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
        
        if (interval is not null) {
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
}

public enum RecurrenceType
{
    Daily,
    Weekly
}