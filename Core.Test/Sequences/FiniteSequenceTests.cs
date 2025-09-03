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
        Assert.Equal(10, shifted.S(0));
    }

    [Fact]
    public void StartFromIndex_ShouldReturnASequenceThatStartsFromTheSecondElement_WhenIndexIsOne() {
        var sequence = new FiniteSequence(10, 11);
        var shifted = sequence.StartFromIndex(1);
        Assert.Equal(11, shifted.S(0));
    }

    [Fact]
    public void
        StartFromIndex_ShouldReturnASequenceThatStartsFromTheLastElement_WhenIndexIsTheLengthOfTheSequenceMinusOne() {
        var sequence = new FiniteSequence(10, 455, 5);
        var shifted = sequence.StartFromIndex(sequence.Length!.Value - 1);
        Assert.Equal(455, shifted.S(0));
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

    [Fact]
    public void FirstElementShouldBeAMemberIfNotEmpty() {
        var sequence = new FiniteSequence(10, 10);
        Assert.True(sequence.IsMember(10));
    }

    [Fact]
    public void TheLastElementShouldBeAMemberIfNotEmptyAndIsEffectiveEnd() {
        var sequence = new FiniteSequence(10, 455, 5);
        Assert.True(sequence.IsMember(455));
    }

    [Fact]
    public void TheLastElementShouldNotBeAMemberIfNotEmptyAndIsNotEffectiveEnd() {
        var sequence = new FiniteSequence(10, 456, 5);
        Assert.False(sequence.IsMember(456));
    }

    [Fact]
    public void NoOneShouldBeAMember_WhenAnEmptySequence() {
        var sequence = new FiniteSequence(10, 0);
        Assert.False(sequence.IsMember(0));
        Assert.False(sequence.IsMember(10));
        Assert.False(sequence.IsMember(1));
    }

    [Fact]
    public void ShouldCollapseToSelf() {
        var sequence = new FiniteSequence(10, 20, 3);
        var collapsed = sequence.CollapseToRangeOf(sequence);
        Assert.Equal(sequence, collapsed);
    }

    [Fact]
    public void StartOfTheCollapsed_ShouldBeEqualToTheOtherSequencesStart_WhenTheLaterIsBiggerAndTheLaterIsAMemberOfTheOriginal() {
        var sequence = new  FiniteSequence(1, 20, 3);
        var other = new FiniteSequence(10, 20, 3);

        Assert.Equal(other.Start, sequence.CollapseToRangeOf(other).Start);
    }
    
    [Fact]
    public void StartOfTheCollapsed_ShouldBeEqualToTheOriginalSequencesStart_WhenTheFormerIsBigger() {
        var sequence = new FiniteSequence(10, 20, 3);
        var other = new  FiniteSequence(1, 20, 3);
        
        Assert.Equal(sequence.Start, sequence.CollapseToRangeOf(other).Start);
    }
    
    [Fact]
    public void StartOfTheCollapsed_ShouldBeEqualToTheSmallestMemberInTheRangeOfTheOther_WhenTheLaterIsBiggerAndTheLaterIsNotMemberOfTheOriginal() {
        var sequence = new  FiniteSequence(1, 20, 3);
        var other = new FiniteSequence(9, 20, 3);

        Assert.Equal(10, sequence.CollapseToRangeOf(other).Start);
        
        other = new  FiniteSequence(11, 20, 3);
        Assert.Equal(13, sequence.CollapseToRangeOf(other).Start);
    }

    [Fact]
    public void IntervalOfTheCollapsed_ShouldBeEqualToTheIntervalOfTheOriginal() {
        var sequence = new FiniteSequence(10, 20, 3);
        var other = new  FiniteSequence(10, 20, 4);
        Assert.Equal(3, sequence.CollapseToRangeOf(other).Interval);
    }

    [Fact]
    public void EndShouldBe_EqualToEndOfTheOther_WhenTheOtherEndIsSmaller() {
        var sequence = new  FiniteSequence(10, 30, 4);
        var other = new FiniteSequence(10, 20, 3);
        Assert.Equal(20, sequence.CollapseToRangeOf(other).End);
    }
    
    [Fact]
    public void EndShouldBe_EqualToEndOfTheOriginal_WhenTheOriginalEndIsSmaller() {
        var sequence = new FiniteSequence(10, 20, 3);
        var other = new  FiniteSequence(10, 30, 4);
        Assert.Equal(20, sequence.CollapseToRangeOf(other).End);
    }

    [Fact]
    public void EndShouldBe_EqualToEndOfTheOriginal_WhenTheOtherIsInfinite() {
        var sequence = new FiniteSequence(10, 30, 4);
        var other = new InfiniteSequence(30, 30);
        
        Assert.Equal(30, sequence.CollapseToRangeOf(other).End);
    }

    [Fact]
    public void ShouldCollapse_ToEmptySequence_WhenNoCommonRange() {
        var sequence = new FiniteSequence(10, 15, 3);
        var other = new FiniteSequence(14, 17);
        
        var collapsed =  sequence.CollapseToRangeOf(other);
        Assert.True(collapsed.IsEmpty);
        
        var another = new InfiniteSequence(14);
        collapsed = sequence.CollapseToRangeOf(another);
        Assert.True(collapsed.IsEmpty);
    }
}