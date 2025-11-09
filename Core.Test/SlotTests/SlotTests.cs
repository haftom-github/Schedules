namespace Core.Test.SlotTests;

public class SlotTests {
    private TimeOnly _twoOclock = new(2, 0);
    private TimeOnly _threeOclock = new(3, 0);
    private TimeOnly _fourOclock = new(4, 0);
    private TimeOnly _eightOclock = new(8, 0);
    
    [Fact]
    public void SameSlotsShouldOverlap() {
        var s1 = new Slot();
        var s2 = new Slot();
        
        var overlap = s1.Overlap(s2);
        Assert.Equal(overlap, s2);
    }

    [Fact]
    public void NonOverlappingSlotsShouldOverlap() {
        var s1 = new Slot(_twoOclock, _threeOclock);
        var s2 = new Slot(_threeOclock, _fourOclock);
        
        var overlap = s1.Overlap(s2);
        Assert.True(overlap.IsEmpty);
    }

    [Fact]
    public void ShouldOverlap_WhenOneIsInsideTheOtherSlot() {
        var s1 = new Slot(_twoOclock, _eightOclock);
        var s2 = new Slot(_threeOclock, _fourOclock);
        
        var overlap = s1.Overlap(s2);
        Assert.Equal(s2, overlap);
    }

    [Fact]
    public void ASlotOnTheSameDay_ShouldNotOverlap_WithASlotOnTheNextDay() {
        var s1 = new Slot(_twoOclock, _threeOclock);
        var s2 = new Slot(TimeSpan.FromHours(26), TimeSpan.FromHours(27));
        
        var overlap = s1.Overlap(s2);
        Assert.True(overlap.IsEmpty);
    }

    [Fact]
    public void TwoMidnightCrossingSlots_ShouldOverlap() {
        var s1 = new Slot(_threeOclock, _threeOclock);
        var s2 = new Slot(_fourOclock, _fourOclock);
        
        var overlap = s1.Overlap(s2);
        Assert.False(overlap.IsEmpty);
    }
}