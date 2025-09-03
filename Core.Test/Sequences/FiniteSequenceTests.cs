using Core.Sequences;

namespace Core.Test.Sequences;

public class SequenceTests {

    [Fact]
    public void ShouldHaveLengthOfOne_WhenStartIsEqualToEnd() {
        var sequence = new FiniteSequence(10, 10);
        Assert.Equal(1, sequence.Length);
        
        sequence = new FiniteSequence(10, 10, 100);
        Assert.Equal(1, sequence.Length);
    }

    [Fact]
    public void ShouldReturnTheFirstElement_For0() {
        var sequence = new FiniteSequence(10, 10);
        Assert.Equal(10, sequence.S(0));
    }

    [Fact]
    public void ShouldThrowException_WhenNegativeIndex() {
        var sequence = new FiniteSequence(10, 10);
        Assert.Throws<ArgumentOutOfRangeException>(() => sequence.S(-1));
    }

    [Fact]
    public void ShouldThrowException_WhenIndexOutOfRange() {
        var sequence = new FiniteSequence(10, 10);
        Assert.Throws<ArgumentOutOfRangeException>(() => sequence.S(1));
    }

    [Fact]
    public void ShouldReturnTheElementAtIndex_WhenInRange() {
        var sequence = new FiniteSequence(10, 20, 3);
        Assert.Equal(13, sequence.S(1));
        Assert.Equal(16, sequence.S(2));
        Assert.Equal(19, sequence.S(3));
    }
    
    [Fact]
    public void ShouldBeEmpty_WhenStartIsLessThanEnd() {
        var sequence = new FiniteSequence(10, 9);
        Assert.True(sequence.IsEmpty);
    }

    [Fact]
    public void S_ShouldThrowArgumentException_OnEmptySequence() {
        var sequence = new FiniteSequence(10, 9);
        Assert.Throws<ArgumentOutOfRangeException>(() => sequence.S(0));
    }

    [Fact]
    public void Length_ShouldBeLessThanOrEqualToZero_ForEmptySequence() {
        var sequence = new FiniteSequence(10, 9);
        Assert.True(sequence.Length <= 0);
    }
    
    [Fact]
    public void Length_ShouldBeGreaterThanZero_ForNotEmptySequence() {
        var sequence = new FiniteSequence(0, 5, 6);
        Assert.True(sequence.Length > 0);
    }
    
    [Fact]
    public void StartFromIndex_ShouldReturnASequenceThatStartsFromTheFirstElement_WhenIndexIsZero() {
        var sequence = new FiniteSequence(10, 11);
        var shifted = sequence.StartFromIndex(0);
        Assert.Equal(10, shifted?.S(0));
    }

    [Fact]
    public void StartFromIndex_ShouldReturnASequenceThatStartsFromTheSecondElement_WhenIndexIsOne() {
        var sequence = new FiniteSequence(10, 11);
        var shifted = sequence.StartFromIndex(1);
        Assert.Equal(11, shifted?.S(0));
    }

    [Fact]
    public void
        StartFromIndex_ShouldReturnASequenceThatStartsFromTheLastElement_WhenIndexIsTheLengthOfTheSequenceMinusOne() {
        var sequence = new FiniteSequence(10, 455, 5);
        var shifted = sequence.StartFromIndex(sequence.Length!.Value - 1);
        Assert.Equal(455, shifted?.S(0));
    }

    [Fact]
    public void StartFromIndex_ShouldReturnAnEmptySequence_WhenLengthIsOneAndIndexIsOne() {
        var sequence = new FiniteSequence(10, 10, 5);
        var shifted = sequence.StartFromIndex(1);
        
        Assert.True(shifted.IsEmpty);
    }

    [Fact]
    public void StartFromIndex_ShouldReturnAnEmptySequence_WhenIndexIsGreaterThanOrEqualToLength() {
        var sequence = new FiniteSequence(10, 455);
        var shifted = sequence.StartFromIndex(sequence.Length!.Value);
        Assert.True(shifted.IsEmpty);
        
        shifted = sequence.StartFromIndex(sequence.Length!.Value + 1);
        Assert.True(shifted.IsEmpty);
    }
}