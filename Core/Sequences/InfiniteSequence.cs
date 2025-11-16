namespace Core.Sequences;

public record InfiniteSequence : ISequence {
    public int Start { get; }
    public int? End => null;
    public int Interval { get; }
    public bool IsFinite => false;
    public int? Length => null;
    public bool IsEmpty => false;
    
    public string? Tag { get; }

    public InfiniteSequence(int start, int interval = 1, string? tag = null) {
        if (interval <= 0)
            throw new ArgumentOutOfRangeException(nameof(interval), "interval must be a positive integer.");
        
        Start = start;
        Interval = interval;
        Tag = tag;
    }

    public int S(int n) {
        if (n < 0)
            throw new ArgumentOutOfRangeException(nameof(n), "index must be a non-negative integer.");

        return Start + n * Interval;
    }

    public ISequence StartFromIndex(int n) =>
        new  InfiniteSequence(S(n), Interval);

    public bool IsMember(int x) => x >= Start && (x - Start) % Interval == 0;

    
    public ISequence CollapseToRangeOf(ISequence other) {
        // Start + Interval * n >= other.Start
        // n >= Ceil((other.Start - Start) / Interval)
        var n = SequenceMath.Ceil(other.Start - Start, Interval);
        var start = Math.Max(Start, Start + n * Interval);
        
        return SequenceFactory.Create(start, other.End, Interval);
    }
}