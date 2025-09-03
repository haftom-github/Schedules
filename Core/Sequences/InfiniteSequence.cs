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

    
    public ISequence CollapseToRangeOf(ISequence other) {
        // Start + Interval * n >= other.Start
        // n >= Ceil((other.Start - Start) / Interval)
        var n = SequenceMath.Ceil(other.Start - Start, Interval);
        var start = Math.Max(Start, Start + n * Interval);
        
        return SequenceFactory.Create(start, other.End, Interval);
    }
    
    
    
    // equatable implementation
    public bool Equals(ISequence? other) {
        return Start == other?.Start && Interval == other.Interval;
    }

    public override bool Equals(object? obj) {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((InfiniteSequence)obj);
    }

    public override int GetHashCode() {
        return HashCode.Combine(Start, Interval);
    }

    public static bool operator ==(InfiniteSequence? left, InfiniteSequence? right) {
        return Equals(left, right);
    }

    public static bool operator !=(InfiniteSequence? left, InfiniteSequence? right) {
        return !Equals(left, right);
    }
}