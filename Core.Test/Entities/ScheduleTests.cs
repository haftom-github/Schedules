using Core.Entities;
using Core.Enums;
using Core.Options;

namespace Core.Test.Entities;

public class ScheduleTests {

    // constructor tests
    [Fact]
    public void Constructor_ShouldInitializeProperties_WhenValidParametersAreProvided() {
        // Arrange
        var startTime = new TimeOnly(9, 0);
        var endTime = new TimeOnly(17, 0);
        var startDate = TimeSettings.Today.AddDays(1);
        
        // Act
        var schedule = new Schedule(startTime, endTime, startDate);
        
        // Assert
        Assert.Equal(startTime, schedule.StartTime);
        Assert.Equal(endTime, schedule.EndTime);
        Assert.Equal(startDate, schedule.StartDate);
        Assert.Equal(RecurrenceType.Daily, schedule.RecurrenceType);
        Assert.Null(schedule.EndDate);
        
        var endDate = TimeSettings.Today.AddDays(2);
        schedule = new Schedule(startTime, endTime, startDate, endDate);
        
        Assert.Equal(endDate, schedule.EndDate);
        
    }

    // RecurDaily tests
    [Fact]
    public void RecurDaily_ShouldSetRecurrenceProperties_WhenCalledWithValidInterval() {
        // Arrange
        var startTime = new TimeOnly(9, 0);
        var endTime = new TimeOnly(17, 0);
        var startDate = TimeSettings.Today.AddDays(1);
        var schedule = new Schedule(startTime, endTime, startDate);
        
        // Act
        schedule.RecurDaily(2);
        
        // Assert
        Assert.Equal(RecurrenceType.Daily, schedule.RecurrenceType);
        Assert.Equal(2, schedule.RecurrenceInterval);
    }

    [Fact]
    public void RecurDaily_ShouldThrowArgumentOutOfRangeException_WhenIntervalIsNotPositive() {
        // Arrange
        var startTime = new TimeOnly(9, 0);
        var endTime = new TimeOnly(17, 0);
        var startDate = TimeSettings.Today.AddDays(1);
        var schedule = new Schedule(startTime, endTime, startDate);
        
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => schedule.RecurDaily(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => schedule.RecurDaily(-1));
    }
    
    // RecurWeekly tests
    [Fact]
    public void RecurWeekly_ShouldSetRecurrenceProperties_WhenCalledWithValidDaysAndInterval() {
        // Arrange
        var startTime = new TimeOnly(9, 0);
        var endTime = new TimeOnly(17, 0);
        var startDate = TimeSettings.Today.AddDays(1);
        var schedule = new Schedule(startTime, endTime, startDate);
        
        // Act
        schedule.RecurWeekly([DayOfWeek.Monday, DayOfWeek.Wednesday], 2);
        
        // Assert
        Assert.Equal(RecurrenceType.Weekly, schedule.RecurrenceType);
        Assert.Equal(2, schedule.RecurrenceInterval);
        Assert.NotNull(schedule.RecurrenceDays);
        Assert.Equal(2, schedule.RecurrenceDays.Count);
        Assert.Contains(DayOfWeek.Monday, schedule.RecurrenceDays);
        Assert.Contains(DayOfWeek.Wednesday, schedule.RecurrenceDays);
    }

    [Fact]
    public void RecurWeekly_ShouldThrowArgumentException_WhenDaysOfWeekIsNullOrEmpty() {
        // Arrange
        var startTime = new TimeOnly(9, 0);
        var endTime = new TimeOnly(17, 0);
        var startDate = TimeSettings.Today.AddDays(1);
        var schedule = new Schedule(startTime, endTime, startDate);
        
        // Act & Assert
        Assert.Throws<ArgumentException>(() => schedule.RecurWeekly(null!));
        Assert.Throws<ArgumentException>(() => schedule.RecurWeekly([]));
    }

    [Fact]
    public void RecurWeekly_ShouldThrowArgumentOutOfRangeException_WhenIntervalIsNotPositive() {
        // Arrange
        var startTime = new TimeOnly(9, 0);
        var endTime = new TimeOnly(17, 0);
        var startDate = TimeSettings.Today.AddDays(1);
        var schedule = new Schedule(startTime, endTime, startDate);
        
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => schedule.RecurWeekly([DayOfWeek.Monday], 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => schedule.RecurWeekly([DayOfWeek.Monday], -1));
    }
    
    // UpdateRecurrenceInterval tests

    [Fact]
    public void UpdateRecurrenceInterval_ShouldUpdateInterval_WhenCalledWithValidInterval() {
        // Arrange
        var startTime = new TimeOnly(9, 0);
        var endTime = new TimeOnly(17, 0);
        var startDate = TimeSettings.Today.AddDays(1);
        var schedule = new Schedule(startTime, endTime, startDate);
        
        // Act
        schedule.RecurDaily(); // Set initial interval
        schedule.UpdateRecurrenceInterval(3);
        
        // Assert
        Assert.Equal(3, schedule.RecurrenceInterval);
    }
    
    [Fact]
    public void UpdateRecurrenceInterval_ShouldThrowArgumentOutOfRangeException_WhenIntervalIsNotPositive() {
        // Arrange
        var startTime = new TimeOnly(9, 0);
        var endTime = new TimeOnly(17, 0);
        var startDate = TimeSettings.Today.AddDays(1);
        var schedule = new Schedule(startTime, endTime, startDate);
        
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => schedule.UpdateRecurrenceInterval(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => schedule.UpdateRecurrenceInterval(-1));
    }
}