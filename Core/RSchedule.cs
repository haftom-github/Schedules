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

    public RSchedule(RSlot slot, long? recurrence = null, Rotation? rotation = null) 
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

    public Rotation(long rotationSpan, ImmutableArray<long> offsetsInRotationSpan) {
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

    public static LimitedRSchedule Limit(this RSchedule schedule, long start, long? end = null)
        => new(schedule, start, end);
}

public record LimitedRSchedule(RSchedule Schedule, long Start, long? End = null) {
    public bool IsEmpty => Start < (End ?? long.MaxValue) || Schedule.IsEmpty;
}