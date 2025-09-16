using Core.Entities;

namespace Core.Test.ScheduleTests;

public class DefaultBehaviorTests
{
    private readonly DateOnly _today = new(2025, 9, 8);
    private readonly DateOnly _tomorrow;

    private readonly TimeOnly _threeOClock = new(3, 0);
    private readonly TimeOnly _fourOClock = new(4, 0);

    public DefaultBehaviorTests()
    {
        _tomorrow = _today.AddDays(1);
    }

    [Fact]
    public void IsForever() {
        var s = new Schedule(_today);
        Assert.True(s.IsForever);
    }

    [Fact]
    public void IsNotForever_IfEndSpecified()
    {
        Assert.False(new Schedule(_today, _tomorrow).IsForever);
    }

    [Fact]
    public void EndShouldNotComeBeforeStart()
    {
        Assert.Throws<ArgumentException>(() => new Schedule(_tomorrow, _today));
    }

    [Fact]
    public void ShouldRecurDaily()
    {
        Assert.Equal(RecurrenceType.Daily, new Schedule(_today).RecurrenceType);
    }

    [Fact]
    public void HasRecurrenceIntervalOf_One()
    {
        Assert.Equal(1, new Schedule(_today).RecurrenceInterval);
    }

    [Fact]
    public void ShouldNotAllowANonPositiveRecurrenceInterval()
    {
        var s = new Schedule(_today);
        Assert.Throws<ArgumentException>(() => s.UpdateRecurrence(interval: 0));
    }

    [Fact]
    public void StartTimeCannotBeEqualToEndTime()
    {
        Assert.Throws<ArgumentException>(() => new Schedule(_today, _tomorrow, _threeOClock, _threeOClock));
    }

    [Fact]
    public void CrossBoundary_Allowed()
    {
        var s = new Schedule(_today, startTime: _fourOClock, endTime: _threeOClock);
        Assert.True(s.CrossesDayBoundary);
    }

    [Fact]
    public void DoesNotCrossBoundary_WhenStartTimeComesBeforeEndTime()
    {
        var s = new Schedule(_today, startTime: _threeOClock, endTime: _fourOClock);
        Assert.False(s.CrossesDayBoundary);
    }
}