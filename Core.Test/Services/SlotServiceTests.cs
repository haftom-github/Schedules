using Core.Entities;
using Core.Services;
using TimeOnly = System.TimeOnly;

namespace Core.Test.Services;

public class SlotServiceTests {

    private readonly DateOnly _today;
    private readonly TimeOnly _2OClock;
    private readonly TimeOnly _11OClock;
    private readonly DateOnly _lastMonth;
    private readonly WorkSchedule _officeHours;
    private readonly WorkSchedule _nightShift;
    private readonly BlockedSchedule _weekend;
    private readonly BlockedSchedule _lunchBreak;
    private readonly BlockedSchedule _coffeeBreak;
    private readonly TimeSpan _slotSpan;

    public SlotServiceTests() {
        _today = new DateOnly(2025, 08, 16);
        _2OClock = new TimeOnly(5, 0);
        _11OClock = new TimeOnly(14, 0);
        var oClockMorning = new TimeOnly(1, 0);
        _lastMonth = _today.AddDays(-30);
        _officeHours = new WorkSchedule(_2OClock, _11OClock, _lastMonth);
        _nightShift = new WorkSchedule(_11OClock, oClockMorning, _lastMonth);
        _weekend = new BlockedSchedule(_lastMonth, null, null);
        _weekend.RecurWeekly([DayOfWeek.Sunday, DayOfWeek.Saturday]);
        var luchStart = new TimeOnly(10, 0);
        var lunchEnd = new TimeOnly(11, 0);
        _lunchBreak = new BlockedSchedule(_lastMonth, luchStart, lunchEnd);
        var coffeeStart = new TimeOnly(6, 0);
        var coffeeEnd = new TimeOnly(6, 30);
        _coffeeBreak = new BlockedSchedule(_lastMonth, coffeeStart, coffeeEnd);
        _slotSpan = TimeSpan.FromMinutes(30);
    }
    
    [Fact]
    public void Generate_ShouldReturnEmptyList_WhenDayIsCompletelyBlocked() {
        var availableSlots = 
            SlotService.Generate(_slotSpan, _today, [_officeHours], [_weekend]);
        
        Assert.Empty(availableSlots);
    }

    [Fact]
    public void Generate_ShouldHaveNoEffect_WhenAddingTwoOverlappingBlockingTimes() {
        var slots = SlotService.Generate(_slotSpan, _today, [_officeHours], [_weekend]);
        var slots2 = SlotService.Generate(_slotSpan, _today, [_officeHours], [_weekend, _lunchBreak]);
        
        Assert.Equal(slots2.Count, slots.Count);
        Assert.Empty(slots);
    }

    [Fact]
    public void Generate_WhenScheduleCrossesBoundary() {
        _nightShift.UpdateRecurrenceInterval(10);
        var slots = SlotService.Generate(_slotSpan, _today.AddDays(1), [_nightShift], []);
        
        Assert.NotEmpty(slots);
    }

    [Fact]
    public void Generate_WhenMoreThanOneSchedule() {
        var schedule1 = new WorkSchedule(_2OClock, _11OClock, _lastMonth);
        schedule1.RecurWeekly([DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday]);

        var saturdays = new WorkSchedule(_2OClock, new TimeOnly(9, 0), _lastMonth);
        saturdays.RecurWeekly([DayOfWeek.Saturday]);

        var slots = SlotService.Generate(_slotSpan, _today, [schedule1, saturdays], [_coffeeBreak, _lunchBreak]);

        Assert.NotEmpty(slots);
    }
}