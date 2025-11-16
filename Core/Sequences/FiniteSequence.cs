namespace Core.Sequences;

public record FiniteSequence : ISequence {
    public int Start { get; }
    public int? End { get; }
    public int Interval { get; }
    public bool IsFinite => true;
    public int? Length => SequenceMath.Floor(End!.Value - Start, Interval) + 1;
    public bool IsEmpty => Length < 1;
    public string? Tag { get; }

    public FiniteSequence(int start, int end, int interval = 1, string? tag = null) {
        if (interval <= 0)
            throw new ArgumentOutOfRangeException(nameof(interval), "Interval must be a positive integer.");

        Start = start;
        End = end;
        Interval = interval;
        Tag = tag;
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
}