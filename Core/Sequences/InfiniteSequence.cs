namespace Core.Sequences;

public class InfiniteSequence : ISequence {
    public int Start { get; }
    public int? End => null;
    public int Interval { get; }
    public bool IsFinite => false;
    public int? Length => null;

    public InfiniteSequence(int start, int interval = 1) {
        if (interval <= 0)
            throw new ArgumentOutOfRangeException(nameof(interval), "Interval must be a positive integer.");
        
        Start = start;
        Interval = interval;
    }

    public int S(int n) => Start + n * Interval;
    public ISequence StartFromNext() {
        return new  InfiniteSequence(S(1), Interval);
    }

    public bool IsMember(int x) => x >= Start && (x - Start) % Interval == 0;
}