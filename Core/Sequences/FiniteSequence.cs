namespace Core.Sequences;

public class FiniteSequence : ISequence {
    public int Start { get; }
    public int? End { get; }
    public int Interval { get; }
    public bool IsFinite => true;
    public int? Length => (End - Start) / Interval + 1;

    public FiniteSequence(int start, int end, int interval = 1) {
        if (interval <= 0)
            throw new ArgumentOutOfRangeException(nameof(interval), "Interval must be a positive integer.");
        
        if (end < start) 
            throw new ArgumentOutOfRangeException(nameof(end), "End must be greater than start value.");
        
        var effectiveEnd = start + ((end - start) / interval) * interval;
        if (start > effectiveEnd)
            throw new ArgumentOutOfRangeException(nameof(start),$"Start must be less than effective end ({effectiveEnd}).");

        Start = start;
        End = effectiveEnd;
        Interval = interval;
    }
    
    public int S(int n) {
        var maxN = (End - Start) / Interval;
        if (n > maxN)
            throw new ArgumentOutOfRangeException(nameof(n), $"n must be less than or equal to {maxN}.");
        
        return Start + n * Interval;
    }

    public ISequence? StartFromNext() {
        return Length == 1 ? null : new FiniteSequence(S(1), End!.Value, Interval);
    }

    public bool IsMember(int x) =>
        x >= Start && x <= End && (x - Start) % Interval == 0;

    public static FiniteSequence SingleElement(int start) => new(start, start);
}