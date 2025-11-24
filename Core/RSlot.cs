namespace Core;

public readonly record struct RSlot(int Start, int Duration) {
    private int End => Start + Duration;
    private int First => Duration >= 0 ? Start : End;
    private int Last => Duration >= 0 ? End : Last;
    private int Sign => Duration < 0 ? -1 : 1;
    public bool IsPositive => Duration > 0;
    public bool IsNegative => Duration < 0;
    public bool IsEmpty => Duration == 0;
    private static RSlot FromRange(int start, int end)
        => new(start,end - start);
    
    public RSlot GetOverlapWith(RSlot other) {
        var overlap = IsPositive
            ? FromRange(
                Math.Max(Start, other.First),
                Math.Min(Last, other.End))
            : FromRange(
                Math.Min(Start, other.Last),
                Math.Max(End, other.First));

        return Sign == overlap.Sign 
            ? overlap 
            : this with { Duration = 0 };
    }
}
