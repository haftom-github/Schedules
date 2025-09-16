using Core.Entities;

namespace Core.Test.ScheduleTests;

public class DailyRecurrenceBoundaryCrossingTests {
    private readonly DateOnly _today = new(2025, 9, 8);
    private readonly DateOnly _tomorrow;
    private readonly DateOnly _yesterday;
    private readonly DateOnly _afterTomorrow;

    private readonly TimeOnly _twoOClock = new(2, 0);
    private readonly TimeOnly _threeOClock = new(3, 0);
    private readonly TimeOnly _fourOClock = new(4, 0);
    private readonly TimeOnly _fiveOClock = new(5, 0);

    public DailyRecurrenceBoundaryCrossingTests()
    {
        _tomorrow = _today.AddDays(1);
        _yesterday = _today.AddDays(-1);
        _afterTomorrow = _today.AddDays(2);
    }
    
    [Fact]
    public void ShouldReturn_TwoPeriods()
    {
        var s = new Schedule(_today, null, _fourOClock, _twoOClock);
        var periods = s.SlotsAtDate(_tomorrow);
        Assert.Equal(2, periods.Count);
    }
    
    [Fact]
    public void SecondPeriod_ShouldMatchAfterMidnight()
    {
        var s = new Schedule(_today, null, _threeOClock, _twoOClock);
        var firstPeriod = s.SlotsAtDate(_tomorrow)[1];
        Assert.Equal(TimeOnly.MinValue, firstPeriod.Start);
        Assert.Equal(_twoOClock, firstPeriod.End);
    }
    
    [Fact]
    public void FirstPeriod_ShouldMatchBeforeMidnight()
    {
        var s = new Schedule(_today, null, _threeOClock, _twoOClock);
        var secondPeriod = s.SlotsAtDate(_today)[0];
        Assert.Equal(_threeOClock, secondPeriod.Start);
        Assert.Equal(TimeOnly.MaxValue, secondPeriod.End);
    }
    
    [Fact]
    public void ThereCannotBeTwoPeriods()
    {
        var s = new Schedule(_today, startTime: _fiveOClock, endTime: _threeOClock);
        Assert.Single(s.SlotsAtDate(_today));
    }
    
    [Fact]
    public void EvenDistanceFromStart_ShouldResultInPeriodBeforeMidnight_WhenRecurringEveryTwoDays()
    {
        var s = new Schedule(_yesterday, startTime: _fiveOClock, endTime: _fourOClock);
        s.UpdateRecurrence(interval: 2);

        var periods = s.SlotsAtDate(_yesterday);
        Assert.Single(periods);
        Assert.Equal(TimeOnly.MaxValue, periods[0].End);
        Assert.Equal(s.StartTime, periods[0].Start);

        periods = s.SlotsAtDate(_tomorrow);
        Assert.Single(periods);
        Assert.Equal(TimeOnly.MaxValue, periods[0].End);
        Assert.Equal(s.StartTime, periods[0].Start);
    }
    
    [Fact]
    public void OddDistanceFromStart_ShouldResultInPeriodAfterMidnight_WhenRecurringEveryTwoDays()
    {
        var s = new Schedule(_yesterday, startTime: _fiveOClock, endTime: _fourOClock);
        s.UpdateRecurrence(interval: 2);

        var periods = s.SlotsAtDate(_today);
        Assert.Single(periods);
        Assert.Equal(s.EndTime, periods[0].End);
        Assert.Equal(TimeOnly.MinValue, periods[0].Start);

        periods = s.SlotsAtDate(_afterTomorrow);
        Assert.Single(periods);
        Assert.Equal(s.EndTime, periods[0].End);
        Assert.Equal(TimeOnly.MinValue, periods[0].Start);
    }
}