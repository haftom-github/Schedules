using Core.Entities;
using Core.Enums;
using Core.Options;

namespace Core.Test.Entities;

public class BlockedScheduleTests {
    private readonly DateOnly _date610 = new DateOnly(2024, 6, 10);
    private readonly DateOnly _date615 = new DateOnly(2024, 6, 15);
    private readonly DateOnly _date620 = new DateOnly(2024, 6, 20);
    private readonly DateOnly _date621 = new DateOnly(2024, 6, 21);
        
    // constructor tests
    [Fact]
    public void Constructor_ShouldInitializeProperties_WhenValidParametersAreProvided() {
        // Arrange
        var startTime = new TimeOnly(9, 0);
        var endTime = new TimeOnly(17, 0);
        var startDate = TimeSettings.Today;
        var endDate = TimeSettings.Today.AddDays(2);

        var blockedTime = new BlockedSchedule(_date610, null, null);
        
        // Assert
        Assert.Equal(TimeOnly.MinValue, blockedTime.StartTime);
        Assert.Equal(TimeOnly.MaxValue, blockedTime.EndTime);
        Assert.Null(blockedTime.EndDate);
        Assert.Equal(RecurrenceType.Daily, blockedTime.RecurrenceType);
        Assert.Empty(blockedTime.DaysOfWeek);
        Assert.Equal(1, blockedTime.RecurrenceInterval);

        // Act
        blockedTime = new BlockedSchedule(_date610, startTime, endTime);

        // Assert
        Assert.Equal(startTime, blockedTime.StartTime);
        Assert.Equal(endTime, blockedTime.EndTime);
        
        blockedTime = new BlockedSchedule(startDate, startTime, endTime, endDate);
        
        Assert.Equal(startDate, blockedTime.StartDate);
        Assert.Equal(endDate, blockedTime.EndDate);
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentExceptionWhenEndTimeComesAfterEndTime() {
        // Arrange
        var startTime = new TimeOnly(9, 0);
        var endTime = new TimeOnly(17, 0);
        var startDate = TimeSettings.Today.AddDays(1);
        var endDate = TimeSettings.Today;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new BlockedSchedule(startDate, startTime, endTime, endDate));
    }
    
    
    // RecurWeekly tests
    
    [Fact]
    public void RecurWeekly_ShouldRemoveDuplicateDays_WhenCalledWithDuplicateDays() {
        // Arrange
        var startTime = new TimeOnly(9, 0);
        var endTime = new TimeOnly(17, 0);
        var startDate = TimeSettings.Today.AddDays(1);
        var blockedTime = new BlockedSchedule(startDate, startTime, endTime);
        List<DayOfWeek> daysOfWeek = [DayOfWeek.Monday, DayOfWeek.Monday, DayOfWeek.Wednesday];

        // Act
        blockedTime.RecurWeekly(daysOfWeek);

        // Assert
        Assert.Equal(RecurrenceType.Weekly, blockedTime.RecurrenceType);
        Assert.Equal(daysOfWeek.ToHashSet(), blockedTime.DaysOfWeek);
    }
    
    [Fact]
    public void RecurWeekly_ShouldSetRecurrenceProperties_WhenCalledWithValidDaysAndInterval() {
        // Arrange
        var startTime = new TimeOnly(9, 0);
        var endTime = new TimeOnly(17, 0);
        var startDate = TimeSettings.Today.AddDays(1);
        var blockedTime = new BlockedSchedule(startDate, startTime, endTime);
        List<DayOfWeek> daysOfWeek = [DayOfWeek.Monday, DayOfWeek.Wednesday];

        // Act
        blockedTime.RecurWeekly(daysOfWeek, 2);

        // Assert
        Assert.Equal(RecurrenceType.Weekly, blockedTime.RecurrenceType);
        Assert.Equal(2, blockedTime.RecurrenceInterval);
        Assert.Equal(daysOfWeek.ToHashSet(), blockedTime.DaysOfWeek);
    }

    [Fact]
    public void RecurWeekly_ShouldThrowArgumentOutOfRangeException_WhenIntervalIsNotPositive() {
        // Arrange
        var startTime = new TimeOnly(9, 0);
        var endTime = new TimeOnly(17, 0);
        var startDate = TimeSettings.Today.AddDays(1);
        var blockedTime = new BlockedSchedule(startDate, startTime, endTime);
        
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => blockedTime.RecurWeekly([DayOfWeek.Monday], 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => blockedTime.RecurWeekly([DayOfWeek.Monday], -1));
    }

    [Fact]
    public void RecurWeekly_ShouldThrowArgumentException_WhenDaysOfWeekIsNullOrEmpty() {
        // Arrange
        var startTime = new TimeOnly(9, 0);
        var endTime = new TimeOnly(17, 0);
        var startDate = TimeSettings.Today.AddDays(1);
        var blockedTime = new BlockedSchedule(startDate, startTime, endTime);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => blockedTime.RecurWeekly(null!));
        Assert.Throws<ArgumentException>(() => blockedTime.RecurWeekly([]));
    }
    
    [Fact]
    public void UpdateRecurrenceInterval_ShouldUpdateInterval_WhenCalledWithValidInterval() {
        // Arrange
        var startTime = new TimeOnly(9, 0);
        var endTime = new TimeOnly(17, 0);
        var blockedTime = new BlockedSchedule(_date610, startTime, endTime);
        
        // Act
        blockedTime.UpdateRecurrenceInterval(3);
        
        // Assert
        Assert.Equal(3, blockedTime.RecurrenceInterval);
    }
    
    [Fact]
    public void UpdateRecurrenceInterval_ShouldThrowArgumentOutOfRangeException_WhenIntervalIsNotPositive() {
        // Arrange
        var startTime = new TimeOnly(9, 0);
        var endTime = new TimeOnly(17, 0);
        var startDate = TimeSettings.Today.AddDays(1);
        var blockedTime = new BlockedSchedule(startDate, startTime, endTime);
        
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => blockedTime.UpdateRecurrenceInterval(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => blockedTime.UpdateRecurrenceInterval(-1));
    }
    
    // RecurDaily tests
    [Fact]
    public void RecurDaily_ShouldSetRecurrenceProperties_WhenCalledWithValidInterval() {
        // Arrange
        var startTime = new TimeOnly(9, 0);
        var endTime = new TimeOnly(17, 0);
        var blockedTime = new BlockedSchedule(_date610, startTime, endTime);
        
        // Act
        blockedTime.RecurDaily(2);
        
        // Assert
        Assert.Equal(RecurrenceType.Daily, blockedTime.RecurrenceType);
        Assert.Equal(2, blockedTime.RecurrenceInterval);
    }

    [Fact]
    public void RecurDaily_ShouldThrowArgumentOutOfRangeException_WhenIntervalIsNotPositive() {
        // Arrange
        var startTime = new TimeOnly(9, 0);
        var endTime = new TimeOnly(17, 0);
        var blockedTime = new BlockedSchedule(_date610, startTime, endTime);
        
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => blockedTime.RecurDaily(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => blockedTime.RecurDaily(-1));
    }
    
    // IsWholeDayBlocked tests

    [Fact]
    public void IsWholeDayBlocked_ShouldReturnFalse_WhenDateIsBeforeStartDate() {
        // Arrange
        var startDate = TimeSettings.Today.AddDays(1);
        var endDate = TimeSettings.Today.AddDays(2);
        var blockedTime = new BlockedSchedule(startDate, null, null, endDate);

        // Act
        var isBlocked = blockedTime.IsWholeDayBlocked(TimeSettings.Today);

        // Assert
        Assert.False(isBlocked);
        
        // Arrange
        var startTime = new TimeOnly(9, 0);
        var endTime = new TimeOnly(17, 0);
        var blockedTimeWithTime = new BlockedSchedule(startDate, startTime, endTime, endDate);
        
        // Act
        isBlocked = blockedTimeWithTime.IsWholeDayBlocked(TimeSettings.Today);
        
        // Assert
        Assert.False(isBlocked);
    }

    [Fact]
    public void IsWholeDayBlocked_ShouldReturnFalse_WhenDateIsAfterEndDate() {
        // Arrange
        var startDate = TimeSettings.Today.AddDays(1);
        var endDate = TimeSettings.Today.AddDays(2);
        var blockedTime = new BlockedSchedule(startDate, null, null, endDate);

        // Act
        var isBlocked = blockedTime.IsWholeDayBlocked(TimeSettings.Today.AddDays(3));

        // Assert
        Assert.False(isBlocked);
        
        // Arrange
        var startTime = new TimeOnly(9, 0);
        var endTime = new TimeOnly(17, 0);
        var blockedTimeWithTime = new BlockedSchedule(startDate, startTime, endTime, endDate);
        
        // Act
        isBlocked = blockedTimeWithTime.IsWholeDayBlocked(TimeSettings.Today.AddDays(3));
        
        // Assert
        Assert.False(isBlocked);
    }
    
    [Fact]
    public void IsWholeDayBlocked_ShouldReturnFalse_WhenStartTimeAndEndTimeAreSet() {
        // Arrange
        var startDate = TimeSettings.Today.AddDays(1);
        var endDate = TimeSettings.Today.AddDays(2);
        var startTime = new TimeOnly(9, 0);
        var endTime = new TimeOnly(17, 0);
        var blockedTime = new BlockedSchedule(startDate, startTime, endTime, endDate);
        
        // Act
        var isBlocked = blockedTime.IsWholeDayBlocked(TimeSettings.Today.AddDays(1));
        
        // Assert
        Assert.False(isBlocked);
    }
    
    // IsWholeDayBlocked Daily recurrence tests
    [Fact]
    public void IsWholeDayBlocked_ShouldReturnTrue_WhenDateIsStartDate() {
        // Arrange
        var startDate = TimeSettings.Today.AddDays(1);
        var endDate = TimeSettings.Today.AddDays(2);
        var blockedTime = new BlockedSchedule(startDate, null, null, endDate);

        // Act
        var isBlocked = blockedTime.IsWholeDayBlocked(startDate);

        // Assert
        Assert.True(isBlocked);
    }
    
    [Fact]
    public void IsWholeDayBlocked_ShouldReturnTrue_WhenDateIsEndDate() {
        // Arrange
        var startDate = TimeSettings.Today.AddDays(1);
        var endDate = TimeSettings.Today.AddDays(2);
        var blockedTime = new BlockedSchedule(startDate: startDate, null, null, endDate: endDate);
        
        // Act
        var isBlocked = blockedTime.IsWholeDayBlocked(endDate);
        
        // Assert
        Assert.True(isBlocked);
    }
    
    [Fact]
    public void IsWholeDayBlocked_ShouldReturnTrue_WhenDateIsWithInBlockedTime() {
        // Arrange
        var startDate = TimeSettings.Today.AddDays(1);
        var endDate = TimeSettings.Today.AddDays(3);
        var blockedTime = new BlockedSchedule(startDate: startDate, null, null, endDate: endDate);

        // Act
        var isBlocked = blockedTime.IsWholeDayBlocked(TimeSettings.Today.AddDays(2));

        // Assert
        Assert.True(isBlocked);
    }

    [Fact]
    public void IsWholeDayBlocked_ShouldReturnTrue_WhenStartAndEndDatesNotSpecified() {
        // Arrange
        var blockedTime = new BlockedSchedule(_date610, null, null);

        // Act
        var isBlocked = blockedTime.IsWholeDayBlocked(TimeSettings.Today);

        // Assert
        Assert.True(isBlocked);
    }
    
    // IsWholeDayBlocked Weekly recurrence tests
    [Fact]
    public void IsWholeDayBlocked_ShouldReturnTrue_WhenDateIsStartDateWeekly() {
        // Arrange
        var startDate = TimeSettings.Today.AddDays(1);
        var endDate = TimeSettings.Today.AddDays(4);
        var blockedTime = new BlockedSchedule(startDate: startDate, null, null, endDate: endDate);
        blockedTime.RecurWeekly([startDate.DayOfWeek, endDate.DayOfWeek]);

        // Act
        var isBlocked = blockedTime.IsWholeDayBlocked(startDate);

        // Assert
        Assert.True(isBlocked);
    }

    [Fact]
    public void IsWholeDayBlocked_ShouldReturnTrue_WhenDateIsEndDateWeekly() {
        // Arrange
        var startDate = TimeSettings.Today.AddDays(1);
        var endDate = TimeSettings.Today.AddDays(4);
        var blockedTime = new BlockedSchedule(startDate: startDate, null, null, endDate: endDate);
        blockedTime.RecurWeekly([startDate.DayOfWeek, endDate.DayOfWeek]);

        // Act
        var isBlocked = blockedTime.IsWholeDayBlocked(endDate);

        // Assert
        Assert.True(isBlocked);
    }

    [Fact]
    public void IsWholeDayBlocked_ShouldReturnTrue_WhenDateIsWithInBlockedTimeWeekly() {
        // Arrange
        var startDate = TimeSettings.Today.AddDays(1);
        var date = TimeSettings.Today.AddDays(2);
        var endDate = TimeSettings.Today.AddDays(4);
        var blockedTime = new BlockedSchedule(startDate: startDate, null, null, endDate: endDate);
        blockedTime.RecurWeekly([date.DayOfWeek]);

        // Act
        var isBlocked = blockedTime.IsWholeDayBlocked(date);

        // Assert
        Assert.True(isBlocked);
    }

    [Fact]
    public void IsWholeDayBlocked_ShouldReturnFalse_WhenDateIsWithInRageButDayIsNotInDaysList() {
        // Arrange
        var startDate = TimeSettings.Today.AddDays(1);
        var date = TimeSettings.Today.AddDays(3);
        var endDate = TimeSettings.Today.AddDays(4);
        var blockedTime = new BlockedSchedule(startDate: startDate, null, null, endDate: endDate);
        blockedTime.RecurWeekly([startDate.DayOfWeek, endDate.DayOfWeek]);

        // Act
        var isBlocked = blockedTime.IsWholeDayBlocked(date);

        // Assert
        Assert.False(isBlocked);
    }

    [Fact]
    public void IsWholeDayBlocked_ShouldReturnTrue_WhenStartDateAndEndDateAreNotSpecifiedAndDayOfWeekMatches() {
        // Arrange
        var blockedTime = new BlockedSchedule(_date610, null, null);
        blockedTime.RecurWeekly([TimeSettings.Today.DayOfWeek]);

        // Act
        var isBlocked = blockedTime.IsWholeDayBlocked(TimeSettings.Today);

        // Assert
        Assert.True(isBlocked);
    }
    
    [Fact]
    public void IsWholeDayBlocked_ShouldReturnFalse_WhenStartDateAndEndDateAreNotSpecifiedButDayOfWeekDoesNotMatch() {
        // Arrange
        var blockedTime = new BlockedSchedule(_date610, null, null);
        var day1 = TimeSettings.Today;
        var day2 = day1.AddDays(1);
        var day3 = day1.AddDays(2);
        blockedTime.RecurWeekly([day1.DayOfWeek, day2.DayOfWeek]);

        // Act
        var isBlocked = blockedTime.IsWholeDayBlocked(day3);

        // Assert
        Assert.False(isBlocked);
    }
    
    // IsWholeDayBlocked tests with recurrence interval > 1

    [Fact]
    public void IsWholeDayBlocked_WhenDailyRecurrence() {
        
        // Arrange
        var blockedTime = new BlockedSchedule( _date610, null, null);
        blockedTime.RecurDaily(2);

        // Act & Assert
        Assert.True(blockedTime.IsWholeDayBlocked(_date610));
        Assert.False(blockedTime.IsWholeDayBlocked(_date610.AddDays(1)));
        Assert.True(blockedTime.IsWholeDayBlocked(_date610.AddDays(2)));
        Assert.False(blockedTime.IsWholeDayBlocked(_date610.AddDays(7)));
        Assert.True(blockedTime.IsWholeDayBlocked(_date610.AddDays(8)));
    }
    
    [Fact]
    public void IsWholeDayBlocked_WhenWeeklyRecurrence() {
        
        // Arrange
        var blockedTime = new BlockedSchedule( startDate: _date610, null, null);
        blockedTime.RecurWeekly([_date610.DayOfWeek], 2);

        // Act & Assert
        Assert.True(blockedTime.IsWholeDayBlocked(_date610));
        Assert.False(blockedTime.IsWholeDayBlocked(_date610.AddDays(7)));
        Assert.True(blockedTime.IsWholeDayBlocked(_date610.AddDays(14)));
        Assert.False(blockedTime.IsWholeDayBlocked(_date610.AddDays(49)));
        Assert.True(blockedTime.IsWholeDayBlocked(_date610.AddDays(56)));
    }

    // IsBlocked tests
    [Fact]
    public void IsBlocked_ReturnsFalse_WhenDateIsBeforeStartDate() {
        var dateToCheck = new DateOnly(2024, 6, 9);
        var startTimeToCheck = new TimeOnly(8, 0);
        var endTimeToCheck = new TimeOnly(9, 0);
        var blocked = new BlockedSchedule( _date610, null, null, _date620);
        Assert.False(blocked.IsBlocked(dateToCheck, startTimeToCheck, endTimeToCheck));
    }

    [Fact]
    public void IsBlocked_ReturnsFalse_WhenDateIsAfterEndDate() {
        var startTimeToCheck = new TimeOnly(8, 0);
        var endTimeToCheck = new TimeOnly(9, 0);
        var blocked = new BlockedSchedule( _date610, null, null, _date620);
        Assert.False(blocked.IsBlocked(_date621, startTimeToCheck, endTimeToCheck));
    }

    [Fact]
    public void IsBlocked_ReturnsFalse_WhenTimeDoesNotOverlap() {
        var startTime = new TimeOnly(10, 0);
        var endTime = new TimeOnly(12, 0);
        var startTimeToCheck = new TimeOnly(8, 0);
        var endTimeToCheck = new TimeOnly(9, 0);
        var blocked = new BlockedSchedule( _date615, startTime, endTime, _date615);
        Assert.False(blocked.IsBlocked(_date615, startTimeToCheck, endTimeToCheck));
    }

    [Fact]
    public void IsBlocked_ReturnsTrue_WhenTimeOverlaps() {
        var startTime = new TimeOnly(10, 0);
        var endTime = new TimeOnly(12, 0);
        var startTimeToCheck = new TimeOnly(11, 0);
        var endTimeToCheck = new TimeOnly(13, 0);
        var blocked = new BlockedSchedule( _date615, startTime, endTime, _date615);
        Assert.True(blocked.IsBlocked(_date615, startTimeToCheck, endTimeToCheck));
    }

    [Fact]
    public void IsBlocked_ReturnsTrue_ForDailyRecurrence() {
        var startTime = new TimeOnly(8, 0);
        var endTime = new TimeOnly(17, 0);
        var startTimeToCheck = new TimeOnly(9, 0);
        var endTimeToCheck = new TimeOnly(10, 0);
        var blocked = new BlockedSchedule( _date610, startTime, endTime, _date620);
        Assert.True(blocked.IsBlocked(_date615, startTimeToCheck, endTimeToCheck));
    }

    [Fact]
    public void IsBlocked_ReturnsFalse_ForWeeklyRecurrence_DayNotIncluded() {
        var startTime = new TimeOnly(8, 0);
        var endTime = new TimeOnly(17, 0);
        var startTimeToCheck = new TimeOnly(9, 0);
        var endTimeToCheck = new TimeOnly(10, 0);
        var blocked = new BlockedSchedule( _date610, startTime, endTime, _date620);
        blocked.RecurWeekly([DayOfWeek.Monday]);
        Assert.False(blocked.IsBlocked(new DateOnly(2024, 6, 12), startTimeToCheck, endTimeToCheck)); // Wednesday
    }

    [Fact]
    public void IsBlocked_ReturnsTrue_ForWeeklyRecurrence_DayIncluded() {
        var startTime = new TimeOnly(8, 0);
        var endTime = new TimeOnly(17, 0);
        var startTimeToCheck = new TimeOnly(9, 0);
        var endTimeToCheck = new TimeOnly(10, 0);
        var blocked = new BlockedSchedule( _date610, startTime, endTime, _date620);
        blocked.RecurWeekly([DayOfWeek.Wednesday]);
        Assert.True(blocked.IsBlocked(new DateOnly(2024, 6, 12), startTimeToCheck, endTimeToCheck)); // Wednesday
    }

    [Fact]
    public void IsBlocked_ThrowsArgumentException_WhenStartTimeIsAfterEndTime() {
        var startTime = new TimeOnly(8, 0);
        var endTime = new TimeOnly(17, 0);
        var startTimeToCheck = new TimeOnly(12, 0);
        var endTimeToCheck = new TimeOnly(10, 0);
        var blocked = new BlockedSchedule( _date610, startTime, endTime, _date620);
        Assert.Throws<ArgumentException>(() =>
            blocked.IsBlocked(new DateOnly(2024, 6, 12), startTimeToCheck, endTimeToCheck));
    }
    
    
    // IsBlocked tests with recurrence interval > 1

    [Fact]
    public void IsBlocked_ShouldFallBackToInterval1_WhenStartDateIsNotSet() {
        // Arrange
        var startTime = new TimeOnly(8, 0);
        var endTime = new TimeOnly(17, 0);
        var blockedTime = new BlockedSchedule( _date610, startTime, endTime);
        blockedTime.RecurDaily(2);

        // Act
        var isBlocked = blockedTime.IsBlocked(_date610, startTime, endTime);

        // Assert
        Assert.True(isBlocked);
        
        isBlocked = blockedTime.IsBlocked(_date610.AddDays(2), startTime, endTime);
        Assert.True(isBlocked);
    }

    [Fact]
    public void IsBlocked_ShouldReturnFalse_WhenRecurrenceIntervalIsNotMet() {
        // Arrange
        var startTime = new TimeOnly(8, 0);
        var endTime = new TimeOnly(17, 0);
        var blockedTime = new BlockedSchedule( _date610, startTime, endTime);
        blockedTime.RecurDaily(3);

        // Act & Assert
        Assert.False(blockedTime.IsBlocked(_date610.AddDays(1), startTime, endTime));
        Assert.False(blockedTime.IsBlocked(_date610.AddDays(2), startTime, endTime));
        Assert.True(blockedTime.IsBlocked(_date610.AddDays(3), startTime, endTime));
        Assert.False(blockedTime.IsBlocked(_date610.AddDays(4), startTime, endTime));
        Assert.True(blockedTime.IsBlocked(_date610.AddDays(6), startTime, endTime));
    }

    [Fact]
    public void IsBlocked_ShouldReturnFalse_WhenRecurrenceIntervalIsNotMet_WeeklyRecurrence() {
        // Arrange
        var startTime = new TimeOnly(8, 0);
        var endTime = new TimeOnly(17, 0);
        var blockedTime = new BlockedSchedule( _date610, startTime, endTime);
        blockedTime.RecurWeekly([_date610.DayOfWeek], 3); // Set weekly recurrence with interval of 2 weeks

        // Act & Assert
        Assert.False(blockedTime.IsBlocked(_date610.AddDays(7), startTime, endTime));
        Assert.False(blockedTime.IsBlocked(_date610.AddDays(14), startTime, endTime));
        Assert.True(blockedTime.IsBlocked(_date610.AddDays(21), startTime, endTime));
        Assert.False(blockedTime.IsBlocked(_date610.AddDays(28), startTime, endTime));
        Assert.False(blockedTime.IsBlocked(_date610.AddDays(35), startTime, endTime));
    }
    
}