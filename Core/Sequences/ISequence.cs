namespace Core.Sequences;

public interface ISequence {
    public int Start { get; }
    public int? End { get; }
    public int Interval { get; }
    public bool IsFinite { get; }
    public bool IsInfinite => !IsFinite;
    public int? Length { get; }
    public int S(int n);
    public ISequence? StartFromNext();
    public bool IsMember(int x);
    public bool IsNotMember(int x) => !IsMember(x);
}