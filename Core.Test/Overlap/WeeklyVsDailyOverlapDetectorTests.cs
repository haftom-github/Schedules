using Core.Entities;
using Core.Overlap;

namespace Core.Test.Overlap;

public class WeeklyVsDailyOverlapDetectorTests {
    private readonly WeeklyVsDailyOverlapDetector _overlapDetector = new();
    private readonly DateOnly _today = DateOnly.FromDateTime(DateTime.UtcNow);

    private readonly TimeOnly _commonStartTime = new(0, 0);
    private readonly TimeOnly _commonEndTime = new(1, 0);
    
    //no overlap cases
    [Fact]
    public void Detect_ShouldThrowAnException_WhenNullArgumentProvided() {
        var schedule1 = new Schedule(new TimeOnly(0, 0), new TimeOnly(1, 0), _today);
        schedule1.RecurWeekly([DayOfWeek.Monday]);
        Assert.Throws<ArgumentNullException>(() => _overlapDetector.Detect(null, schedule1));
        Assert.Throws<ArgumentNullException>(() => _overlapDetector.Detect(schedule1, null));
        Assert.Throws<ArgumentNullException>(() => _overlapDetector.Detect(null, null));
    }
    
    [Fact]
    public void Detect_ShouldThrowArgumentException_WhenTheCombinationIsNotWeeklyAndDaily() {
        var schedule1 = new Schedule(_commonStartTime, _commonEndTime, _today);
        var schedule2 = new Schedule(_commonStartTime, _commonEndTime, _today);
        // schedule2.RecurWeekly([DayOfWeek.Monday]);
        Assert.Throws<ArgumentException>(() => _overlapDetector.Detect(schedule1, schedule2));
        schedule2.RecurWeekly([DayOfWeek.Friday], 2);
        schedule1.RecurWeekly([DayOfWeek.Friday], 2);
        Assert.Throws<ArgumentException>(() => _overlapDetector.Detect(schedule1, schedule2));
    }

    [Fact]
    public void Detect_ShouldReturnNull_TimeDoesNotOverlap() {
        List<DayOfWeek> commonDays = [DayOfWeek.Monday, DayOfWeek.Friday];

        var s1 = new Schedule(_commonStartTime, _commonEndTime, _today, _today.AddDays(10));
        s1.RecurWeekly(commonDays);
        
        var s2 = new Schedule(_commonEndTime, _commonEndTime.AddMinutes(20), _today, _today.AddDays(10));

        var result = _overlapDetector.Detect(s1, s2);
        Assert.Null(result);
    }

    [Fact]
    public void Detect_OverlapShouldBeTheWeeklySequenceItSelf_WhenTheDailyScheduleHasOneDayInterval() {
        var startDate1 = _today.AddDays(0);
        var endDate1 = _today.AddDays(100);
        
        var startDate2 = _today.AddDays(0);
        var endDate2 = _today.AddDays(100);

        List<DayOfWeek> days1 = [startDate1.AddDays(1).DayOfWeek,];

        var s1 = new Schedule(_commonStartTime, _commonEndTime, startDate1, endDate1);
        s1.RecurWeekly(days1, 10);
        
        var s2 = new Schedule(_commonStartTime, _commonEndTime, startDate2, endDate2);

        var result = _overlapDetector.Detect(s1, s2);
        Assert.NotNull(result);
        Assert.Equal(s1.StartDate.DayNumber + 1, result.Start);
        Assert.Equal(s1.StartDate.DayNumber + 1 + 70, result.End);
        Assert.Equal(2, result.Length);
    }
}