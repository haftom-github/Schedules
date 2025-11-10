using Core.Services;

namespace Core.Test.ScheduleTests;

public class WeeklyRecurrenceBoundaryCrossingTests {
    
    private readonly DateOnly _today = new(2025, 9, 8);
    private readonly DateOnly _tomorrow;

    private readonly TimeOnly _oneOClock = new(1, 0);
    private readonly TimeOnly _twoOClock = new(2, 0);
    private readonly TimeOnly _fourOClock = new(4, 0);
    private readonly TimeOnly _fiveOClock = new(5, 0);
    private readonly TimeOnly _elevenOClock = new(11, 0);

    public WeeklyRecurrenceBoundaryCrossingTests()
    {
        _tomorrow = _today.AddDays(1);
    }
    
    [Fact]
    public void TheLastDayWithInSchedule_ShouldHaveASlot() {
        var s = new Schedule(_today, _today.AddDays(8), _fiveOClock, _twoOClock);
        s.UpdateRecurrence(type: RecurrenceType.Weekly, daysOfWeek: [_today.DayOfWeek]);
        
        var slots = s.SlotsAtDate(_today.AddDays(8));
        
        Assert.Single(slots);
        Assert.Equal(TimeOnly.MinValue, slots[0].StartTime);
        Assert.Equal(_twoOClock, slots[0].EndTime);
    }
    
    [Fact]
    public void TheDayBeforeTheLastDayWithInSchedule_ShouldHaveTwoSlots_WhenConsecutiveScheduleDays() {
        var s = new Schedule(_today, _today.AddDays(8), _fiveOClock, _twoOClock);
        s.UpdateRecurrence(type: RecurrenceType.Weekly, daysOfWeek: [_today.DayOfWeek, _tomorrow.DayOfWeek]);
        
        var slots = s.SlotsAtDate(_today.AddDays(8));
        
        Assert.Equal(2, slots.Count);
        Assert.Equal(_fiveOClock, slots[0].StartTime);
        Assert.Equal(TimeOnly.MinValue, slots[0].EndTime);
        
        Assert.Equal(TimeOnly.MinValue, slots[1].StartTime);
        Assert.Equal(_twoOClock, slots[1].EndTime);
    }
    
    [Fact]
    public void ADayWithInSchedule_ShouldHaveASlot_WhenConsecutiveDaysStart() {
        var s = new Schedule(_today, _today.AddDays(15), _fiveOClock, _twoOClock);
        s.UpdateRecurrence(RecurrenceType.Weekly, daysOfWeek: [_today.DayOfWeek, _tomorrow.DayOfWeek]);
        
        var slots = s.SlotsAtDate(_today.AddDays(7));
        
        Assert.Single(slots);
        Assert.Equal(_fiveOClock, slots[0].StartTime);
        Assert.Equal(TimeOnly.MinValue, slots[0].EndTime);
    }
    
    [Fact]
    public void ADayWithInSchedule_ShouldHaveASlot_WhenItsConsecutiveDaysEnd() {
        var s = new Schedule(_today, _today.AddDays(15), _fiveOClock, _twoOClock);
        s.UpdateRecurrence(RecurrenceType.Weekly, daysOfWeek: [_today.DayOfWeek, _tomorrow.DayOfWeek]);
        
        var slots = s.SlotsAtDate(_today.AddDays(8));
        
        Assert.Equal(2, slots.Count);
        Assert.Equal(_fiveOClock, slots[0].StartTime);
        Assert.Equal(TimeOnly.MinValue, slots[0].EndTime);
        Assert.Equal(TimeOnly.MinValue, slots[1].StartTime);
        Assert.Equal(_twoOClock, slots[1].EndTime);
    }
    
    [Fact]
    public void TheDayAfterTheLastDayInDaysOfWeek_ShouldHaveASlot() {
        var s = new Schedule(_today, _today.AddDays(15), _fiveOClock, _twoOClock);
        s.UpdateRecurrence(RecurrenceType.Weekly, daysOfWeek: [_today.DayOfWeek, _tomorrow.DayOfWeek]);
        
        var slots = s.SlotsAtDate(_today.AddDays(9));
        
        Assert.Single(slots);
        Assert.Equal(TimeOnly.MinValue, slots[0].StartTime);
        Assert.Equal(_twoOClock, slots[0].EndTime);
    }

    #region OverlapDetection

    [Fact]
    public void ShouldCrossBoundary_WhenBothCrossBoundary() {
        // |..MON...|..TUE...|..WED...|..THU...|..FRI...|..SAT...|..SUN...|..MON...|..TUE...|..WED...|..THU...|..FRI...|..SAT...|..SUN...|
        // |.......#|####....|.......#|####....|........|........|........|.......#|####....|.......#|####....|........|........|........|
        // |.....###|#.......|.....###|#.......|........|........|........|.....###|#.......|.....###|#.......|........|........|........|

        var s = new Schedule(_today, startTime: _fiveOClock, endTime: _twoOClock);
        s.UpdateRecurrence(RecurrenceType.Weekly, daysOfWeek: [DayOfWeek.Monday, DayOfWeek.Wednesday]);
        
        var other = new Schedule(_today, startTime: _fourOClock, endTime: _oneOClock);
        other.UpdateRecurrence(RecurrenceType.Weekly, daysOfWeek: [DayOfWeek.Monday, DayOfWeek.Wednesday]);
        
        var overlaps = s.OverlapScheduleWith(other);
        Assert.Single(overlaps);
        Assert.True(overlaps[0].CrossesDayBoundary);
        Assert.Equal(RecurrenceType.Weekly, overlaps[0].RecurrenceType);
        Assert.Equal(2, overlaps[0].DaysOfWeek.Count);
    }
    
    [Fact]
    public void WhenThereAreMultipleOverlaps() {
        // |..MON...|..TUE...|..WED...|..THU...|..MON...|..TUE...|..WED...|..THU...|
        // |.......#|####...#|####....|........|.......#|####...#|####....|........|
        // |...#####|#..#####|#.......|........|...#####|#..#####|#.......|........|

        var s = new Schedule(_today, startTime: _elevenOClock, endTime: _fiveOClock);
        s.UpdateRecurrence(RecurrenceType.Weekly, daysOfWeek: [DayOfWeek.Monday, DayOfWeek.Tuesday]);
        
        var other = new Schedule(_today, startTime: _fourOClock, endTime: _oneOClock);
        other.UpdateRecurrence(RecurrenceType.Weekly, daysOfWeek: [DayOfWeek.Monday, DayOfWeek.Tuesday]);
        
        var overlaps = s.OverlapScheduleWith(other);
        Assert.Equal(2, overlaps.Count);
        Assert.True(overlaps[0].CrossesDayBoundary);
        Assert.Equal(RecurrenceType.Weekly, overlaps[0].RecurrenceType);
        Assert.Equal(2, overlaps[0].DaysOfWeek.Count);
        
        Assert.False(overlaps[1].CrossesDayBoundary);
        Assert.Equal(RecurrenceType.Weekly, overlaps[1].RecurrenceType);
        Assert.Single(overlaps[1].DaysOfWeek);
        Assert.Contains(DayOfWeek.Tuesday, overlaps[1].DaysOfWeek);
    }

    #endregion
}