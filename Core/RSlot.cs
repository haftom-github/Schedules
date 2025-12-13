namespace Core;

public readonly record struct RSlot(int Start, int Duration) {
    public int End => Start + Duration;
    private int First => Duration >= 0 ? Start : End;
    private int Last => Duration >= 0 ? End : Last;
    private int Sign => Duration < 0 ? -1 : 1;
    public bool IsPositive => Duration > 0;
    public bool IsNegative => Duration < 0;
    public bool IsEmpty => Duration == 0;
    public static RSlot FromRange(int start, int end)
        => new(start,end - start);
    
    public RSlot GetOverlapWith(RSlot other) {
        var overlap = IsPositive
            ? FromRange(
                Math.Max(Start, other.First),
                Math.Min(End, other.Last))
            : FromRange(
                Math.Min(Start, other.Last),
                Math.Max(End, other.First));

        return Sign == overlap.Sign 
            ? overlap 
            : this with { Duration = 0 };
    }

    public bool CanBeMergedWith(RSlot other) {
        var overlap = IsPositive
            ? FromRange(
                Math.Max(Start, other.First),
                Math.Min(End, other.Last))
            : FromRange(
                Math.Min(Start, other.Last),
                Math.Max(End, other.First));
        
        return Sign == overlap.Sign || overlap.IsEmpty;
    }

    public RSlot Merge(RSlot other) 
         => IsPositive
            ? FromRange(
                Math.Min(Start, other.First),
                Math.Max(End, other.Last))
            : FromRange(
                Math.Max(Start, other.Last),
                Math.Min(End, other.First));
    
    public RSlot Shift(int x)
        => this with { Start = Start + x };
}
