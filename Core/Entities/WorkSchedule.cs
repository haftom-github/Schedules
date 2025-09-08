namespace Core.Entities;

public class WorkSchedule(
    DateOnly startDate,
    DateOnly? endDate = null,
    TimeOnly? startTime = null,
    TimeOnly? endTime = null)
    : Schedule(startDate, endDate, startTime, endTime) { }