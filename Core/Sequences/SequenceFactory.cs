namespace Core.Sequences;

public static class SequenceFactory {
    public static ISequence Create(int start, int? end, int interval, string? tag = null) {
        return end switch {
            null => new InfiniteSequence(start, interval, tag:tag),
            _ => new FiniteSequence(start, end.Value, interval, tag:tag)
        };
    }
}