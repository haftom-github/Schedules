namespace Core.Sequences;

public class FiniteSequence : ISequence {
    public int Start { get; }
    public int? End { get; }
    public int Interval { get; }
    public bool IsFinite => true;
    public int? Length => (End - Start) / Interval + 1;
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
}