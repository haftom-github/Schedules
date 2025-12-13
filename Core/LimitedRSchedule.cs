using Core.Sequences;

namespace Core;

public record LimitedRSchedule(RSchedule Schedule, long Start, long? End = null) {
    // private bool IsEmpty => Start >= (End ?? long.MaxValue) || Schedule.IsEmpty;

    private long StartAt(int x)
        => Start + Schedule.Start + Schedule.Recurrence * x;

    private long EndAt(int x)
        => StartAt(x) + Schedule.OverallDuration;
    
    private IEnumerable<RSlot> Availabilities(long from, long to) {
        return Schedule.Availabilities((int)(from - Start), (int)(to - Start));
    }

    private LimitedRSchedule Limit(long from, long? to = null) {

        var maxStart = Math.Max(from, Start);
        var minEnd = to == null 
            ? End 
            : Math.Min(to.Value, End ?? to.Value);

        var x = (int)((maxStart - Start) / Schedule.Recurrence);
        var shiftBy = (-(int)(maxStart - Start) % Schedule.Recurrence) +
                      (EndAt(x) - maxStart >= 0 ? 0 : Schedule.Recurrence);
        
        return new LimitedRSchedule(
            Schedule.Shift(shiftBy),
            maxStart, minEnd
        );
    }

    public IEnumerable<LimitedRSchedule> GetOverlapsWith(LimitedRSchedule other) {
        var (first, second) 
            = other.Schedule.OverallDuration > Schedule.OverallDuration 
            ? (other.Limit(Start, End), Limit(other.Start, other.End)) 
            : (Limit(other.Start, other.End), other.Limit(Start, End));
        
        var x = first.StartAt(0);
        var y = first.EndAt(0);

        var u = second.StartAt(0);
        var v = second.EndAt(0);

        var deltaStart = u - y + 1;
        var deltaEnd = v - x;
        
        var abNot = SequenceMath.ExtendedGcd(first.Schedule.Recurrence, second.Schedule.Recurrence);

        var deltas = Enumerable
            .Range((int) deltaStart, (int)(deltaEnd - deltaStart))
            .Where(d => d % abNot.gcd == 0);

        var overlaps = new List<LimitedRSchedule>();
        foreach (var delta in deltas) {
            var x0 = delta / abNot.gcd * abNot.x;
            var t0 = SequenceMath.Ceil(-x0, second.Schedule.Recurrence / abNot.gcd);
            var xt0 = x0 + second.Schedule.Recurrence / abNot.gcd * t0;
            var from = first.StartAt(xt0);
            var to = first.EndAt(xt0);
            var overlap = first.Availabilities(from, to)
                .GetOverlapWith(second.Availabilities(from, to));
            
            var rs = second.Schedule.Recurrence / abNot.gcd * first.Schedule.Recurrence;
            var overlappingSchedule = first.Limit(from) with { Schedule = new RSchedule(overlap, rs) };
            
            overlaps.Add(overlappingSchedule);
        }

        return overlaps;
    }
}