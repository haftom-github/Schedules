using System.Collections.Immutable;

namespace Core;

public record Rotation {
    public int RotationSpan { get; }
    public ImmutableArray<int> OffsetsInRotationSpan { get; }

    public Rotation(int rotationSpan, ImmutableArray<int> offsetsInRotationSpan) {
        if (rotationSpan <= 0)
            throw new ArgumentOutOfRangeException(nameof(rotationSpan), "Rotation span should be greater than zero");
        
        if (offsetsInRotationSpan.Any(o => o < 0 || o >= rotationSpan ))
            throw new ArgumentOutOfRangeException(nameof(offsetsInRotationSpan), $"Offsets must be between 0 and {rotationSpan}");
        
        RotationSpan = rotationSpan;
        OffsetsInRotationSpan = offsetsInRotationSpan;
    }

    public static Rotation NoRotation
        => new(1, [0]);
}