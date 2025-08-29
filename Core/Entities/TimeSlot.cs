namespace Core.Entities;

public class TimeSlot {
    public DateOnly Date { get; }
    public TimeOnly StartTime { get; }
    public TimeSpan Span { get; private set; }

    public TimeSlot(DateOnly date, TimeOnly startTime, TimeSpan span) {
        if( span <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(span), "time span can not be zero");
        
        this.Date = date;
        this.StartTime = startTime;
        this.Span = span;
    }

    public TimeSlot(DateTime dateTime, TimeSpan span) 
        : this(DateOnly.FromDateTime(dateTime), TimeOnly.FromDateTime(dateTime), span) {}
}