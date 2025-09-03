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
        Assert.Equal(10, shifted?.S(0));
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
}