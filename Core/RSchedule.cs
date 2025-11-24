using System.Collections.Immutable;

namespace Core;

public record RSchedule {
    public ImmutableArray<RSlot> Slots { get; }
    public long Recurrence { get; }
    public long RotationSpan { get; }
    public long OffsetInRotationSpan { get; }

    public RSchedule(IEnumerable<RSlot> slots, long recurrence, long rotationSpan = 1, long offsetInRotationSpan = 0) {

        var ordered = slots
            .Where(s => s.IsPositive)
            .OrderBy(slot => slot.Start)
            .ToList();

        if (ordered.Count == 0) {
            throw new ArgumentException("zero positive slots are provided", nameof(slots));
        }

        var start = ordered.First().Start;
        var end = ordered.Last().Start + ordered.Last().Duration;

        if (end - start > RotationSpan * Recurrence) {
            throw new ArgumentException("overall range can not be greater than recurrence", nameof(recurrence));
        }
        Slots = [..ordered];

        if (recurrence < 1) {
            throw new ArgumentOutOfRangeException(nameof(recurrence), recurrence, "recurrence span must be greater than zero");
        }
        Recurrence = recurrence;

        if (rotationSpan < 1) {
            throw new ArgumentOutOfRangeException(nameof(rotationSpan), rotationSpan, "rotation span must be greater than zero");
        }
        RotationSpan = rotationSpan;
        
        if (offsetInRotationSpan < 0) {
            throw new ArgumentOutOfRangeException(nameof(offsetInRotationSpan), offsetInRotationSpan, "offset must be greater than zero");
        }
        OffsetInRotationSpan = offsetInRotationSpan;
    }
}

public record LimitedRSchedule
(
    RSchedule RSchedule, 
    long Start, 
    long? End = null
);