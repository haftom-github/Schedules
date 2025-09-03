namespace Core.Sequences;

public class FiniteSequence : ISequence {
    public int Start { get; }
    public int? End { get; }
    public int Interval { get; }
    public bool IsFinite => true;
    public int? Length => SequenceMath.Floor(End!.Value - Start, Interval) + 1;
    public bool IsEmpty => Length < 1;

    public FiniteSequence(int start, int end, int interval = 1) {
        if (interval <= 0)
            throw new ArgumentOutOfRangeException(nameof(interval), "Interval must be a positive integer.");

        Start = start;
        End = end;
        Interval = interval;
    }
    
    public int S(int n) {
        if (n < 0)
            throw new ArgumentOutOfRangeException(nameof(n), "index must be greater than or equal to 0.");
        
        var maxN = (End - Start) / Interval;
        if (n > maxN)
            throw new ArgumentOutOfRangeException(nameof(n), $"index must be less than or equal to {maxN}.");
        
        return Start + n * Interval;
    }

    public ISequence StartFromIndex(int n) =>
        new FiniteSequence(Start + n * Interval, End!.Value, Interval);

    public bool IsMember(int x) =>
        x >= Start && x <= End && (x - Start) % Interval == 0;

    
    public ISequence CollapseToRangeOf(ISequence other) {
        // Start + Interval * n >= other.Start
        // n >= Ceil((other.Start - Start) / Interval)
        var n = SequenceMath.Ceil(other.Start - Start, Interval);
        var start = Math.Max(Start, Start + n * Interval);
        
        return new FiniteSequence(start, Math.Min(End!.Value, other.End ?? End!.Value + 1), Interval);
    }


    // equatable implementations
    public bool Equals(ISequence? other) {
        return Start == other?.Start && End == other.End && Interval == other.Interval;
    }

    public override bool Equals(object? obj) {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((FiniteSequence)obj);
    }

    public override int GetHashCode() {
        return HashCode.Combine(Start, End, Interval);
    }

    public static bool operator ==(FiniteSequence? left, FiniteSequence? right) {
        return Equals(left, right);
    }

    public static bool operator !=(FiniteSequence? left, FiniteSequence? right) {
        return !Equals(left, right);
    }
}