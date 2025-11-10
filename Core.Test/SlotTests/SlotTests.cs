namespace Core.Test.SlotTests;

public class SlotTests {
    private readonly TimeOnly _twoOClock = new(2, 0);
    private readonly TimeOnly _threeOClock = new(3, 0);
    private readonly TimeOnly _fourOClock = new(4, 0);
    private readonly TimeOnly _eightOClock = new(8, 0);
    
    [Fact]
    public void SameSlotsShouldOverlap() {
        var s1 = new Slot();
        var s2 = new Slot();
        
        var overlap = s1.Overlap(s2);
        Assert.Equal(overlap, s2);
    }

    [Fact]
    public void NonOverlappingSlotsShouldOverlap() {
        var s1 = new Slot(_twoOClock, _threeOClock);
        var s2 = new Slot(_threeOClock, _fourOClock);
        
        var overlap = s1.Overlap(s2);
        Assert.True(overlap.IsEmpty);
    }

    [Fact]
    public void ShouldOverlap_WhenOneIsInsideTheOtherSlot() {
        var s1 = new Slot(_twoOClock, _eightOClock);
        var s2 = new Slot(_threeOClock, _fourOClock);
        
        var overlap = s1.Overlap(s2);
        Assert.Equal(s2, overlap);
    }

    [Fact]
    public void ASlotOnTheSameDay_ShouldNotOverlap_WithASlotOnTheNextDay() {
        var s1 = new Slot(_twoOClock, _threeOClock);
        var s2 = new Slot(TimeSpan.FromHours(26), TimeSpan.FromHours(27));
        
        var overlap = s1.Overlap(s2);
        Assert.True(overlap.IsEmpty);
    }

    [Fact]
    public void TwoMidnightCrossingSlots_ShouldOverlap() {
        var s1 = new Slot(_threeOClock, _threeOClock);
        var s2 = new Slot(_fourOClock, _fourOClock);
        
        var overlap = s1.Overlap(s2);
        Assert.False(overlap.IsEmpty);
    }
}