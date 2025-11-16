using System.Collections.Immutable;

namespace Core;

public record Schedule {
    public DateOnly StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public IReadOnlyList<Slot> Slices { get; init; }
    public Recurrence Recurrence { get; init; }

    public TimeOnly StartTime => Slices.MinBy(s => s.StartSpan)?.StartTime ?? throw new Exception("no slots specified");
    public TimeOnly EndTime => Slices.MaxBy(s => s.EndSpan)?.EndTime ?? throw new Exception("no slots specified");

    public bool IsForever => EndDate is null;
    public bool CrossesDayBoundary => Slices.Any(s => s.ExtendsBeyondBoundary);

    public Schedule(DateOnly startDate, DateOnly? endDate = null, TimeOnly? startTime = null, TimeOnly? endTime = null, Recurrence? recurrence = null)
        : this([new Slot(startTime ?? TimeOnly.MinValue, endTime ?? TimeOnly.MinValue)], startDate, endDate, recurrence) { }

    public Schedule(IEnumerable<Slot> slots, DateOnly startDate, DateOnly? endDate = null, Recurrence? recurrence = null) {
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
        
        Slices = slotsList;
        StartDate = startDate;
        EndDate = endDate;
        Recurrence = recurrence ?? Recurrence.Daily();
        if (Recurrence.Interval < 1)
            throw new ArgumentException("interval can not be less than 1");
    }
}

public enum RecurrenceType
{
    Daily,
    Weekly
}

public sealed record Recurrence(
    RecurrenceType Type, 
    int Interval, 
    ImmutableHashSet<DayOfWeek> DaysOfWeek
)
{ 
    public static Recurrence Daily(int interval = 1)
        => new(
            Type:RecurrenceType.Daily, 
            Interval:interval, 
            DaysOfWeek:ImmutableHashSet<DayOfWeek>.Empty
        );

    public static Recurrence Weekly(IEnumerable<DayOfWeek> daysOfWeek, int interval = 1)
        => new(
            Type:RecurrenceType.Weekly,
            Interval:interval,
            DaysOfWeek:daysOfWeek.ToImmutableHashSet()
        );
}
