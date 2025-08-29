using Core.Sequences;

namespace Core.Test.Sequences;

public class SequenceMathTests {
    [Fact]
    public void Overlaps_FiniteSequences_ShouldReturnFalseWhenEffectiveRangesDoNotOverlap() {
        var s1 = new FiniteSequence(3, 20, 1);
        var s2 = new FiniteSequence(21, 40, 1);
        
        var result = SequenceMath.SequencesOverlap(s1, s2);
        Assert.False(result);

        s1 = new FiniteSequence(0, 69, 10);
        s2 = new FiniteSequence(65, 80, 5);
        
        result = SequenceMath.SequencesOverlap(s1, s2);
        Assert.False(result);
        
        
    }
    
    [Fact]
    public void Overlaps_FiniteSequences_ShouldReturnFalse_WhenSequencesNeverOverlap_NotConsideringWhereTheSequenceEnds() {
        var s1 = new FiniteSequence(0, 900, 2);
        var s2 = new FiniteSequence(1, 5478, 2);
        
        var result = SequenceMath.SequencesOverlap(s1, s2);
        Assert.False(result);

        s1 = new FiniteSequence(0, 754893, 4);
        s2 = new FiniteSequence(1, 2457935, 6);
        
        result = SequenceMath.SequencesOverlap(s1, s2);
        Assert.False(result);
        
        s1 = new FiniteSequence(30, 60, 5);
        s2 = new FiniteSequence(31, 37, 6);
        result = SequenceMath.SequencesOverlap(s1, s2);
        Assert.False(result);
    }
    
    // tests for non-finite sequences
    [Theory]
    [InlineData(15, 3, 20, 1)] 
    [InlineData(10, 5, 30, 2)] 
    [InlineData(0, 10, 100, 5)]
    [InlineData(5, 0, 50, 10)]
    public void Overlaps_FirstSequenceNotFinite_ShouldFallbackToFiniteSequenceVersionBasedOnSecondOne(int s1Start, int s2Start, int s2End, int step) {
        var s1 = new InfiniteSequence(s1Start, step);
        var s2 = new FiniteSequence(s2Start, s2End, step);
        var s1Fallback = new FiniteSequence(s1Start, s2.End!.Value, step);

        Assert.Equal(SequenceMath.SequencesOverlap(s1, s2), SequenceMath.SequencesOverlap(s1Fallback, s2));
    }
    
    [Fact]
    public void FirstOverlap_ShouldReturnFirstOverlapOfTwoFiniteSequences() {
        var s1 = new FiniteSequence(0, 20, 5);
        var s2 = new FiniteSequence(10, 30, 5);
        
        var firstOverlap = SequenceMath.GetOverlapOfSequences(s1, s2);
        Assert.NotNull(firstOverlap);
        Assert.Equal(10, firstOverlap.Start);
        
        s1 = new FiniteSequence(0, 20, 5);
        s2 = new FiniteSequence(21, 30, 5);
        firstOverlap = SequenceMath.GetOverlapOfSequences(s1, s2);
        Assert.Null(firstOverlap);
        
        s1 = new FiniteSequence(0, 20, 5);
        s2 = new FiniteSequence(5, 25, 5);
        firstOverlap = SequenceMath.GetOverlapOfSequences(s1, s2);
        Assert.NotNull(firstOverlap);
        Assert.Equal(5, firstOverlap.Start);
        
        s1 = new FiniteSequence(0, 20, 5);
        s2 = new FiniteSequence(0, 20, 5);
        firstOverlap = SequenceMath.GetOverlapOfSequences(s1, s2);
        Assert.NotNull(firstOverlap);
        Assert.Equal(0, firstOverlap.Start);
        
        s1 = new FiniteSequence(0, 25, 5);
        s2 = new FiniteSequence(1, 26, 6);
        firstOverlap = SequenceMath.GetOverlapOfSequences(s1, s2);
        Assert.NotNull(firstOverlap);
        Assert.Equal(25, firstOverlap.Start);
    }
    
    // Brute force helper to verify correctness
    private static int? BruteForceFirstOverlap(InfiniteSequence s1, InfiniteSequence s2, int searchLimit = 100000)
    {
        int? earliest = null;
        for (var i = 0; i < searchLimit; i++)
        {
            var val1 = s1.Start + i * s1.Interval;
            if (val1 < s1.Start || val1 < s2.Start) continue; // ensure "future"
            for (var j = 0; j < searchLimit; j++)
            {
                var val2 = s2.Start + j * s2.Interval;
                if (val1 != val2) continue;
                earliest = val1;
                break;
            }
            if (earliest.HasValue) break;
        }
        return earliest;
    }

    [Fact]
    public void OverlapInFuture_PositiveIntervals()
    {
        var s1 = new InfiniteSequence(5, 3);  // 5, 8, 11, 14...
        var s2 = new InfiniteSequence(2, 4);  // 2, 6, 10, 14...
        var overlap = SequenceMath.GetOverlapOfSequences(s1, s2);
        Assert.NotNull(overlap);
        Assert.Equal(14, overlap.Start);
    }

    [Fact]
    public void OverlapAtStart()
    {
        var s1 = new InfiniteSequence(10, 5);
        var s2 = new InfiniteSequence(10, 7);
        var overlap = SequenceMath.GetOverlapOfSequences(s1, s2);
        Assert.NotNull(overlap);
        Assert.Equal(10, overlap.Start);
    }

    [Fact]
    public void NoOverlap()
    {
        var s1 = new InfiniteSequence(0, 4);
        var s2 = new InfiniteSequence(3, 6);
        Assert.Null(SequenceMath.GetOverlapOfSequences(s1, s2));
    }

    [Theory]
    [InlineData(5, 3, 2, 4)]
    [InlineData(0, 4, 3, 6)]
    [InlineData(100, 12, 110, 18)]
    [InlineData(15, 5, 40, 10)]
    public void MatchesBruteForce(int start1, int int1, int start2, int int2)
    {
        var s1 = new InfiniteSequence(start1, int1);
        var s2 = new InfiniteSequence(start2, int2);

        var expected = BruteForceFirstOverlap(s1, s2);
        var actual = SequenceMath.GetOverlapOfSequences(s1, s2)?.Start;

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void FirstOverlap_ShouldWorkAsExpected_When_Single_item_sequences() {
        var s1 = new InfiniteSequence(0, 5);
        var s2 = new FiniteSequence(5, 10, 6);
        
        var overlap = SequenceMath.GetOverlapOfSequences(s1, s2);
        Assert.NotNull(overlap);
    }
}