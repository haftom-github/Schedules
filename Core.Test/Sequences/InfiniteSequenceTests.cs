using Core.Sequences;

namespace Core.Test.Sequences;

public class InfiniteSequenceTests {

    [Fact]
    public void ShouldNotBeEmpty() {
        var sequence = new InfiniteSequence(0);
        Assert.False(sequence.IsEmpty);
    }
    
    [Fact]
    public void ShouldHaveLengthOfNull() {
        var sequence = new InfiniteSequence(10, 10);
        Assert.Null(sequence.Length);
    }

    [Fact]
    public void ShouldReturnTheFirstElement_For0() {
        var sequence = new InfiniteSequence(10, 10);
        Assert.Equal(10, sequence.S(0));
    }

    [Fact]
    public void ShouldThrowException_WhenNegativeIndex() {
        var sequence = new InfiniteSequence(10, 10);
        Assert.Throws<ArgumentOutOfRangeException>(() => sequence.S(-1));
    }

    [Fact]
    public void ShouldReturnTheElementAtIndex_WhenInRange() {
        var sequence = new InfiniteSequence(10, 3);
        Assert.Equal(13, sequence.S(1));
        Assert.Equal(16, sequence.S(2));
        Assert.Equal(19, sequence.S(3));
    }
    
    [Fact]
    public void StartFromIndex_ShouldReturnASequenceThatStartsFromTheFirstElement_WhenIndexIsZero() {
        var sequence = new InfiniteSequence(10, 11);
        var shifted = sequence.StartFromIndex(0);
        Assert.Equal(10, shifted.S(0));
    }

    [Fact]
    public void StartFromIndex_ShouldReturnASequenceThatStartsFromTheSecondElement_WhenIndexIsOne() {
        var sequence = new InfiniteSequence(10, 11);
        var shifted = sequence.StartFromIndex(1);
        Assert.Equal(21, shifted.S(0));
    }
    
    [Fact]
    public void FirstElementShouldBeAMember() {
        var sequence = new InfiniteSequence(10);
        Assert.True(sequence.IsMember(10));
    }

    [Fact]
    public void ShouldBeAMember() {
        var sequence = new InfiniteSequence(10, 5);
        Assert.True(sequence.IsMember(10));
        Assert.True(sequence.IsMember(15));
        Assert.True(sequence.IsMember(10 + 5 * 20));
    }

    [Fact]
    public void ShouldNotBeAMember() {
        var sequence = new InfiniteSequence(10, 3);
        Assert.False(sequence.IsMember(11));
        Assert.False(sequence.IsMember(12));
        Assert.False(sequence.IsMember(10 + 3 * 20 + 2));
    }
    
    [Fact]
    public void ShouldCollapseToSelf() {
        var sequence = new InfiniteSequence(10, 3);
        var collapsed = sequence.CollapseToRangeOf(sequence);
        Assert.Equal(sequence, collapsed);
    }

    [Fact]
    public void StartOfTheCollapsed_ShouldBeEqualToTheOtherSequencesStart_WhenTheLaterIsBiggerAndTheLaterIsAMemberOfTheOriginal() {
        var sequence = new  InfiniteSequence(1, 3);
        var other = new FiniteSequence(10, 20, 3);

        Assert.Equal(other.Start, sequence.CollapseToRangeOf(other).Start);
    }
    
    [Fact]
    public void StartOfTheCollapsed_ShouldBeEqualToTheOriginalSequencesStart_WhenTheFormerIsBigger() {
        var sequence = new InfiniteSequence(10, 3);
        var other = new  FiniteSequence(1, 20, 3);
        
        Assert.Equal(sequence.Start, sequence.CollapseToRangeOf(other).Start);
    }
    
    [Fact]
    public void StartOfTheCollapsed_ShouldBeEqualToTheSmallestMemberInTheRangeOfTheOther_WhenTheLaterIsBiggerAndTheLaterIsNotMemberOfTheOriginal() {
        var sequence = new  InfiniteSequence(1, 3);
        var other = new FiniteSequence(9, 20, 3);

        Assert.Equal(10, sequence.CollapseToRangeOf(other).Start);
        
        other = new  FiniteSequence(11, 20, 3);
        Assert.Equal(13, sequence.CollapseToRangeOf(other).Start);
    }

    [Fact]
    public void IntervalOfTheCollapsed_ShouldBeEqualToTheIntervalOfTheOriginal() {
        var sequence = new InfiniteSequence(10, 3);
        var other = new  FiniteSequence(10, 20, 4);
        Assert.Equal(3, sequence.CollapseToRangeOf(other).Interval);
    }

    [Fact]
    public void EndShouldBe_EqualToEndOfTheOther_WhenTheOtherIsFinite() {
        var sequence = new  InfiniteSequence(10, 4);
        var other = new FiniteSequence(10, 20, 3);
        Assert.Equal(20, sequence.CollapseToRangeOf(other).End);
    }
    
    [Fact]
    public void Collapsed_ShouldBeInfinite_WhenOtherIsInfinite() {
        var sequence = new InfiniteSequence(10, 3);
        var other = new InfiniteSequence(10, 4);
        Assert.True(sequence.CollapseToRangeOf(other).IsInfinite);
    }

    [Fact]
    public void ShouldCollapse_ToEmptySequence_WhenNoCommonRange() {
        var sequence = new InfiniteSequence(10, 3);
        var other = new FiniteSequence(9, 9);
        
        var collapsed =  sequence.CollapseToRangeOf(other);
        Assert.True(collapsed.IsEmpty);
        
        other = new FiniteSequence(0, 9);
        collapsed = sequence.CollapseToRangeOf(other);
        Assert.True(collapsed.IsEmpty);
    }
}