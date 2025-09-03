namespace Core.Sequences;

public static class SequenceMath {

    public static bool OverlapsWith(this ISequence s1, ISequence s2) {
        if (s1.IsInfinite && s2.IsInfinite)
            return InfiniteSequencesOverlap(s1, s2);

        return FindOverlapWith(s1, s2) != null;
    }

    /// <summary>
    /// returns all points of overlap as a sequence
    /// if one of the sequences is finite, the second sequence will be collapsed to the common finite range
    /// </summary>
    /// <returns>
    /// returns null if the sequences don't overlap
    /// </returns>
    public static ISequence? FindOverlapWith(this ISequence s1, ISequence s2) {
        if (s1.IsInfinite && s2.IsInfinite)
            return GetSequenceOfOverlapsForInfiniteSequences(s1, s2);

        try {
            var finiteS1 = new FiniteSequence(s1.Start, s1.End ?? s2.End!.Value, s1.Interval);
            var finiteS2 = new FiniteSequence(s2.Start, s2.End ?? finiteS1.End!.Value, s2.Interval);
            return GetSequenceOfOverlapsForFiniteSequences(finiteS1, finiteS2);
        }
        catch (ArgumentOutOfRangeException) {
            return null;
        }
    }
    
    private static FiniteSequence? GetSequenceOfOverlapsForFiniteSequences(FiniteSequence s1, FiniteSequence s2) {
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
        (int l, int u)? solution;
        if ((xStep ^ yStep) >= 0) {
            solution = xStep > 0
                ? FindBoundsOfIntersection((ra, ta), (tb, rb))
                : FindBoundsOfIntersection((ta, ra), (rb, tb));
            if (solution == null) return null;
            if (xStep < 0) solution = (solution.Value.u, solution.Value.l);
            return new FiniteSequence(s1.S(x0 + solution.Value.l * xStep), 
                s1.S(x0 + solution.Value.u * xStep), s1.Interval * Math.Abs(xStep));
        }

        solution = xStep > 0
            ? FindBoundsOfIntersection((ra, ta), (rb, tb))
            : FindBoundsOfIntersection((ta, ra), (tb, rb));
        if (solution == null) return null;
        if (xStep < 0) solution = (solution.Value.u, solution.Value.l);
        return new FiniteSequence(s1.S(x0 + solution.Value.l * xStep),
                s1.S(x0 + solution.Value.u * xStep), s1.Interval * Math.Abs(xStep));
    }

    // a finite sequence argument will be treated as an infinite sequence
    private static InfiniteSequence? GetSequenceOfOverlapsForInfiniteSequences(ISequence s1, ISequence s2) {
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

    private static (int l, int u)? FindBoundsOfIntersection((int l, int u) a, (int l, int u) b) {
        var minUpper = Math.Min(a.u, b.u);
        var maxLower = Math.Max(a.l, b.l);
        if (minUpper < maxLower) return null;
        return (maxLower, minUpper);
    }
    
    private static bool InfiniteSequencesOverlap(ISequence s1, ISequence s2) {
        var gcd = Gcd(s1.Interval, -s2.Interval);
        return (s2.Start - s1.Start) % gcd != 0;
    }
    
    public static int Gcd(int a, int b) {
        while (a != 0) {
            var temp = a;
            a = b % a;
            b = temp;
        }
        return b;
    }
    
    public static (int gcd, int x, int y) ExtendedGcd(int a, int b) {
        if (a == 0) 
            return (b, 0, 1);
        
        var (gcd, x1, y1) = ExtendedGcd(b % a, a);
        return (gcd, y1 - (b / a) * x1, x1);
    }
    
    public static int Floor(int a, int b) =>
        (a ^ b) < 0 ? a < 0 ? (a - b + 1) / b : (a - b -1) / b : a / b;
    
    public static int Ceil(int a, int b) =>
        (a ^ b) < 0 ? a / b : a < 0 ? (a + b + 1) / b : (a + b - 1) / b;
}