namespace Core.Sequences;

public static class SequenceMath {
    private static int Gcd(int a, int b) {
        while (a != 0) {
            var temp = a;
            a = b % a;
            b = temp;
        }
        return b;
    }
    
    private static (int gcd, int x, int y) ExtendedGcd(int a, int b) {
        if (a == 0) 
            return (b, 0, 1);
        
        var (gcd, x1, y1) = ExtendedGcd(b % a, a);
        return (gcd, y1 - (b / a) * x1, x1);
    }

    /// <summary>
    /// checks if two sequences meet.
    /// if both sequences are infinite
    /// if one of the sequences is finite, the second sequence will be collapsed to the finite sequence's end.
    /// </summary>
    /// <param name="s1">first sequence</param>
    /// <param name="s2">the second sequence</param>
    /// <returns><c>true</c> if the sequences have a possibility of overlapping</returns>
    public static bool Overlaps(ISequence s1, ISequence s2) {
        if (!s1.IsFinite && !s2.IsFinite)
            return OverlapsUnbounded(s1, s2);

        return FirstOverlapSequence(s1, s2) != null;
    }
    
    private static bool OverlapsUnbounded(ISequence s1, ISequence s2) {
        var gcd = Gcd(s1.Interval, -s2.Interval);
        return (s2.Start - s1.Start) % gcd != 0;
    }

    /// <summary>
    /// computes first last and the number of overlapping occurrences.
    /// if one of the sequences is finite, the second sequence will be collapsed to the finite
    /// </summary>
    /// <param name="s1">first sequence</param>
    /// <param name="s2">second sequence</param>
    /// <returns>
    /// returns a <c>tupple</c> of the <c>f</c>: first <c>l</c>: last and <c>count</c>: no of occurrences.
    /// count <c>0</c> implies no overlap
    /// count <c>null</c> implies infinite overlaps with <c>l</c>: being null
    /// </returns>
    public static ISequence? FirstOverlapSequence(ISequence s1, ISequence s2) {
        if (!s1.IsFinite && !s2.IsFinite)
            return FirstOverlapInfinite(s1, s2);

        try {
            var finiteS1 = new FiniteSequence(s1.Start, s1.End ?? s2.End!.Value, s1.Interval);
            var finiteS2 = new FiniteSequence(s2.Start, s2.End ?? finiteS1.End!.Value, s2.Interval);
            return FirstOverlapFinite(finiteS1, finiteS2);
        }
        catch (ArgumentOutOfRangeException) {
            return null;
        }
    }
    
    private static FiniteSequence? FirstOverlapFinite(FiniteSequence s1, FiniteSequence s2) {
        if (s1.End < s2.Start || s2.End < s1.Start)
            return null;

        var (gcd, x0, y0) = ExtendedGcd(s1.Interval, -s2.Interval);

        if ((s2.Start - s1.Start) % gcd != 0)
            return null;
        
        var k = (s2.Start - s1.Start) / gcd;
        x0 *= k;
        y0 *= k;
    
        var xStep = -s2.Interval / gcd;
        var yStep = s1.Interval / gcd;
        
        var minEnd = Math.Min(s1.End!.Value, s2.End!.Value);
        
        var aOccurence = (minEnd - s1.Start) / s1.Interval;
        var bOccurence = (s2.Start - minEnd) / -s2.Interval;
        var ta = xStep > 0 ? Floor(aOccurence - x0, xStep) : Ceil(aOccurence - x0, xStep);
        var tb = yStep < 0 ? Floor(y0 - bOccurence, yStep) : Ceil(y0 - bOccurence, yStep);
        var ra = xStep > 0 ? Ceil(-x0, xStep) : Floor(-x0, xStep);
        var rb = yStep < 0 ? Ceil(y0, yStep) : Floor(y0, yStep);
        (int f, int l)? solution;
        if ((xStep ^ yStep) >= 0) {
            solution = xStep > 0
                ? OverlapOfRange(ra, ta, tb, rb)
                : OverlapOfRange(ta, ra, rb, tb);
            if (solution == null) return null;
            if (xStep < 0) solution = (solution.Value.l, solution.Value.f);
            return new FiniteSequence(s1.S(x0 + solution.Value.f * xStep), 
                s1.S(x0 + solution.Value.l * xStep), s1.Interval * Math.Abs(xStep));
        }

        solution = xStep > 0
            ? OverlapOfRange(ra, ta, rb, tb)
            : OverlapOfRange(ta, ra, tb, rb);
        if (solution == null) return null;
        if (xStep < 0) solution = (solution.Value.l, solution.Value.f);
        return new FiniteSequence(s1.S(x0 + solution.Value.f * xStep),
                s1.S(x0 + solution.Value.l * xStep), s1.Interval * Math.Abs(xStep));
    }

    private static InfiniteSequence? FirstOverlapInfinite(ISequence s1, ISequence s2) {
        var (gcd, x0, y0) = ExtendedGcd(s1.Interval, -s2.Interval);
        
        if ((s2.Start - s1.Start) % gcd != 0) return null; 
        
        var k = (s2.Start - s1.Start) / gcd;
        x0 *= k;
        y0 *= k;
    
        var xStep = -s2.Interval / gcd;
        var yStep = s1.Interval / gcd;
        
        var ra = xStep > 0 ? Ceil(-x0, xStep) : Floor(-x0, xStep);
        var rb = yStep < 0 ? Ceil(y0, yStep) : Floor(y0, yStep);
        int? t;
        if ((xStep ^ yStep) >= 0) {
            t = xStep > 0 ? ra : rb;
            return new InfiniteSequence(s1.S(x0 + t.Value * xStep), s1.Interval * Math.Abs(xStep));
        }

        t = xStep > 0
            ? Math.Max(ra, rb)
            : Math.Min(ra, rb);

        return new InfiniteSequence(s1.S(x0 + t.Value * xStep), s1.Interval * Math.Abs(xStep));
    }

    private static (int l, int u)? OverlapOfRange(int al, int au, int bl, int bu) {
        var minUpper = Math.Min(au, bu);
        var maxLower = Math.Max(al, bl);
        if (minUpper < maxLower) return null;
        return (maxLower, minUpper);
    }
    
    private static int Floor(int a, int b) =>
        (a ^ b) < 0 ? a < 0 ? (a - b + 1) / b : (a - b -1) / b : a / b;
    
    private static int Ceil(int a, int b) =>
        (a ^ b) < 0 ? a / b : a < 0 ? (a + b + 1) / b : (a + b - 1) / b;
}