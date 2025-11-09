using Core.Services;

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
    private readonly TimeOnly _sixOClock = new(6, 0);
    private readonly TimeOnly _eightOClock = new(8, 0);
    private readonly TimeOnly _nineOClock = new(9, 0);

    public DailyRecurrenceBoundaryCrossingTests()
    {
        _tomorrow = _today.AddDays(1);
        _yesterday = _today.AddDays(-1);
        _afterTomorrow = _today.AddDays(2);
    }
    
    [Fact]
    public void AnyDayInTheMiddleOfTheSchedule_ShouldHaveTwoSlots_WhenIntervalIsOne()
    {
        var s = new Schedule(_today, null, _fourOClock, _twoOClock);
        
        var periods = s.SlotsAtDate(_tomorrow);
        Assert.Equal(2, periods.Count);
    }
    
    // an after midnight slot is a slot that starts at the beginning of the day and ends at the specified end time
    // a before midnight slot is a slot that starts at the specified start time and ends at the end of the day
    [Fact]
    public void SecondSlot_ShouldBeAnAfterMidnightSlot()
    {
        var s = new Schedule(_today, null, _threeOClock, _twoOClock);
        
        var firstPeriod = s.SlotsAtDate(_tomorrow)[1];
        
        Assert.Equal(TimeOnly.MinValue, firstPeriod.StartTime);
        Assert.Equal(_twoOClock, firstPeriod.EndTime);
    }
    
    [Fact]
    public void SecondSlot_ShouldBeABeforeMidnightSlot()
    {
        var s = new Schedule(_today, null, _threeOClock, _twoOClock);
        
        var secondPeriod = s.SlotsAtDate(_today)[0];
        
        Assert.Equal(_threeOClock, secondPeriod.StartTime);
        Assert.Equal(TimeOnly.MinValue, secondPeriod.EndTime);
    }
    
    [Fact]
    public void TheFirstDayWithInSchedule_ShouldNotHaveTwoSlots()
    {
        var s = new Schedule(_today, startTime: _fiveOClock, endTime: _threeOClock);
        Assert.Single(s.SlotsAtDate(_today));
    }
    
    [Fact]
    public void DatesOnEvenDistanceFromStart_ShouldHaveASingleBeforeMidNightSlot_WhenRecurringEveryTwoDays()
    {
        var s = new Schedule(_yesterday, startTime: _fiveOClock, endTime: _fourOClock);
        s.UpdateRecurrence(interval: 2);

        var periods = s.SlotsAtDate(_yesterday);
        Assert.Single(periods);
        Assert.Equal(TimeOnly.MinValue, periods[0].EndTime);
        Assert.Equal(s.StartTime, periods[0].StartTime);

        periods = s.SlotsAtDate(_tomorrow);
        Assert.Single(periods);
        Assert.Equal(TimeOnly.MinValue, periods[0].EndTime);
        Assert.Equal(s.StartTime, periods[0].StartTime);
    }
    
    [Fact]
    public void DatesOnOddDistanceFromStart_ShouldHaveASingleAfterMidnightSot_WhenRecurringEveryTwoDays()
    {
        var s = new Schedule(_yesterday, startTime: _fiveOClock, endTime: _fourOClock);
        s.UpdateRecurrence(interval: 2);

        var periods = s.SlotsAtDate(_today);
        Assert.Single(periods);
        Assert.Equal(s.EndTime, periods[0].EndTime);
        Assert.Equal(TimeOnly.MinValue, periods[0].StartTime);

        periods = s.SlotsAtDate(_afterTomorrow);
        Assert.Single(periods);
        Assert.Equal(s.EndTime, periods[0].EndTime);
        Assert.Equal(TimeOnly.MinValue, periods[0].StartTime);
    }
    
    # region OverlapDetection

    [Fact]
    public void TheOverlapShould_CrossBoundary_WhenOtherAlsoCrossesBoundary() {
        
        // ..##|###...##|###...##|###..
        // ..##|###...##|###...##|###..
        var s = new Schedule(_today, _today, _fourOClock, _threeOClock);
        var other = new Schedule(_today, _today, _fourOClock, _threeOClock);

        var overlaps = s.OverlapScheduleWith(other);
        Assert.Single(overlaps);
        Assert.True(overlaps[0].CrossesDayBoundary);
    }
    
    [Fact]
    public void TheOverlapShould_ShouldNotCrossBoundary_WhenOtherDoesNotCrossBoundary() {
        
        // .###|##...###|##...###|##..
        // ....|.#......|.#......|.#..
        var s = new Schedule(_today, _today, _fourOClock, _threeOClock);
        var other = new Schedule(_tomorrow, _afterTomorrow, _twoOClock, _threeOClock);

        var overlaps = s.OverlapScheduleWith(other);
        Assert.Single(overlaps);
        Assert.False(overlaps[0].CrossesDayBoundary);
    }

    [Fact]
    public void TheStartOfTheOverlap_WhenTheFirstScheduleStartsAfterTheOther() {
        // |........|......##|####..##|####..
        // |..##....|..##....|..##....|..##..
        var s = new Schedule(_tomorrow, _afterTomorrow, _fourOClock, _threeOClock);
        var other = new Schedule(_today, _afterTomorrow, _twoOClock, _threeOClock);
        
        var overlaps = s.OverlapScheduleWith(other);
        
        Assert.Single(overlaps);
        Assert.Equal(_afterTomorrow, overlaps[0].StartDate);
        Assert.Equal(_afterTomorrow, overlaps[0].EndDate);
        Assert.Equal(_twoOClock, overlaps[0].StartTime);
        Assert.Equal(_threeOClock, overlaps[0].EndTime);
    }

    [Fact]
    public void BothCrossingBoundary_WithDifferentIntervals() {
        // |...0....|...1....|...2....|...3....|...4....|...5....|
        // |......##|####....|......##|####....|......##|####....|
        // |........|.......#|#.......|........|.......#|#.......|
        var other = new Schedule(_today, _today.AddDays(4), _fourOClock, _threeOClock);
        other.UpdateRecurrence(interval: 2);
        var s = new Schedule(_tomorrow, _today.AddDays(4), _fiveOClock, _twoOClock);
        s.UpdateRecurrence(interval: 3);
        
        var overlaps = s.OverlapScheduleWith(other);
        Assert.Single(overlaps);
        Assert.True(overlaps[0].CrossesDayBoundary);
        Assert.Equal(_today.AddDays(4), overlaps[0].StartDate);
        Assert.Equal(_today.AddDays(4), overlaps[0].EndDate);
        Assert.Equal(_fiveOClock, overlaps[0].StartTime);
        Assert.Equal(_twoOClock, overlaps[0].EndTime);
    }

    [Fact]
    public void WhenOneSchedules_BeforeMidNightRangeOverlaps_WithTheOtherSchedulesAfterMidnightRange() {
        // |...0....|...1....|...2....|...3....|...4....|...5....|
        // |..######|#.......|..######|#.......|..######|#.......|
        // |........|.......#|###.....|.......#|###.....|........|

        var s = new Schedule(_today, _today.AddDays(4), _sixOClock, _twoOClock);
        s.UpdateRecurrence(interval: 2);

        var other = new Schedule(_tomorrow, _today.AddDays(4), _nineOClock, _eightOClock);
        other.UpdateRecurrence(interval: 2);

        var overlaps = s.OverlapScheduleWith(other);
        Assert.Single(overlaps);
        Assert.False(overlaps[0].CrossesDayBoundary);
        Assert.Equal(_today.AddDays(2), overlaps[0].StartDate);
        Assert.Equal(_today.AddDays(4), overlaps[0].EndDate);
        Assert.Equal(_sixOClock, overlaps[0].StartTime);
        Assert.Equal(_eightOClock, overlaps[0].EndTime);
    }
    
    # endregion
}