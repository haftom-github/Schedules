namespace Core.Entities;

public class WorkSchedule(TimeOnly startTime, TimeOnly endTime, DateOnly startDate, DateOnly? endDate = null)
    : Schedule(startTime, endTime, startDate, endDate) {
    
    
    protected WorkSchedule() : this(TimeOnly.MinValue, TimeOnly.MaxValue, DateOnly.MinValue) { }
}