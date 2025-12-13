namespace Core.Test.RScheduleTests;

public class RScheduleTests {
    [Fact]
    public void ShouldBeEmpty_WhenNoSlotsAreProvided() {
        var s = new RSchedule();
        Assert.True(s.IsEmpty);
    }

    [Fact]
    public void ShouldNotBeEmpty_WhenANonEmptySlotIsProvided() {
        var slot = new RSlot(0, 1);
        var s = new RSchedule(slot);
        
        Assert.False(s.IsEmpty);
    }

    [Fact]
    public void ShouldBeEmpty_WhenEmptySlotIsProvided() {
        var slot = new RSlot(0, 0);
        var s = new RSchedule(slot);
        
        Assert.True(s.IsEmpty);
    }

    [Fact]
    public void ShouldBeEmpty_WhenEmptyManyEmptySlotsAreProvided() {
        var slot1 = new RSlot(0, 0);
        var slot2 = new RSlot(1, 0);
        var s = new RSchedule([slot1, slot2]);
        
        Assert.True(s.IsEmpty);
    }

    [Fact]
    public void ShouldMerge_SlotsThatCanBeMerged() {
        var slot1 = new RSlot(0, 1);
        var slot2 = new RSlot(1, 1);
        var s = new RSchedule([slot1, slot2]);
        
        Assert.Single(s.Slots);
        Assert.Equal(0, s.Start);
        Assert.Equal(2, s.End);
    }

    [Fact]
    public void ShouldNotMerge_SlotsThatCanNotBeMerged() {
        var slot1 = new RSlot(0, 1);
        var slot2 = new RSlot(2, 1);
        var s = new RSchedule([slot1, slot2]);
        
        Assert.Equal(2, s.Slots.Length);
    }

    [Fact]
    public void OverallDuration_ShouldNotExtend_BeyondRecurrenceSpan() {
        var slot = new RSlot(0, 2);
        Assert.Throws<ArgumentException>(() => new RSchedule(slot, 1));
        
        var otherSlot = new RSlot(4, 3);
        Assert.Throws<ArgumentException>(() => new RSchedule([slot, otherSlot], 6));
    }

    #region OverlapingRotationInstances

    [Fact]
    public void HasOverlapsWithIn_RotationInstances() {
        var slot1 = new RSlot(0, 2);

        var s = new RSchedule(slot1, 2, new Rotation(2, [0, 1]));
        var overlaps = s.OverlapsWithIn;
        Assert.True(overlaps.Any());
    }

    #endregion
}