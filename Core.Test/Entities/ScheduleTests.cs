using Core.Entities;
using Core.Options;

namespace Core.Test.Entities;

public class ScheduleTests {

    private readonly DateOnly _today = TimeSettings.Today;
    private readonly DateOnly _tomorrow;
    private readonly DateOnly _yesterday;
    private readonly DateOnly _afterTomorrow;

    private readonly TimeOnly _twoOClock = new(2, 0);
    private readonly TimeOnly _threeOClock = new(3, 0);
    private readonly TimeOnly _fourOClock = new(4, 0);
    private readonly TimeOnly _fiveOClock = new(5, 0);

    public ScheduleTests()
    {
        _tomorrow = _today.AddDays(1);
        _yesterday = _today.AddDays(-1);
        _afterTomorrow = _today.AddDays(2);
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

    [Fact]
    public void ShouldRecurWeekly_AtSpecifiedInterval()
    {
        var s = new Schedule(_today);

        s.UpdateRecurrence(RecurrenceType.Weekly, daysOfWeek: [DayOfWeek.Monday]);
        Assert.Equal(RecurrenceType.Weekly, s.RecurrenceType);
        Assert.Single(s.DaysOfWeek);
        Assert.Contains(DayOfWeek.Monday, s.DaysOfWeek);

        s.UpdateRecurrence(RecurrenceType.Weekly, daysOfWeek: [], interval: 5);
        Assert.Equal(5, s.RecurrenceInterval);
        Assert.Empty(s.DaysOfWeek);
        Assert.Equal(RecurrenceType.Weekly, s.RecurrenceType);
    }
}