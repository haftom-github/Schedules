using Core.Entities;

namespace Core.Test.ScheduleTests;

public class DailyRecurrenceTests {
    private readonly DateOnly _today = new(2025, 9, 8);
    private readonly DateOnly _tomorrow;
    private readonly DateOnly _yesterday;
    private readonly DateOnly _afterTomorrow;

    private readonly TimeOnly _twoOClock = new(2, 0);
    private readonly TimeOnly _threeOClock = new(3, 0);
    private readonly TimeOnly _fourOClock = new(4, 0);
    private readonly TimeOnly _fiveOClock = new(5, 0);

    public DailyRecurrenceTests()
    {
        _tomorrow = _today.AddDays(1);
        _yesterday = _today.AddDays(-1);
        _afterTomorrow = _today.AddDays(2);
    }

    [Fact]
    public void ShouldReturnEmpty_WhenDateNotWithinSchedule()
    {
        var s = new Schedule(_today);
        Assert.Empty(s.SlotsAtDate(_yesterday));
    }

    [Fact]
    public void ShouldReturn_SingleFullDayPeriod_ForFullDaySchedules()
    {
        var s = new Schedule(_today);
        var periods = s.SlotsAtDate(_today);
        Assert.Single(periods);
        Assert.True(periods[0].IsFullDay);
    }

    [Fact]
    public void ShouldReturn_TwoPeriods_WhenDayBoundaryIsCrossed()
    {
        var s = new Schedule(_today, null, _fourOClock, _twoOClock);
        var periods = s.SlotsAtDate(_tomorrow);
        Assert.Equal(2, periods.Count);
    }

    [Fact]
    public void StartAndEndOfPeriod_ShouldEqualStartTime_WhenDayBoundaryNotCrossed()
    {
        var s = new Schedule(_today, null, _twoOClock, _threeOClock);
        var periods = s.SlotsAtDate(_today);
        Assert.Equal(_twoOClock, periods[0].Start);
        Assert.Equal(_threeOClock, periods[0].End);
    }

    [Fact]
    public void SecondPeriod_ShouldMatchAfterMidnight_WhenDayBoundaryIsCrossed()
    {
        var s = new Schedule(_today, null, _threeOClock, _twoOClock);
        var firstPeriod = s.SlotsAtDate(_tomorrow)[1];
        Assert.Equal(TimeOnly.MinValue, firstPeriod.Start);
        Assert.Equal(_twoOClock, firstPeriod.End);
    }

    [Fact]
    public void FirstPeriod_ShouldMatchBeforeMidnight_WhenDayBoundaryCrossed()
    {
        var s = new Schedule(_today, null, _threeOClock, _twoOClock);
        var secondPeriod = s.SlotsAtDate(_today)[0];
        Assert.Equal(_threeOClock, secondPeriod.Start);
        Assert.Equal(TimeOnly.MaxValue, secondPeriod.End);
    }

    [Fact]
    public void ThereCannotBeTwoPeriods_WhenRecurringDaily_AndCrossesBoundary()
    {
        var s = new Schedule(_today, startTime: _fiveOClock, endTime: _threeOClock);
        Assert.Single(s.SlotsAtDate(_today));
    }

    [Fact]
    public void EvenDistanceFromStart_ShouldResultInPeriodBeforeMidnight_WhenDaily_EveryTwoDays_CrossesBoundary()
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
    public void OddDistanceFromStart_ShouldResultInPeriodAfterMidnight_WhenDaily_EveryTwoDays_CrossesBoundary()
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

    [Fact]
    public void EmptyPeriods_WhenDateNotInSchedule()
    {
        var s = new Schedule(_yesterday, startTime: _fiveOClock, endTime: _fourOClock);
        s.UpdateRecurrence(interval: 3);

        Assert.Empty(s.SlotsAtDate(_tomorrow));
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