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
        var s = new Schedule(
            startDate:_today, 
            recurrence: Recurrence.Weekly([DayOfWeek.Monday]));

        Assert.Equal(RecurrenceType.Weekly, s.Recurrence.Type);
        Assert.Single(s.Recurrence.DaysOfWeek);
        Assert.Contains(DayOfWeek.Monday, s.Recurrence.DaysOfWeek);

        var s2 = s with { Recurrence = Recurrence.Weekly([], 5) };
        Assert.Equal(5, s2.Recurrence.Interval);
        Assert.Empty(s2.Recurrence.DaysOfWeek);
        Assert.Equal(RecurrenceType.Weekly, s2.Recurrence.Type);
    }
    
    
    // these tests assume first day of week is monday
    [Fact]
    public void ShouldHave_ZeroSlots_WhenDayOfWeekIsNotOnSchedule() 
    {
        var s = new Schedule(_today, recurrence:Recurrence.Weekly([_today.DayOfWeek]));
        Assert.Empty(s.SlotsAtDate(_tomorrow)); 
    }

    [Fact]
    public void TheLastDayWithInSchedule_ShouldHaveASlot() {
        var s = new Schedule(
            startDate:_today, 
            endDate:_today.AddDays(8),
            recurrence:Recurrence.Weekly([_today.DayOfWeek]));

        Assert.Single(s.SlotsAtDate(_today.AddDays(7)));
    }
    
    // general case
    [Fact]
    public void ADayWithInSchedule_ShouldHaveASlot_ConsecutiveDaysStart() {
        var s = new Schedule(
            startDate:_today, 
            endDate:_today.AddDays(15), 
            startTime:_twoOClock, 
            endTime:_fiveOClock,
            recurrence:Recurrence.Weekly([_today.DayOfWeek, _tomorrow.DayOfWeek]));
        
        var slots = s.SlotsAtDate(_today.AddDays(7));
        
        Assert.Single(slots);
        Assert.Equal(_twoOClock, slots[0].StartTime);
        Assert.Equal(_fiveOClock, slots[0].EndTime);
    }

    [Fact]
    public void ADayWithInSchedule_ShouldHaveASlot_ConsecutiveDaysEnd() {
        var s = new Schedule(_today, _today.AddDays(15), _twoOClock, _fiveOClock);
        
        var slots = s.SlotsAtDate(_today.AddDays(8));
        
        Assert.Single(slots);
        Assert.Equal(_twoOClock, slots[0].StartTime);
        Assert.Equal(_fiveOClock, slots[0].EndTime);
    }

    #region OverlapDetection

    [Fact]
    public void TheOverlap_ShouldBeWeeklyRecurring_WhenBothHaveWeeklyRecurrence() {
        
        // |..MON...|..TUE...|..WED...|..THU...|..FRI...|..SAT...|..SUN...|..MON...|
        // |..####..|........|........|........|........|........|........|..####..|
        // |....###.|........|........|........|........|........|........|....###.|
        
        var s = new Schedule(
            startDate:_today, 
            endDate:_today.AddDays(7), 
            startTime:_twoOClock, 
            endTime:_fiveOClock,
            recurrence:Recurrence.Weekly([_today.DayOfWeek]));
        
        var other = new Schedule(
            startDate:_today, 
            endDate:_today.AddDays(8), 
            startTime:_fourOClock, 
            endTime:_fiveOClock,
            recurrence:Recurrence.Weekly([DayOfWeek.Monday]));
        
        var overlaps = s.OverlapScheduleWith(other);
        Assert.Single(overlaps);
        Assert.Equal(RecurrenceType.Weekly, overlaps[0].Recurrence.Type);
    }

    [Fact]
    public void When_MoreThanOneDaysOfWeek_ShouldContainAllTheCommonDays() {
        
        // |..MON...|..TUE...|..WED...|..THU...|..FRI...|..SAT...|..SUN...|..MON...|..TUE...|..WED...|..THU...|..FRI...|..SAT...|..SUN...|
        // |..####..|..####..|..####..|........|........|........|........|..####..|..####..|..####..|........|........|........|........|
        // |....###.|........|....###.|....###.|........|........|........|....###.|........|....###.|....###.|........|........|........|
        
        var s = new Schedule(
            startDate:_today, 
            startTime:_twoOClock, 
            endTime:_fiveOClock,
            recurrence: Recurrence.Weekly([DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday]));
        
        var other = new Schedule(
            startDate:_today, 
            startTime:_fourOClock, 
            endTime:_fiveOClock,
            recurrence: Recurrence.Weekly([DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Thursday])
            );
        
        var overlaps = s.OverlapScheduleWith(other);
        Assert.Single(overlaps);
        Assert.Equal(RecurrenceType.Weekly, overlaps[0].Recurrence.Type);
        Assert.Equal(2, overlaps[0].Recurrence.DaysOfWeek.Count);
        Assert.Contains(DayOfWeek.Monday, overlaps[0].Recurrence.DaysOfWeek);
        Assert.Contains(DayOfWeek.Wednesday, overlaps[0].Recurrence.DaysOfWeek);
    }

    [Fact]
    public void When_DifferentIntervals() {
        // |..MON...|..TUE...|..MON...|..TUE...|..MON...|..TUE...|..MON...|..TUE...|..MON...|..TUE...|..MON...|..TUE...|..MON...|..TUE...|
        // |..####..|..####..|........|........|..####..|..####..|........|........|..####..|..####..|........|........|..####..|..####..|
        // |....###.|........|........|........|........|........|....###.|........|........|........|........|........|....###.|........|

        var s = new Schedule(
            startDate:_today, 
            startTime: _twoOClock, 
            endTime: _fourOClock,
            recurrence: Recurrence.Weekly([DayOfWeek.Monday, DayOfWeek.Tuesday], 2));
        
        var other = new Schedule(
            startDate:_today, 
            startTime: _twoOClock, 
            endTime: _fiveOClock,
            recurrence:Recurrence.Weekly([DayOfWeek.Monday], 3));
        
        var overlaps = s.OverlapScheduleWith(other);
        Assert.Single(overlaps);
        Assert.Equal(RecurrenceType.Weekly, overlaps[0].Recurrence.Type);
        Assert.Single(overlaps[0].Recurrence.DaysOfWeek);
        Assert.Contains(DayOfWeek.Monday, overlaps[0].Recurrence.DaysOfWeek);
        Assert.Equal(6, overlaps[0].Recurrence.Interval);
    }

    #endregion
}