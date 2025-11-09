namespace Core;

public record Slot(TimeSpan StartSpan, TimeSpan EndSpan) {
    public TimeOnly End => TimeOnly.MinValue.Add(EndSpan);
    public TimeOnly Start => TimeOnly.MinValue.Add(StartSpan);
    public TimeSpan Span => EndSpan - StartSpan;
    public bool IsPositive => EndSpan > StartSpan;
    public bool IsEmpty => Span <= TimeSpan.Zero;
    public bool IsFullDay => Start == TimeOnly.MinValue && Start == End;
    
    public Slot(TimeSpan? startSpan = null, TimeSpan? EndSpan = null)
        : this(startSpan ?? TimeSpan.Zero, EndSpan ?? TimeSpan.FromHours(24)) { }
    public static Slot FullDay => new(TimeSpan.Zero, TimeSpan.FromHours(24));
    
    public Slot(TimeOnly start, TimeOnly end) 
        : this(start.ToTimeSpan(), start >= end ? end.ToTimeSpan().Add(TimeSpan.FromHours(24)) : end.ToTimeSpan()){}

    public Slot Overlap(Slot other) {
        return new Slot(
            TimeSpan.FromTicks(Math.Max(StartSpan.Ticks, other.StartSpan.Ticks)), 
            TimeSpan.FromTicks(Math.Min(EndSpan.Ticks, other.EndSpan.Ticks))
        );
    }
}
