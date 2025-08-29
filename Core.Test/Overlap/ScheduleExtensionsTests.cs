using Core.Entities;
using Core.Options;
using Core.Overlap;

namespace Core.Test.Overlap;

public class ScheduleExtensionsTests {
    private readonly DateOnly _today = TimeSettings.Today;
    [Fact]
    public void Split_ShouldReturnTheScheduleAndNull_WhenScheduleDoesNotCrossDayBoundary() {
        var startTime = TimeOnly.MinValue;
        var endTime = TimeOnly.MaxValue;
        var schedule = new Schedule(startTime, endTime, _today);

        var (s, e) = schedule.SplitOnDayBoundary();
        
        Assert.Equal(schedule, s);
        Assert.Null(e);
    }

    [Fact]
    public void Split_ShouldOnlyChangeEndTimeToGetS_WhenScheduleCrossesDayBoundary() {
        var startTime = new TimeOnly(20, 0);
        var endTime = new TimeOnly(4, 0);
        var schedule = new Schedule(startTime, endTime, _today);
        
        var (s, _) = schedule.SplitOnDayBoundary();
        
        Assert.Equal(s.StartTime, startTime);
        Assert.Equal(s.EndTime, TimeOnly.MaxValue);
        Assert.Equal(s.RecurrenceType, schedule.RecurrenceType);
        Assert.Equal(s.StartDate, schedule.StartDate);
        Assert.Equal(s.EndDate, schedule.EndDate);
        Assert.Equal(s.DaysOfWeek, schedule.DaysOfWeek);
        
        schedule.RecurWeekly([DayOfWeek.Monday], 7);
        (s, _) = schedule.SplitOnDayBoundary();
        Assert.Equal(s.DaysOfWeek, schedule.DaysOfWeek);
        Assert.Equal(s.RecurrenceInterval, schedule.RecurrenceInterval);
    }
    
    [Fact]
    public void Split_ShouldUpdateStartTimeToGetE_WhenScheduleCrossesDayBoundary() {
        var startTime = new TimeOnly(20, 0);
        var endTime = new TimeOnly(4, 0);
        var schedule = new Schedule(startTime, endTime, _today);
        
        var (_, e) = schedule.SplitOnDayBoundary();

        Assert.NotNull(e);
        Assert.Equal(e.StartTime, TimeOnly.MinValue);
        Assert.Equal(e.EndTime, schedule.EndTime);
        Assert.Equal(e.RecurrenceType, schedule.RecurrenceType);
        Assert.Equal(e.StartDate, schedule.StartDate.AddDays(1));
        Assert.Equal(e.EndDate, schedule.EndDate?.AddDays(1));
        Assert.Equal(e.DaysOfWeek, schedule.DaysOfWeek);

        schedule.RecurWeekly([DayOfWeek.Sunday, DayOfWeek.Monday], 7);
        (_, e) = schedule.SplitOnDayBoundary();
        
        Assert.NotNull(e);
        Assert.Equal(e.DaysOfWeek, [DayOfWeek.Monday, DayOfWeek.Tuesday]);
    }
}