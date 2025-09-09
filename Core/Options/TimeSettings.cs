namespace Core.Options;

public static class TimeSettings {
    public static DateTime Now => DateTime.UtcNow;
    public static DateOnly Today => DateOnly.FromDateTime(Now);
    
    public static DayOfWeek FirstDayOfWeek => DayOfWeek.Monday;

    public static DateOnly ToFirstDayOfWeek(this DateOnly date) {
        var firstDayOfWeek = date.AddDays(0);
        while (firstDayOfWeek.DayOfWeek != FirstDayOfWeek)
            firstDayOfWeek = firstDayOfWeek.AddDays(-1);

        return firstDayOfWeek;
    }

    public static DayOfWeek ToNextDayOfWeek(this DayOfWeek day) {
        return (DayOfWeek)(((int)day + 1) % 7);
    }
}