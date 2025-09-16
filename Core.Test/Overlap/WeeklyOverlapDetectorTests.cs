// using Core.Entities;
// using Core.Overlap;
//
// namespace Core.Test.Overlap;
//
// public class WeeklyOverlapDetectorTests {
//     private readonly WeeklyOverlapDetector _overlapDetector = new WeeklyOverlapDetector();
//     private readonly DateOnly _today = new DateOnly(2025, 8, 18);
//
//     private readonly TimeOnly _commonStartTime = new(0, 0);
//     private readonly TimeOnly _commonEndTime = new(1, 0);
//     
//     //no overlap cases
//     [Fact]
//     public void Detect_ShouldThrowAnException_WhenNullArgumentProvided() {
//         var schedule1 = new Schedule(new TimeOnly(0, 0), new TimeOnly(1, 0), _today);
//         schedule1.RecurWeekly([DayOfWeek.Monday]);
//         Assert.Throws<ArgumentNullException>(() => _overlapDetector.Detect(null, schedule1));
//         Assert.Throws<ArgumentNullException>(() => _overlapDetector.Detect(schedule1, null));
//         Assert.Throws<ArgumentNullException>(() => _overlapDetector.Detect(null, null));
//     }
//     
//     [Fact]
//     public void Detect_ShouldThrowArgumentException_WhenOneOfTheSchedulesAreNotWeekly() {
//         var schedule1 = new Schedule(new TimeOnly(0, 0), new TimeOnly(1, 0), _today);
//         var schedule2 = new Schedule(new  TimeOnly(0, 0), new TimeOnly(1, 0), _today);
//         schedule2.RecurWeekly([DayOfWeek.Monday]);
//         Assert.Throws<ArgumentException>(() => _overlapDetector.Detect(schedule1, schedule2));
//     }
//
//     [Fact]
//     public void Detect_ShouldReturnNoOverlap_WhenNonOverlappingTimeRangeDetected() {
//         var schedule1 = new Schedule(new TimeOnly(0, 0), new TimeOnly(1, 0), _today);
//         var schedule2 = new Schedule(new  TimeOnly(1, 0), new TimeOnly(2, 0), _today);
//         
//         schedule1.RecurWeekly([DayOfWeek.Monday]);
//         schedule2.RecurWeekly([DayOfWeek.Monday]);
//         
//         var result = _overlapDetector.Detect(schedule1, schedule2);
//         Assert.Null(result);
//     }
//     
//     // boundary conditions
//
//     [Fact]
//     public void Detect_ShouldReturnNull_TimeDoesNotOverlap() {
//         List<DayOfWeek> commonDays = [DayOfWeek.Monday, DayOfWeek.Friday];
//
//         var s1 = new Schedule(_commonStartTime, _commonEndTime, _today, _today.AddDays(10));
//         s1.RecurWeekly(commonDays);
//         
//         var s2 = new Schedule(_commonEndTime, _commonEndTime.AddMinutes(20), _today, _today.AddDays(10));
//         s2.RecurWeekly(commonDays);
//
//         var result = _overlapDetector.Detect(s1, s2);
//         Assert.Null(result);
//     }
//     
//     [Fact]
//     public void Detect_ShouldReturnNull_WhenNoDaysInCommon() {
//         List<DayOfWeek> days1 = [DayOfWeek.Monday, DayOfWeek.Friday];
//         List<DayOfWeek> days2 = [DayOfWeek.Wednesday, DayOfWeek.Thursday];
//
//         var s1 = new Schedule(_commonStartTime, _commonEndTime, _today, _today.AddDays(10));
//         s1.RecurWeekly(days1);
//         
//         var s2 = new Schedule(_commonStartTime, _commonEndTime, _today, _today.AddDays(10));
//         s2.RecurWeekly(days2);
//
//         var result = _overlapDetector.Detect(s1, s2);
//         Assert.Null(result);
//     }
//
//     [Fact]
//     public void Detect_ShouldBehave_WhenBoundaryConditions() {
//         var startDate1 = _today.AddDays(4);
//         var endDate1 = _today.AddDays(100);
//         
//         var startDate2 = _today.AddDays(2);
//         var endDate2 = _today.AddDays(100);
//
//         List<DayOfWeek> days1 = [startDate1.AddDays(1).DayOfWeek];
//         List<DayOfWeek> days2 = [startDate1.AddDays(1).DayOfWeek];
//
//         var s1 = new Schedule(_commonStartTime, _commonEndTime, startDate1, endDate1);
//         s1.RecurWeekly(days1, 10);
//         
//         var s2 = new Schedule(_commonStartTime, _commonEndTime, startDate2, endDate2);
//         s2.RecurWeekly(days2, 10);
//
//         var result = _overlapDetector.Detect(s1, s2);
//         Assert.NotNull(result);
//     }
//
//     [Fact]
//     public void Detect_SpecialBoundaryConditions() {
//         var startDate1 = _today.AddDays(1);
//         var endDate1 = _today.AddDays(4);
//         var schedule1 = new Schedule(_commonStartTime, _commonEndTime, startDate1, endDate1);
//         schedule1.RecurWeekly([DayOfWeek.Monday, DayOfWeek.Tuesday]);
//         
//         var schedule2 = new Schedule(_commonStartTime, _commonEndTime, startDate1);
//         schedule2.RecurWeekly([DayOfWeek.Monday, DayOfWeek.Tuesday]);
//         
//         var result = _overlapDetector.Detect(schedule1, schedule2);
//         
//         Assert.NotNull(result);
//     }
// }