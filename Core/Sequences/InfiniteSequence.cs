namespace Core.Sequences;

public class InfiniteSequence : ISequence {
    public int Start { get; }
    public int? End => null;
    public int Interval { get; }
    public bool IsFinite => false;
    public int? Length => null;
    public bool IsEmpty => false;

    public InfiniteSequence(int start, int interval = 1) {
        if (interval <= 0)
            throw new ArgumentOutOfRangeException(nameof(interval), "interval must be a positive integer.");
        
        Start = start;
        Interval = interval;
    }

    public int S(int n) {
        if (n < 0)
            throw new ArgumentOutOfRangeException(nameof(n), "index must be a non-negative integer.");

        return Start + n * Interval;
    }

    public ISequence StartFromIndex(int n) =>
        new  InfiniteSequence(S(n), Interval);

    public bool IsMember(int x) => x >= Start && (x - Start) % Interval == 0;
}