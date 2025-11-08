namespace Core;

public record Slot(TimeOnly Start, TimeSpan Span) {
    public TimeOnly End => Start.Add(Span);
    public bool IsPositive =>  Span > TimeSpan.Zero;
    public bool IsFullDay => Start == TimeOnly.MinValue && Start == End;
    private long EndInTicks => Start.Ticks + Span.Ticks;
    
    public Slot(TimeOnly? start = null, TimeSpan? span = null)
        : this(start ?? TimeOnly.MinValue, 
            span ?? TimeSpan.FromTicks(TimeOnly.MaxValue.Ticks 
                - (start?.Ticks ?? TimeOnly.MinValue.Ticks) + 1)) {}
    public static Slot FullDay => new(TimeOnly.MinValue, TimeSpan.FromHours(24));
    public static Slot UntilMidnight(TimeOnly start) => new(start, TimeSpan.FromTicks(TimeOnly.MaxValue.Ticks - start.Ticks + 1));
    public static Slot FromMidnight(TimeSpan span) => new(TimeOnly.MinValue, span);

    public Slot Overlap(Slot other) {
        return new Slot(Start > other.Start ? Start : other.Start, TimeSpan.FromTicks(
            Math.Min(EndInTicks, other.EndInTicks) 
            - Math.Max(Start.Ticks, other.Start.Ticks)));
    }
}
