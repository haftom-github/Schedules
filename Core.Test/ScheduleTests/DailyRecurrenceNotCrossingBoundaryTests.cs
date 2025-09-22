using Core.Entities;

namespace Core.Test.ScheduleTests;

public class DailyRecurrenceNotCrossingBoundaryTests {
    private readonly DateOnly _today = new(2025, 9, 8);
    private readonly DateOnly _tomorrow;
    private readonly DateOnly _yesterday;

    private readonly TimeOnly _twoOClock = new(2, 0);
    private readonly TimeOnly _threeOClock = new(3, 0);
    private readonly TimeOnly _fourOClock = new(4, 0);

    public DailyRecurrenceNotCrossingBoundaryTests()
    {
        _tomorrow = _today.AddDays(1);
        _yesterday = _today.AddDays(-1);
    }

    [Fact]
    public void ShouldHaveNoSlots_WhenDateNotWithinSchedule()
    {
        var s = new Schedule(_today);
        Assert.Empty(s.SlotsAtDate(_yesterday));
        
        s.UpdateRecurrence(interval: 3);
        Assert.Empty(s.SlotsAtDate(_tomorrow));
    }

    [Fact]
    public void ShouldHave_SingleFullDaySlot_WhenDateWithInSchedule_ForFullDaySchedules()
    {
        var s = new Schedule(_today);
        
        var periods = s.SlotsAtDate(_today);
        
        Assert.Single(periods);
        Assert.True(periods[0].IsFullDay);
    }

    [Fact]
    public void StartAndEndOfSlot_ShouldEqualThoseSetInTheSchedule()
    {
        var s = new Schedule(_today, null, _twoOClock, _threeOClock);
        
        var periods = s.SlotsAtDate(_today);
        
        Assert.Equal(_twoOClock, periods[0].Start);
        Assert.Equal(_threeOClock, periods[0].End);
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

    #region OverlapDetection

    [Fact]
    public void ShouldNotOverlap_WhenDateRangesDoNotOverlap() {
        var s1 = new Schedule(_today, _tomorrow);
        var s2 = new Schedule(_tomorrow.AddDays(2), _tomorrow.AddDays(3));
        
        var overlap = s1.OverlapScheduleWith(s2);
        Assert.Null(overlap);
    }

    [Fact]
    public void ShouldOverlap_WhenDateRangesOverlap_AndRecursDaily() {
        var s1 = new Schedule(_today, _today);
        s1.UpdateRecurrence(interval: 1);
        
        var s2 = new Schedule(_today, _tomorrow);
        s2.UpdateRecurrence(interval: 1);
        
        var overlap = s1.OverlapScheduleWith(s2);
        Assert.NotNull(overlap);
    }

    [Fact]
    public void ShouldNotOverlap_WhenTimeRangesDoNotOverlap() {
        var s = new Schedule(_today, _today, _twoOClock, _threeOClock);
        var other = new Schedule(_today, _today, _threeOClock, _fourOClock);

        var overlap = s.OverlapScheduleWith(other);
        Assert.Null(overlap);
    }

    [Fact]
    public void TheOverlap_ShouldRecurOnIntervalOf_2Days_WhenTheFirstRecursDaily_AndTheOtherEveryTwoDays() {
        var s = new Schedule(_today, _today);
        var other = new Schedule(_today, _tomorrow);
        other.UpdateRecurrence(interval: 2);

        var overlap = s.OverlapScheduleWith(other);
        Assert.NotNull(overlap);
        Assert.Equal(2, overlap.RecurrenceInterval);
    }

    [Fact]
    public void Overlap_ShouldRecurEvery6Days_WhenEachRecurEvery_2_And_3_Days() {
        // *  .  *  .  *  .  *  .  *  .  *  .  *
        // #  .  .  #  .  .  #  .  .  #  .  .  #
        var s = new Schedule(_today, _today.AddDays(5));
        s.UpdateRecurrence(interval: 2);
        var other = new Schedule(_today, _today.AddDays(5));
        other.UpdateRecurrence(interval: 3);

        var overlap = s.OverlapScheduleWith(other);
        Assert.NotNull(overlap);
        Assert.Equal(6, overlap.RecurrenceInterval);
    }

    [Fact]
    public void Overlap_ShouldRecur_AtTheSameDay_WhenTheTwoSchedulesHave_TheSameRecurrence() {
        var s = new Schedule(_today, _today);
        s.UpdateRecurrence(interval: 2);
        var other = new Schedule(_today, _today);
        other.UpdateRecurrence(interval: 2);
        
        var overlap = s.OverlapScheduleWith(other);
        Assert.NotNull(overlap);
        Assert.Equal(2, overlap.RecurrenceInterval);
    }

    [Fact]
    public void OverlapsTimeRange_ShouldBe_TheOverlapOfTheTwoTimeRanges() {
        var s = new Schedule(_today, _today, _twoOClock, _fourOClock);
        var other = new Schedule(_today, _today, _threeOClock, _fourOClock);

        var overlap = s.OverlapScheduleWith(other);
        Assert.NotNull(overlap);
        Assert.Equal(_threeOClock, overlap.StartTime);
        Assert.Equal(_fourOClock, overlap.EndTime);
    }

    [Fact]
    public void TwoNeverEndingSchedules_ShouldHaveNeverEndingOverlap() {
        var s = new Schedule(_today);
        var other = new Schedule(_tomorrow);

        var overlap = s.OverlapScheduleWith(other);
        Assert.NotNull(overlap);
        Assert.True(overlap.IsForever);
    }

    #endregion
}