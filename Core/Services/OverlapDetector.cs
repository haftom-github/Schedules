using System.Collections.Immutable;
using Core.Options;
using Core.Sequences;

namespace Core.Services;

public static class OverlapDetector {
    public static List<Schedule> OverlapScheduleWith(this Schedule schedule1, Schedule schedule2) {
        var ownSequences = schedule1.ToSequencesList();
        var otherSequences = schedule2.ToSequencesList();

        List<Schedule> overlapSchedules = [];
        foreach(var ownSequence in  ownSequences) {
            foreach (var otherSequence in otherSequences) {
                var overlapSeq = ownSequence.FindOverlapWith(otherSequence);
                if (overlapSeq is null || overlapSeq.IsEmpty)
                    continue;

                var ownSlots = ownSequence.Tag == "before"
                    ? schedule1.SlotsBeforeMidnight() : schedule1.SlotsAfterMidnight().Select(s => s.ShiftLeftByOneDay).ToList();
                
                var othersSlots = otherSequence.Tag == "before"
                    ? schedule2.SlotsBeforeMidnight() : schedule2.SlotsAfterMidnight().Select(s => s.ShiftLeftByOneDay).ToList();

                var overlaps = Overlaps(ownSlots, othersSlots);
                if (overlaps.All(s => s.IsEmpty)) continue;
                var startDate = DateOnly.FromDayNumber(overlapSeq.Start);
                DateOnly? endDate = overlapSeq.End != null 
                    ? DateOnly.FromDayNumber(overlapSeq.End!.Value) 
                    : null;
                
                var overlapSchedule = new Schedule(
                    overlaps.Where(o => !o.IsEmpty).ToList(),
                    startDate, endDate, Recurrence.Daily(overlapSeq.Interval));
                
                overlapSchedules.Add(overlapSchedule);
            }
        }

        return Merge(overlapSchedules);
    }

    private static List<Schedule> Merge(List<Schedule> schedules) {
        for (var i = 0; i < schedules.Count; i++) {
            
            for (var j = i + 1; j < schedules.Count; j++) {
                var shift = FindShiftWith(schedules[i], schedules[j]);
                if (shift is not 1) continue;
                
                var mergedSlots = schedules[i].Slices
                    .Concat(schedules[j].Slices.Select(s => s.ShiftRightByOneDay))
                    .ToList();
                
                if (mergedSlots.OverallSlotsSpan() > TimeSpan.FromHours(24))
                    continue;
                
                var newStartDate = schedules[i].StartDate;
                var newEndDate = schedules[i].EndDate;
                
                var merged = new Schedule(mergedSlots, newStartDate, newEndDate, schedules[i].Recurrence);
                schedules.RemoveAt(j);
                schedules[i] = merged;
                break;
            }
        }

        for (var i = 0; i < schedules.Count; i++) {
            if (schedules[i].Recurrence.Interval % 7 != 0) continue;
            var weeklyRecurrenceInterval = schedules[i].Recurrence.Interval / 7;
            schedules[i] = schedules[i] with {
                Recurrence = Recurrence.Weekly(
                    [schedules[i].StartDate.DayOfWeek], 
                    interval: weeklyRecurrenceInterval
                )
            };
            for (var j = i + 1; j < schedules.Count; j++) {
                if (schedules[j].Recurrence.Interval 
                    != schedules[i].Recurrence.Interval * 7) continue;
                
                if (schedules[j].Slices.Count != schedules[i].Slices.Count || 
                    schedules[j].Slices.Any(s => !schedules[i].Slices.Contains(s)))
                    continue;
                
                schedules[i] = schedules[i] with {
                    Recurrence = schedules[i].Recurrence with { 
                        DaysOfWeek = schedules[i].Recurrence.DaysOfWeek
                            .Append(schedules[j].StartDate.DayOfWeek)
                            .ToImmutableHashSet()
                    }
                };
                schedules.RemoveAt(j);
                j--;
            }
        }

        return schedules;
    }
    
    private static int? FindShiftWith(Schedule schedule1, Schedule schedule2) {
        if (schedule1.Recurrence.Interval 
            != schedule2.Recurrence.Interval) return null;

        var seq = schedule1.ToSequencesList().First(s => s.Tag == "before");
        var otherSeq = schedule2.ToSequencesList().First(s => s.Tag == "before");

        if (seq.IsFinite != otherSeq.IsFinite) return null;
        
        var shift = otherSeq.Start - seq.Start;
        if (otherSeq.End is null) return shift;
        if (otherSeq.End - seq.End != shift)
            return null;
        
        return shift;
    }
    
    private static List<Slot> Overlaps(List<Slot> slots1, List<Slot> slots2) 
        => slots1.SelectMany(s => s.Overlap(slots2)).ToList();
}

public static class ScheduleExtensions {
    private static List<Schedule> SplitOnDayBoundary(this Schedule schedule) {
        if (!schedule.CrossesDayBoundary)
            return [schedule];

        var beforeMidnight = schedule with {
            Slices = schedule.Slices
                .Select(s => s.BeforeMidnight)
                .Where(s => !s.IsEmpty)
                .ToList() 
        };

        var afterMidnight = new Schedule(
            slots: schedule.Slices
                .Select(s => s.AfterMidnight)
                .Where(s => !s.IsEmpty),
            startDate: schedule.StartDate.AddDays(1),
            endDate: schedule.EndDate?.AddDays(1),
            recurrence: schedule.Recurrence with {
                DaysOfWeek = schedule.Recurrence.DaysOfWeek
                    .Select(d => d.ToNextDayOfWeek())
                    .ToImmutableHashSet()
            });
        
        return [beforeMidnight, afterMidnight];
    }
    public static List<ISequence> ToSequencesList(this Schedule schedule) {
        var splits = SplitOnDayBoundary(schedule);
        string[] tags = ["before", "after"];
        List<ISequence> sequences = [];

        for (var i = 0; i < splits.Count; i++) {
            switch (splits[i].Recurrence.Type) {
                case RecurrenceType.Daily:
                    sequences.Add(
                        SequenceFactory.Create(
                            splits[i].StartDate.DayNumber,
                            splits[i].EndDate?.DayNumber,
                            splits[i].Recurrence.Interval, tags[i])
                    );
                    break;

                case RecurrenceType.Weekly:
                    if (splits[i].Recurrence.DaysOfWeek.Count == 0) break;
                    
                    foreach (var day in splits[i].Recurrence.DaysOfWeek)
                    {
                        var start = splits[i].StartDate.ToFirstDayOfWeek();
                        while (start.DayOfWeek != day)
                            start = start.AddDays(1);
                    
                        var sequence = SequenceFactory.Create(
                            start.DayNumber,
                            splits[i].EndDate?.DayNumber,
                            splits[i].Recurrence.Interval * 7,
                            tags[i]
                        );
                        if (sequence.Start < splits[i].StartDate.DayNumber)
                            sequence = sequence.StartFromIndex(1);

                        if (sequence.IsEmpty) continue;
                
                        sequences.Add(sequence);
                    }
                    break;
                
                default: throw new NotImplementedException();
            }
        }

        return sequences;
    }
    public static List<Slot> SlotsAtDate(this Schedule schedule, DateOnly date) {
        var sequences = schedule.ToSequencesList();
        var slots = new List<Slot>();

        foreach (var sequence in sequences) {
            if (sequence.IsMember(date.DayNumber)) {
                slots.AddRange(sequence.Tag == "before"
                    ? schedule.SlotsBeforeMidnight()
                    : schedule.SlotsAfterMidnight());
            }
        }

        return slots;
    }

    public static List<Slot> SlotsBeforeMidnight(this Schedule schedule) 
        => schedule.Slices.Select(s => s.BeforeMidnight).Where(s => !s.IsEmpty).ToList();

    public static List<Slot> SlotsAfterMidnight(this Schedule schedule) 
        => schedule.Slices.Select(s => s.AfterMidnight).Where(s => !s.IsEmpty).ToList();

    public static TimeSpan OverallSlotsSpan(this IEnumerable<Slot> slots) {
        var slotsList = slots.ToList();
        return (slotsList.MaxBy(s => s.EndSpan)?.EndSpan 
               - slotsList.MinBy(s => s.StartSpan)?.StartSpan)!.Value;
    }
}