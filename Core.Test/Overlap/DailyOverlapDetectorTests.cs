using Core.Entities;
using Core.Options;
using Core.Overlap;

namespace Core.Test.Overlap;

public class DailyOverlapDetectorTests {
    private readonly DailyOverlapDetector _overlapDetector = new DailyOverlapDetector();
    private readonly DateOnly _today = TimeSettings.Today;
    
    //no overlap cases
    [Fact]
    public void Detect_ShouldThrowAnException_WhenNullArgumentProvided() {
        var schedule1 = new Schedule(new TimeOnly(0, 0), new TimeOnly(1, 0), _today);
        Assert.Throws<ArgumentNullException>(() => _overlapDetector.Detect(null, schedule1));
        Assert.Throws<ArgumentNullException>(() => _overlapDetector.Detect(schedule1, null));
        Assert.Throws<ArgumentNullException>(() => _overlapDetector.Detect(null, null));
    }

    [Fact]
    public void Detect_ShouldReturnNoOverlap_WhenNonOverlappingTimeRangeDetected() {
        var schedule1 = new Schedule(new TimeOnly(0, 0), new TimeOnly(1, 0), _today);
        var schedule2 = new Schedule(new  TimeOnly(1, 0), new TimeOnly(2, 0), _today);
        
        var result = _overlapDetector.Detect(schedule1, schedule2);
        Assert.Null(result);
    }
    
    [Fact]
    public void Detect_ShouldReturnNoOverlap_WhenNonOverlappingDateRangeDetected() {
        var startTime1 = new TimeOnly(0, 0);
        var endTime1 = new TimeOnly(3, 0);
        
        var startTime2 = new TimeOnly(1, 0);
        var endTime2 = new TimeOnly(4, 0);
        
        var schedule1 = new Schedule(startTime1, endTime1, _today, _today.AddDays(10));
        var schedule2 = new Schedule(startTime2, endTime2, _today.AddDays(11));
        
        var result = _overlapDetector.Detect(schedule1, schedule2);
        Assert.Null(result);
        
        schedule1 = new Schedule(startTime1, endTime1, _today, _today.AddDays(11));
        schedule1.UpdateRecurrenceInterval(3);
        schedule2 = new Schedule(startTime2, endTime2, _today.AddDays(10), _today.AddDays(20));
        
        result = _overlapDetector.Detect(schedule1, schedule2);
        Assert.Null(result);
    }

    [Fact]
    public void Detect_ShouldCalculateAccurately() {
        var startTime = new TimeOnly(0, 0);
        var endTime = new TimeOnly(3, 0);
        
        var schedule1 = new Schedule(startTime, endTime, _today, _today.AddDays(30));
        schedule1.UpdateRecurrenceInterval(3);
        
        var schedule2 = new Schedule(startTime, endTime, _today, _today.AddDays(30));
        schedule2.UpdateRecurrenceInterval(5);
        
        var result = _overlapDetector.Detect(schedule1, schedule2);
        Assert.NotNull(result);
        Assert.True(result.IsFinite);
        Assert.Equal(3, result.Length);
        Assert.Equal(_today.DayNumber, result.Start);
        Assert.Equal(_today.AddDays(30).DayNumber, result.End);
    }
    
    [Fact]
    public void Detect_ShouldReturnFiniteOverlaps_DifferentIntervals()
    {
        var s1 = new Schedule(new TimeOnly(8, 0), new TimeOnly(9, 0), _today, _today.AddDays(40));
        s1.UpdateRecurrenceInterval(6);

        var s2 = new Schedule(new TimeOnly(8, 0), new TimeOnly(9, 0), _today.AddDays(2), _today.AddDays(40));
        s2.UpdateRecurrenceInterval(8);

        var result = _overlapDetector.Detect(s1, s2);
        
        Assert.NotNull(result);
        Assert.True(result.Length > 0);
    }
    
    [Fact]
    public void Detect_ShouldFindLateFirstOverlap()
    {
        var s1 = new Schedule(new TimeOnly(8, 0), new TimeOnly(9, 0), _today, _today.AddDays(100));
        s1.UpdateRecurrenceInterval(7); // weekly

        var s2 = new Schedule(new TimeOnly(8, 0), new TimeOnly(9, 0), _today.AddDays(5), _today.AddDays(100));
        s2.UpdateRecurrenceInterval(13);

        var result = _overlapDetector.Detect(s1, s2);

        Assert.NotNull(result);
        Assert.True(result.Start > _today.DayNumber);
    }
    
    [Fact]
    public void Detect_ShouldFindOverlapExactlyOnEndDate()
    {
        var start = _today;
        var end = _today.AddDays(30);

        var s1 = new Schedule(new TimeOnly(8, 0), new TimeOnly(9, 0), start, end);
        s1.UpdateRecurrenceInterval(5);

        var s2 = new Schedule(new TimeOnly(8, 0), new TimeOnly(9, 0), start.AddDays(5), end);
        s2.UpdateRecurrenceInterval(25);

        var result = _overlapDetector.Detect(s1, s2);

        Assert.NotNull(result);
        Assert.Equal(end.DayNumber, result.End);
    }
    
    [Fact]
    public void Detect_ShouldNotMarkInfinite_WhenOneScheduleEnds()
    {
        var s1 = new Schedule(new TimeOnly(8, 0), new TimeOnly(9, 0), _today, _today.AddDays(10));
        s1.UpdateRecurrenceInterval(1);

        var s2 = new Schedule(new TimeOnly(8, 0), new TimeOnly(9, 0), _today);
        s2.UpdateRecurrenceInterval(1);

        var result = _overlapDetector.Detect(s1, s2);

        Assert.NotNull(result);
        Assert.Equal(11, result.Length); // finite
    }
    
    [Fact]
    public void Detect_ShouldFindVeryLateFirstOverlap()
    {
        var s1 = new Schedule(new TimeOnly(8, 0), new TimeOnly(9, 0), _today, _today.AddDays(500));
        s1.UpdateRecurrenceInterval(37); // prime-ish

        var s2 = new Schedule(new TimeOnly(8, 0), new TimeOnly(9, 0), _today.AddDays(5), _today.AddDays(500));
        s2.UpdateRecurrenceInterval(41);

        var result = _overlapDetector.Detect(s1, s2);

        Assert.NotNull(result);
        Assert.True(result.Start > _today.DayNumber + 200); // first hit far in future
    }
    
    // Mid night crossing schedules

    [Fact]
    public void Detect_ShouldDetectOverlapsThatHappenAfterMidNight() {
        var startTime1 = new TimeOnly(22, 0);
        var endTime1 = new TimeOnly(5, 0);
        var startTime2 = new TimeOnly(3, 0);
        var endTime2 = new TimeOnly(4, 0);
        var s1 = new Schedule(startTime1, endTime1, _today, _today.AddDays(1));
        s1.UpdateRecurrenceInterval(2);
        
        var s2 = new Schedule(startTime2, endTime2, _today.AddDays(1), _today.AddDays(1));
        s2.UpdateRecurrenceInterval(2);
        
        var result = _overlapDetector.Detect(s1, s2);
        Assert.NotNull(result);
    }

    [Fact]
    public void Detect_ShouldDetect_BothSchedulesCrossMidNight() {
        var startTime1 = new TimeOnly(22, 0);
        var endTime1 = new TimeOnly(5, 0);
        var startTime2 = new TimeOnly(4, 0);
        var endTime2 = new TimeOnly(1, 0);
        var s1 = new Schedule(startTime1, endTime1, _today, _today.AddDays(2));
        s1.UpdateRecurrenceInterval(2);
        
        var s2 = new Schedule(startTime2, endTime2, _today.AddDays(1), _today.AddDays(3));
        s2.UpdateRecurrenceInterval(2);
        
        var result = _overlapDetector.Detect(s1, s2);
        Assert.NotNull(result);
        Assert.Equal(2, result.Length);
    }
}