using System.Collections.Immutable;

namespace Core;

public record RSchedule {
    public ImmutableArray<RSlot> Slots { get; }
    private long Recurrence { get; }
    private Rotation Rotation { get; }
    public bool IsEmpty => Slots.IsEmpty;
    public long Start => Slots.Length > 0 ? Slots[0].Start : 0;
    public long End => Slots.Length > 0 ? Slots[^1].End : 0;
    private long OverallDuration => End - Start;

    public RSchedule(RSlot slot, long recurrence = 1, Rotation? rotation = null) 
        : this([slot], recurrence, rotation) { }
    
    public RSchedule(IEnumerable<RSlot>? slots = null, long? recurrence = null, Rotation? rotation = null) {
        Slots = slots
            ?.Where(s => s.IsPositive)
            .OrderBy(slot => slot.Start)
            .Merge()
            .ToImmutableArray() ?? [];

        Rotation = rotation ?? Rotation.NoRotation;
        Recurrence = recurrence ?? OverallDuration;

        if (OverallDuration > Rotation.RotationSpan * Recurrence)
            throw new ArgumentException("overall range can not be greater than recurrence", nameof(recurrence));
    }
}

public record Rotation {
    public long RotationSpan { get; }
    public ImmutableArray<long> OffsetsInRotationSpan { get; }

    private Rotation(long rotationSpan, ImmutableArray<long> offsetsInRotationSpan) {
        RotationSpan = rotationSpan;
        OffsetsInRotationSpan = offsetsInRotationSpan;
    }

    public static Rotation NoRotation
        => new(1, [0]);
}

public static class Extensions {
    public static IEnumerable<RSlot> Merge(this IEnumerable<RSlot> slots) {
        var slotsList = slots.ToList();
        for (var i = 0; i < slotsList.Count; i++) {
            for (var j = i + 1; j < slotsList.Count; j++) {
                if (slotsList[i].CanBeMergedWith(slotsList[j])) {
                    slotsList[i] = slotsList[i].Merge(slotsList[j]);
                    slotsList.RemoveAt(j);
                    i--;
                    break;
                }
            }
        }

        return slotsList;
    }
}