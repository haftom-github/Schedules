namespace Core.Sequences;

public static class SequenceFactory {
    public static ISequence Create(int start, int? end, int interval) {
        return end switch {
            null => new InfiniteSequence(start, interval),
            _ => new FiniteSequence(start, end.Value, interval)
        };
    }
}