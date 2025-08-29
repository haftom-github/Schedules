using Core.Enums;
using Core.Overlap;

namespace Core.Entities;

public class Schedule {
    private readonly HashSet<DayOfWeek> _recurrenceDays = [];
    
    public TimeOnly StartTime { get; }
    public TimeOnly EndTime { get; }
    
    public DateOnly StartDate { get; }
    public DateOnly? EndDate { get; }
    
    public RecurrenceType RecurrenceType { get; private set; } = RecurrenceType.Daily;
    
    public IReadOnlySet<DayOfWeek> RecurrenceDays => _recurrenceDays;
    public int RecurrenceInterval { get; private set; } = 1;

    public bool CrossesBoundary => EndTime < StartTime;
    public Schedule(TimeOnly startTime, TimeOnly endTime, DateOnly startDate, DateOnly? endDate = null) {
        
        if (startTime == endTime)
            throw new ArgumentException("end time can not be equal to start time", nameof(endTime));
        
        if (endDate < startDate)
            throw new ArgumentException("End date cannot be earlier than start date.", nameof(endDate));
        
        this.StartTime = startTime;
        this.EndTime = endTime;
        this.StartDate = startDate;
        this.EndDate = endDate;
    }
    
    public void RecurWeekly(List<DayOfWeek> daysOfWeek, int interval = 1) {
        if (daysOfWeek == null || daysOfWeek.Count == 0)
            throw new ArgumentException("Days of week cannot be null or empty.", nameof(daysOfWeek));
        
        if (interval <= 0) 
            throw new ArgumentOutOfRangeException(nameof(interval), "Recurrence interval must be a positive integer.");

        _recurrenceDays.Clear();
        _recurrenceDays.UnionWith(daysOfWeek.ToHashSet());
        
        RecurrenceType = RecurrenceType.Weekly;
        RecurrenceInterval = interval;
    }
    
    public void RecurDaily(int interval = 1) {
        if (interval <= 0)
            throw new ArgumentOutOfRangeException(nameof(interval), "Recurrence interval must be a positive integer.");
        
        RecurrenceType = RecurrenceType.Daily;
        RecurrenceInterval = interval;
    }
    
    public void UpdateRecurrenceInterval(int interval) {
        if (interval <= 0)
            throw new ArgumentOutOfRangeException(nameof(interval), "Recurrence interval must be a positive integer.");
        
        RecurrenceInterval = interval;
    }
    
    public bool Overlaps(Schedule other) {
        var overlapDetector = OverlapDetectorFactory.Create(RecurrenceType, other.RecurrenceType);
        return overlapDetector.IsOverlapping(this, other);
    }

    public (TimeOnly start, TimeOnly end) GetAvailableTime(DateOnly date) {
        throw new NotImplementedException();
    }
}