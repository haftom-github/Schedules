namespace Core.ValueObjects;

public sealed class Period(TimeOnly start, TimeOnly end) : IEquatable<Period> {
    public TimeOnly Start { get; } = start;
    public TimeOnly End { get; } = end;
    public TimeSpan Span { get; } = end - start;
    public bool IsPositive { get; } = start < end;
    
    public Period(TimeOnly start, TimeSpan span) : this(start, start.Add(span)){}

    public bool Equals(Period? other) {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Start.Equals(other.Start) && End.Equals(other.End);
    }

    public override bool Equals(object? obj) {
        return ReferenceEquals(this, obj) || obj is Period other && Equals(other);
    }

    public override int GetHashCode() {
        return HashCode.Combine(Start, End);
    }

    public static bool operator ==(Period? left, Period? right) {
        return Equals(left, right);
    }

    public static bool operator !=(Period? left, Period? right) {
        return !Equals(left, right);
    }
} 