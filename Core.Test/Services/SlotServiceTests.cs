using Core.Services;
using TimeOnly = System.TimeOnly;

namespace Core.Test.Services;

public class SlotServiceTests {

    private readonly DateOnly _today;
    private readonly TimeOnly _2OClock;
    private readonly TimeOnly _11OClock;
    private readonly DateOnly _lastMonth;
    private readonly Schedule _officeHours;
    private readonly Schedule _nightShift;
    private readonly Schedule _weekend;
    private readonly Schedule _lunchBreak;
    private readonly Schedule _coffeeBreak;
    private readonly TimeSpan _slotSpan;

    public SlotServiceTests() {
        _today = new DateOnly(2025, 08, 16);
        _2OClock = new TimeOnly(5, 0);
        _11OClock = new TimeOnly(14, 0);
        var oneOClock = new TimeOnly(1, 0);
        _lastMonth = _today.AddDays(-30);
        _officeHours = new Schedule(_lastMonth, startTime:_2OClock, endTime:_11OClock);
        _nightShift = new Schedule(_lastMonth, startTime:_11OClock, endTime:oneOClock);
        _weekend = new Schedule(_lastMonth);
        _weekend.UpdateRecurrence(RecurrenceType.Weekly, daysOfWeek: [DayOfWeek.Sunday, DayOfWeek.Saturday]);
        var lunchStart = new TimeOnly(10, 0);
        var lunchEnd = new TimeOnly(11, 0);
        _lunchBreak = new Schedule(_lastMonth, startTime: lunchStart, endTime: lunchEnd);
        var coffeeStart = new TimeOnly(6, 0);
        var coffeeEnd = new TimeOnly(6, 30);
        _coffeeBreak = new Schedule(_lastMonth, startTime: coffeeStart, endTime: coffeeEnd);
        _slotSpan = TimeSpan.FromMinutes(30);
    }
    
    [Fact]
    public void Generate_ShouldReturnEmptyList_WhenDayIsCompletelyBlocked() {
        var availableSlots = 
            SlotService.GenerateSlots(_slotSpan, _today, [_officeHours], [_weekend]);
        
        Assert.Empty(availableSlots);
    }

    [Fact]
    public void Generate_ShouldHaveNoEffect_WhenAddingTwoOverlappingBlockingTimes() {
        var slots = SlotService.GenerateSlots(_slotSpan, _today, [_officeHours], [_weekend]);
        var slots2 = SlotService.GenerateSlots(_slotSpan, _today, [_officeHours], [_weekend, _lunchBreak]);
        
        Assert.Equal(slots2.Count, slots.Count);
        Assert.Empty(slots);
    }

    [Fact]
    public void Generate_WhenScheduleCrossesBoundary() {
        _nightShift.UpdateRecurrence(interval:10);
        var slots = SlotService.GenerateSlots(_slotSpan, _today.AddDays(1), [_nightShift], []);
        
        Assert.NotEmpty(slots);
    }

    [Fact]
    public void Generate_WhenMoreThanOneSchedule() {
        var schedule1 = new Schedule(_lastMonth, startTime: _2OClock, endTime: _11OClock);
        schedule1.UpdateRecurrence(RecurrenceType.Weekly, daysOfWeek: [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday]);

        var saturdays = new Schedule( _lastMonth, startTime: _2OClock, endTime: new TimeOnly(9, 0));
        saturdays.UpdateRecurrence(RecurrenceType.Weekly, daysOfWeek:[DayOfWeek.Saturday]);

        var slots = SlotService.GenerateSlots(_slotSpan, _today, [schedule1, saturdays], [_coffeeBreak, _lunchBreak]);

        Assert.NotEmpty(slots);
    }
}