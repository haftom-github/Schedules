using System.Collections.Immutable;
using Core.Sequences;

namespace Core;

public record RSchedule {
    public ImmutableArray<RSlot> Slots { get; }
    public int Recurrence { get; }
    private Rotation Rotation { get; }
    public bool IsEmpty => Slots.IsEmpty;
    public int Start => Slots.Length > 0 ? Slots[0].Start : 0;
    public int End => Slots.Length > 0 ? Slots[^1].End : 0;
    public int OverallDuration => End - Start;

    public RSchedule(RSlot slot, int? recurrence = null, Rotation? rotation = null) 
        : this([slot], recurrence, rotation) { }

    public IEnumerable<LimitedRSchedule> OverlapsWithIn
        => Scatter().GetOverlapWithIn();
    
    public RSchedule(IEnumerable<RSlot>? slots = null, int? recurrence = null, Rotation? rotation = null) {
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

    public IEnumerable<RSlot> Availabilities(int from, int to) {
        var range = RSlot.FromRange(from, to);
        return Enumerable.Range(0, SequenceMath.Floor(to - Start - 1, Recurrence) + 1)
            .SelectMany(i => Slots.Select(s => s
                .Shift(i * Recurrence)
                .GetOverlapWith(range))
            ).Where(s => s.IsPositive);
    }

    public RSchedule Shift(int n)
        => new(
            Slots.Select(s => s.Shift(n)),
            Recurrence, 
            Rotation
        );

    public IEnumerable<LimitedRSchedule> Scatter() {
        var scattered = Rotation.OffsetsInRotationSpan
            .Select(o => new LimitedRSchedule(
                new RSchedule(
                    Slots, 
                    Recurrence * Rotation.RotationSpan, 
                    Rotation.NoRotation
                ),
                o
            ));
        return scattered;
    }
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

    public static IEnumerable<RSlot> GetOverlapWith(this IEnumerable<RSlot> slots, IEnumerable<RSlot> others) {
        var overlaps = new List<RSlot>();
        var slotsList = slots.ToList();
        var othersList = others.ToList();
        foreach (var slot in slotsList) {
            foreach (var other in othersList) {
                var overlap = slot.GetOverlapWith(other);
                if (overlap.IsPositive) overlaps.Add(overlap);
            }
        }

        return overlaps;
    }

    public static IEnumerable<LimitedRSchedule> GetOverlapWithIn(this IEnumerable<LimitedRSchedule> schedules) {
        var schedulesList = schedules.ToList();
        var overlaps = new List<LimitedRSchedule>();

        for (int i = 0; i < schedulesList.Count; i++) {
            for (int j = i + 1; j < schedulesList.Count; j++) {
                var overlapsIj = schedulesList[i].GetOverlapsWith(schedulesList[j]);
                overlaps.AddRange(overlapsIj);
            }
        }

        return overlaps;
    }

    public static LimitedRSchedule Limit(this RSchedule schedule, long start, long? end = null)
        => new(schedule, start, end);
}

