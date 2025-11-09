namespace Core;

public class Schedule {
    public DateOnly StartDate { get; }
    public DateOnly? EndDate { get; }
    public TimeOnly StartTime => _slots.MinBy(s => s.StartSpan)?.StartTime ?? throw new Exception("no slots specified");
    public TimeOnly EndTime => _slots.MaxBy(s => s.EndSpan)?.EndTime ?? throw new Exception("no slots specified");

    private readonly List<Slot> _slots;
    public IReadOnlyList<Slot> Slots => _slots.AsReadOnly();

    public RecurrenceType RecurrenceType { get; private set; } = RecurrenceType.Daily;
    public int RecurrenceInterval { get; private set; } = 1;
    public HashSet<DayOfWeek> DaysOfWeek { get; } = [];

    public bool IsForever => EndDate is null;
    public bool CrossesDayBoundary => _slots.Any(s => s.ExtendsBeyondBoundary);
    public bool StartsAtMidnight => _slots.MinBy(s => s.StartSpan)?.StartTime == TimeOnly.MinValue;
    public bool EndsAtMidnight => _slots.MaxBy(s => s.EndSpan)?.EndTime == TimeOnly.MinValue;

    public Schedule(DateOnly startDate, DateOnly? endDate = null, TimeOnly? startTime = null, TimeOnly? endTime = null)
        : this([new Slot(startTime ?? TimeOnly.MinValue, endTime ?? TimeOnly.MaxValue)], startDate, endDate) { }

    public Schedule(IEnumerable<Slot> slots, DateOnly startDate, DateOnly? endDate = null) {
        if (startDate > endDate)
            throw new ArgumentException("end of schedule should not come before its start");
        
        var slotsList = slots.ToList();
        for (var i = 0; i < slotsList.Count; i++) {
            for (var j = i + 1; j < slotsList.Count; j++) {
                if (slotsList[i].OverlapsWith(slotsList[j]))
                    throw new ArgumentException("Overlapping slots specified");
            }
        }

        if (slotsList.MaxBy(s => s.EndSpan)?.EndSpan
            - slotsList.MinBy(s => s.StartSpan)?.StartSpan
            > TimeSpan.FromHours(24))
            throw new ArgumentOutOfRangeException(nameof(slots), "all slots should lie within 24 hours");
        
        _slots = slotsList;
        StartDate = startDate;
        EndDate = endDate;
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