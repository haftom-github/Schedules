namespace Core.ValueObjects;

public sealed class Slot(TimeOnly? start = null, TimeOnly? end = null) : IEquatable<Slot> {
    public TimeOnly Start { get; } = start ?? TimeOnly.MinValue;
    public TimeOnly End { get; } = end ?? TimeOnly.MaxValue;
    public TimeSpan Span => Start - End;
    public bool IsPositive =>  Start < End;
    public bool IsFullDay => Start == TimeOnly.MinValue && End == TimeOnly.MaxValue;
    
    public Slot(TimeOnly start, TimeSpan span) : this(start, start.Add(span)){}

    public bool Equals(Slot? other) {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Start.Equals(other.Start) && End.Equals(other.End);
    }

    public override bool Equals(object? obj) {
        return ReferenceEquals(this, obj) || obj is Slot other && Equals(other);
    }

    public override int GetHashCode() {
        return HashCode.Combine(Start, End);
    }

    public static bool operator ==(Slot? left, Slot? right) {
        return Equals(left, right);
    }

    public static bool operator !=(Slot? left, Slot? right) {
        return !Equals(left, right);
    }
} 