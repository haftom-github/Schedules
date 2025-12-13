namespace Core.Sequences;

public static class SequenceMath {

    public static bool OverlapsWith(this ISequence s1, ISequence s2) 
        => s1.FindOverlapWith(s2) is { IsEmpty: false };
    
    public static ISequence? FindOverlapWith(this ISequence s1, ISequence s2) {
        return GetSequenceOfOverlaps(
            new InfiniteSequence(s1.Start, s1.Interval),
            new InfiniteSequence(s2.Start, s2.Interval))
            ?.CollapseToRangeOf(s1)
            .CollapseToRangeOf(s2);
    }

    private static InfiniteSequence? GetSequenceOfOverlaps(InfiniteSequence s1, InfiniteSequence s2) {
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
