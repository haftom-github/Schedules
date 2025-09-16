using Core.Entities;

namespace Core.Overlap;

public static class OverlapDetectorFactory {
    public static IOverlapDetector Create(RecurrenceType rt1, RecurrenceType rt2) {
        return rt1 switch {
            RecurrenceType.Daily when rt2 == RecurrenceType.Weekly => new WeeklyVsDailyOverlapDetector(),
            RecurrenceType.Daily => new DailyOverlapDetector(),
            RecurrenceType.Weekly when rt2 == RecurrenceType.Daily => new  WeeklyVsDailyOverlapDetector(),
            RecurrenceType.Weekly => new WeeklyOverlapDetector(),
            _ => throw new ArgumentOutOfRangeException(nameof(rt1), rt1, null),
        };
    }
}