namespace Core;

public record Slot(TimeSpan StartSpan, TimeSpan EndSpan) {
    public TimeOnly EndTime => TimeOnly.MinValue.Add(EndSpan);
    public TimeOnly StartTime => TimeOnly.MinValue.Add(StartSpan);
    public TimeSpan Span => EndSpan - StartSpan;
    public bool ExtendsBeyondBoundary => EndSpan > TimeSpan.FromDays(1);
    public bool IsEmpty => Span <= TimeSpan.Zero;
    public bool IsFullDay => StartTime == TimeOnly.MinValue && StartTime == EndTime;
    
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
    
    public List<Slot> Overlap(List<Slot> otherSlots) 
        => otherSlots.Select(Overlap)
            .ToList();

    public bool OverlapsWith(Slot otherSlot) => !Overlap(otherSlot).IsEmpty;

    public Slot BeforeMidnight => Overlap(FullDay);
    public Slot AfterMidnight => Overlap(new Slot(TimeSpan.FromHours(24), TimeSpan.FromHours(48)));

    public List<Slot> SplitAtMidnight() {
        List<Slot> slices = [BeforeMidnight, AfterMidnight];
        return slices.Where(s => !s.IsEmpty).ToList();
    }
}
