using Core.Entities;

namespace Core.Test.ScheduleTests;

public class DailyRecurrenceNotCrossingBoundaryTests {
    private readonly DateOnly _today = new(2025, 9, 8);
    private readonly DateOnly _tomorrow;
    private readonly DateOnly _yesterday;

    private readonly TimeOnly _twoOClock = new(2, 0);
    private readonly TimeOnly _threeOClock = new(3, 0);

    public DailyRecurrenceNotCrossingBoundaryTests()
    {
        _tomorrow = _today.AddDays(1);
        _yesterday = _today.AddDays(-1);
    }

    [Fact]
    public void ShouldHaveNoSlots_WhenDateNotWithinSchedule()
    {
        var s = new Schedule(_today);
        Assert.Empty(s.SlotsAtDate(_yesterday));
        
        s.UpdateRecurrence(interval: 3);
        Assert.Empty(s.SlotsAtDate(_tomorrow));
    }

    [Fact]
    public void ShouldHave_SingleFullDaySlot_WhenDateWithInSchedule_ForFullDaySchedules()
    {
        var s = new Schedule(_today);
        
        var periods = s.SlotsAtDate(_today);
        
        Assert.Single(periods);
        Assert.True(periods[0].IsFullDay);
    }

    [Fact]
    public void StartAndEndOfSlot_ShouldEqualThoseSetInTheSchedule()
    {
        var s = new Schedule(_today, null, _twoOClock, _threeOClock);
        
        var periods = s.SlotsAtDate(_today);
        
        Assert.Equal(_twoOClock, periods[0].Start);
        Assert.Equal(_threeOClock, periods[0].End);
    }

    [Fact]
    public void ShouldUpdateRecurrence()
    {
        var s = new Schedule(_today);

        s.UpdateRecurrence(interval: 2);
        Assert.Equal(2, s.RecurrenceInterval);

        s.UpdateRecurrence(type: RecurrenceType.Weekly);
        Assert.Equal(2, s.RecurrenceInterval);
        Assert.Equal(RecurrenceType.Weekly, s.RecurrenceType);

        s.UpdateRecurrence(RecurrenceType.Daily, 3);
        Assert.Equal(RecurrenceType.Daily, s.RecurrenceType);
        Assert.Equal(3, s.RecurrenceInterval);
    }
}