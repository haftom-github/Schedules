using Core.Enums;
using Core.Overlap;

namespace Core.Entities;

public class Schedule {
    private readonly HashSet<DayOfWeek> _daysOfWeek = [];
    
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }
    
    public DateOnly StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }
    
    public RecurrenceType RecurrenceType { get; private set; } = RecurrenceType.Daily;
    
    public IReadOnlySet<DayOfWeek> DaysOfWeek => _daysOfWeek;
    public int RecurrenceInterval { get; private set; } = 1;

    public bool CrossesBoundary => EndTime < StartTime;
    public Schedule(TimeOnly startTime, TimeOnly endTime, DateOnly startDate, DateOnly? endDate = null) {
        
        if (startTime == endTime)
            throw new ArgumentException("end time can not be equal to start time", nameof(endTime));
        
        if (endDate < startDate)
            throw new ArgumentException("End date cannot be earlier than start date.", nameof(endDate));
        
        StartTime = startTime;
        EndTime = endTime;
        StartDate = startDate;
        EndDate = endDate;
    }

    public Schedule(Schedule schedule) {
        StartTime = schedule.StartTime;
        EndTime = schedule.EndTime;
        
        StartDate = schedule.StartDate;
        EndDate = schedule.EndDate;
        
        RecurrenceType = schedule.RecurrenceType;
        RecurrenceInterval = schedule.RecurrenceInterval;
        _daysOfWeek = new HashSet<DayOfWeek>(schedule._daysOfWeek);
    }
    
    public void RecurWeekly(List<DayOfWeek> daysOfWeek, int interval = 1) {
        if (daysOfWeek == null || daysOfWeek.Count == 0)
            throw new ArgumentException("Days of week cannot be null or empty.", nameof(daysOfWeek));
        
        if (interval <= 0) 
            throw new ArgumentOutOfRangeException(nameof(interval), "Recurrence interval must be a positive integer.");

        _daysOfWeek.Clear();
        _daysOfWeek.UnionWith(daysOfWeek.ToHashSet());
        
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

    public void UpdateStartDate(DateOnly startDate) {
        if (EndDate < startDate)
            throw new ArgumentException("start date cannot be later than End date.", nameof(startDate));
        
        StartDate = startDate;
    }

    public void UpdateEndDate(DateOnly? endDate) {
        if (StartDate > endDate)
            throw new ArgumentException("End date cannot be earlier than start date.", nameof(endDate));
        
        EndDate = endDate;
    }

    public void UpdateStartTime(TimeOnly startTime) {
        if (EndTime == startTime)
            throw new ArgumentException("start time can not be equal to end time", nameof(startTime));
        
        StartTime = startTime;
    }
    
    public void UpdateEndTime(TimeOnly endTime) {
        if (StartTime == endTime)
            throw new ArgumentException("end time can not be equal to start time", nameof(endTime));

        EndTime = endTime;
    }

    public void UpdateDaysOfWeek(List<DayOfWeek> daysOfWeek) {
        _daysOfWeek.Clear();
        _daysOfWeek.UnionWith(daysOfWeek.ToHashSet());
    }
    
    public bool Overlaps(Schedule other) {
        var overlapDetector = OverlapDetectorFactory.Create(RecurrenceType, other.RecurrenceType);
        return overlapDetector.IsOverlapping(this, other);
    }
}