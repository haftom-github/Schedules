namespace Core.Sequences;

public interface ISequence {
    public int Start { get; }
    public int? End { get; }
    public int Interval { get; }
    public bool IsFinite { get; }
    public bool IsInfinite => !IsFinite;
    public int? Length { get; }
    public bool IsEmpty { get; }
    public int S(int n);
    public ISequence StartFromIndex(int n);
    public bool IsMember(int x);
    public ISequence CollapseToRangeOf(ISequence other);
    public string? Tag { get; }
}