using Core.Services;

namespace Core.Test.ScheduleTests;

public class WeeklyRecurrenceNotCrossingBoundaryTests {
    private readonly DateOnly _today = new(2025, 9, 8);
    private readonly DateOnly _tomorrow;

    private readonly TimeOnly _twoOClock = new(2, 0);
    private readonly TimeOnly _fourOClock = new(4, 0);
    private readonly TimeOnly _fiveOClock = new(5, 0);

    public WeeklyRecurrenceNotCrossingBoundaryTests()
    {
        _tomorrow = _today.AddDays(1);
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
    
    
    // these tests assume first day of week is monday
    [Fact]
    public void ShouldHave_ZeroSlots_WhenDayOfWeekIsNotOnSchedule() 
    {
        var s = new Schedule(_today);
        
        s.UpdateRecurrence(RecurrenceType.Weekly, daysOfWeek: [_today.DayOfWeek]);
        Assert.Empty(s.SlotsAtDate(_tomorrow)); 
    }

    [Fact]
    public void TheLastDayWithInSchedule_ShouldHaveASlot() {
        var s = new Schedule(_today, _today.AddDays(8));
        s.UpdateRecurrence(type: RecurrenceType.Weekly, daysOfWeek: [_today.DayOfWeek]);
        Assert.Single(s.SlotsAtDate(_today.AddDays(7)));
    }
    
    // general case
    [Fact]
    public void ADayWithInSchedule_ShouldHaveASlot_ConsecutiveDaysStart() {
        var s = new Schedule(_today, _today.AddDays(15), _twoOClock, _fiveOClock);
        s.UpdateRecurrence(RecurrenceType.Weekly, daysOfWeek: [_today.DayOfWeek, _tomorrow.DayOfWeek]);
        
        var slots = s.SlotsAtDate(_today.AddDays(7));
        
        Assert.Single(slots);
        Assert.Equal(_twoOClock, slots[0].Start);
        Assert.Equal(_fiveOClock, slots[0].End);
    }

    [Fact]
    public void ADayWithInSchedule_ShouldHaveASlot_ConsecutiveDaysEnd() {
        var s = new Schedule(_today, _today.AddDays(15), _twoOClock, _fiveOClock);
        
        var slots = s.SlotsAtDate(_today.AddDays(8));
        
        Assert.Single(slots);
        Assert.Equal(_twoOClock, slots[0].Start);
        Assert.Equal(_fiveOClock, slots[0].End);
    }

    #region OverlapDetection

    [Fact]
    public void TheOverlap_ShouldBeWeeklyRecurring_WhenBothHaveWeeklyRecurrence() {
        
        // |..MON...|..TUE...|..WED...|..THU...|..FRI...|..SAT...|..SUN...|..MON...|
        // |..####..|........|........|........|........|........|........|..####..|
        // |....###.|........|........|........|........|........|........|....###.|
        
        var s = new Schedule(_today, _today.AddDays(7), _twoOClock, _fiveOClock);
        s.UpdateRecurrence(RecurrenceType.Weekly, daysOfWeek: [_today.DayOfWeek]);
        
        var other = new Schedule(_today, _today.AddDays(8), _fourOClock, _fiveOClock);
        other.UpdateRecurrence(RecurrenceType.Weekly, daysOfWeek: [DayOfWeek.Monday]);
        
        var overlaps = s.OverlapScheduleWith(other);
        Assert.Single(overlaps);
        Assert.Equal(RecurrenceType.Weekly, overlaps[0].RecurrenceType);
    }

    [Fact]
    public void When_MoreThanOneDaysOfWeek_ShouldContainAllTheCommonDays() {
        
        // |..MON...|..TUE...|..WED...|..THU...|..FRI...|..SAT...|..SUN...|..MON...|..TUE...|..WED...|..THU...|..FRI...|..SAT...|..SUN...|
        // |..####..|..####..|..####..|........|........|........|........|..####..|..####..|..####..|........|........|........|........|
        // |....###.|........|....###.|....###.|........|........|........|....###.|........|....###.|....###.|........|........|........|
        
        var s = new Schedule(_today, startTime:_twoOClock, endTime:_fiveOClock);
        s.UpdateRecurrence(RecurrenceType.Weekly, daysOfWeek: [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday]);
        
        var other = new Schedule(_today, startTime:_fourOClock, endTime:_fiveOClock);
        other.UpdateRecurrence(RecurrenceType.Weekly, daysOfWeek: [DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Thursday]);
        
        var overlaps = s.OverlapScheduleWith(other);
        Assert.Single(overlaps);
        Assert.Equal(RecurrenceType.Weekly, overlaps[0].RecurrenceType);
        Assert.Equal(2, overlaps[0].DaysOfWeek.Count);
        Assert.Contains(DayOfWeek.Monday, overlaps[0].DaysOfWeek);
        Assert.Contains(DayOfWeek.Wednesday, overlaps[0].DaysOfWeek);
    }

    [Fact]
    public void When_DifferentIntervals() {
        // |..MON...|..TUE...|..MON...|..TUE...|..MON...|..TUE...|..MON...|..TUE...|..MON...|..TUE...|..MON...|..TUE...|..MON...|..TUE...|
        // |..####..|..####..|........|........|..####..|..####..|........|........|..####..|..####..|........|........|..####..|..####..|
        // |....###.|........|........|........|........|........|....###.|........|........|........|........|........|....###.|........|

        var s = new Schedule(_today, startTime: _twoOClock, endTime: _fourOClock);
        s.UpdateRecurrence(RecurrenceType.Weekly, daysOfWeek:[DayOfWeek.Monday, DayOfWeek.Tuesday], interval:2);
        
        var other = new Schedule(_today, startTime: _twoOClock, endTime: _fiveOClock);
        other.UpdateRecurrence(RecurrenceType.Weekly, daysOfWeek:[DayOfWeek.Monday], interval:3);
        
        var overlaps = s.OverlapScheduleWith(other);
        Assert.Single(overlaps);
        Assert.Equal(RecurrenceType.Weekly, overlaps[0].RecurrenceType);
        Assert.Single(overlaps[0].DaysOfWeek);
        Assert.Contains(DayOfWeek.Monday, overlaps[0].DaysOfWeek);
        Assert.Equal(6, overlaps[0].RecurrenceInterval);
    }

    #endregion
}