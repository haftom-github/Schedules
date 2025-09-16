using Core.Entities;

namespace Core.Test.ScheduleTests;

public class WeeklyRecurrenceTests {
    private readonly DateOnly _today = new(2025, 9, 8);
    private readonly DateOnly _tomorrow;

    private readonly TimeOnly _twoOClock = new(2, 0);
    private readonly TimeOnly _fiveOClock = new(5, 0);

    public WeeklyRecurrenceTests()
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
    public void ZeroSlots_WhenWeekly_AndDayNotInDaysOfWeek() 
    {
        var s = new Schedule(_today);
        
        s.UpdateRecurrence(RecurrenceType.Weekly, daysOfWeek: [_today.DayOfWeek]);
        Assert.Empty(s.SlotsAtDate(_tomorrow)); 
    }

    [Fact]
    public void TheLastDayWithInSchedule_ShouldHaveASlot_DoesNotCrossBoundary() {
        var s = new Schedule(_today, _today.AddDays(8));
        s.UpdateRecurrence(type: RecurrenceType.Weekly, daysOfWeek: [_today.DayOfWeek]);
        Assert.Single(s.SlotsAtDate(_today.AddDays(7)));
    }
    
    [Fact]
    public void TheLastDayWithInSchedule_ShouldHaveASlot_CrossesBoundary() {
        var s = new Schedule(_today, _today.AddDays(8), _fiveOClock, _twoOClock);
        s.UpdateRecurrence(type: RecurrenceType.Weekly, daysOfWeek: [_today.DayOfWeek]);
        
        var slots = s.SlotsAtDate(_today.AddDays(8));
        
        Assert.Single(slots);
        Assert.Equal(TimeOnly.MinValue, slots[0].Start);
        Assert.Equal(_twoOClock, slots[0].End);
    }

    [Fact]
    public void TheDayBeforeTheLastDayWithInSchedule_ShouldHaveTwoSlots_CrossesBoundary_AndConsecutiveScheduleDays() {
        var s = new Schedule(_today, _today.AddDays(8), _fiveOClock, _twoOClock);
        s.UpdateRecurrence(type: RecurrenceType.Weekly, daysOfWeek: [_today.DayOfWeek, _tomorrow.DayOfWeek]);
        
        var slots = s.SlotsAtDate(_today.AddDays(8));
        
        Assert.Equal(2, slots.Count);
        Assert.Equal(_fiveOClock, slots[0].Start);
        Assert.Equal(TimeOnly.MaxValue, slots[0].End);
        
        Assert.Equal(TimeOnly.MinValue, slots[1].Start);
        Assert.Equal(_twoOClock, slots[1].End);
    }
    
    // general case
    [Fact]
    public void ADayWithInSchedule_ShouldHaveASlot_DoesNotCrossBoundary_ConsecutiveDaysStart() {
        var s = new Schedule(_today, _today.AddDays(15), _twoOClock, _fiveOClock);
        s.UpdateRecurrence(RecurrenceType.Weekly, daysOfWeek: [_today.DayOfWeek, _tomorrow.DayOfWeek]);
        
        var slots = s.SlotsAtDate(_today.AddDays(7));
        
        Assert.Single(slots);
        Assert.Equal(_twoOClock, slots[0].Start);
        Assert.Equal(_fiveOClock, slots[0].End);
    }

    [Fact]
    public void ADayWithInSchedule_ShouldHaveASlot_DoesNotCrossBoundary_ConsecutiveDaysEnd() {
        var s = new Schedule(_today, _today.AddDays(15), _twoOClock, _fiveOClock);
        
        var slots = s.SlotsAtDate(_today.AddDays(8));
        
        Assert.Single(slots);
        Assert.Equal(_twoOClock, slots[0].Start);
        Assert.Equal(_fiveOClock, slots[0].End);
    }

    [Fact]
    public void ADayWithInSchedule_ShouldHaveASlot_CrossBoundary_ConsecutiveDaysStart() {
        var s = new Schedule(_today, _today.AddDays(15), _fiveOClock, _twoOClock);
        s.UpdateRecurrence(RecurrenceType.Weekly, daysOfWeek: [_today.DayOfWeek, _tomorrow.DayOfWeek]);
        
        var slots = s.SlotsAtDate(_today.AddDays(7));
        
        Assert.Single(slots);
        Assert.Equal(_fiveOClock, slots[0].Start);
        Assert.Equal(TimeOnly.MaxValue, slots[0].End);
    }

    [Fact]
    public void ADayWithInSchedule_ShouldHaveASlot_CrossBoundary_ConsecutiveDaysEnd() {
        var s = new Schedule(_today, _today.AddDays(15), _fiveOClock, _twoOClock);
        s.UpdateRecurrence(RecurrenceType.Weekly, daysOfWeek: [_today.DayOfWeek, _tomorrow.DayOfWeek]);
        
        var slots = s.SlotsAtDate(_today.AddDays(8));
        
        Assert.Equal(2, slots.Count);
        Assert.Equal(_fiveOClock, slots[0].Start);
        Assert.Equal(TimeOnly.MaxValue, slots[0].End);
        Assert.Equal(TimeOnly.MinValue, slots[1].Start);
        Assert.Equal(_twoOClock, slots[1].End);
    }

    [Fact]
    public void TheDayAfterTheLastDayInDaysOfWeek_ShouldHaveASlot_CrossBoundary() {
        var s = new Schedule(_today, _today.AddDays(15), _fiveOClock, _twoOClock);
        s.UpdateRecurrence(RecurrenceType.Weekly, daysOfWeek: [_today.DayOfWeek, _tomorrow.DayOfWeek]);
        
        var slots = s.SlotsAtDate(_today.AddDays(9));
        
        Assert.Single(slots);
        Assert.Equal(TimeOnly.MinValue, slots[0].Start);
        Assert.Equal(_twoOClock, slots[0].End);
    }
}