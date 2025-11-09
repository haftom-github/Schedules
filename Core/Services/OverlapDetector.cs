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
                    ? schedule1.SlotsBeforeMidnight() : schedule1.SlotsAfterMidnight();
                
                var othersSlots = otherSequence.Tag == "before"
                    ? schedule2.SlotsBeforeMidnight() : schedule2.SlotsAfterMidnight();

                var overlaps = Overlaps(ownSlots, othersSlots);
                if (overlaps.All(s => s.IsEmpty)) continue;
                var startDate = DateOnly.FromDayNumber(overlapSeq.Start);
                DateOnly? endDate = overlapSeq.End != null 
                    ? DateOnly.FromDayNumber(overlapSeq.End!.Value) 
                    : null;
                
                var overlapSchedule = new Schedule(
                    overlaps.Where(o => !o.IsEmpty).ToList(),
                    startDate, endDate);
                
                overlapSchedule.UpdateRecurrence(interval: overlapSeq.Interval);
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
                
                var mergedSlots = schedules[i].Slots.Concat(schedules[j].Slots).ToList();
                if (mergedSlots.OverallSlotsSpan() > TimeSpan.FromHours(24))
                    continue;
                
                var newStartDate = schedules[i].StartDate;
                var newEndDate = schedules[i].EndDate;
                
                var merged = new Schedule(mergedSlots, newStartDate, newEndDate);
                merged.UpdateRecurrence(interval: schedules[i].RecurrenceInterval);
                schedules.RemoveAt(j);
                schedules[i] = merged;
                break;
            }
        }

        for (var i = 0; i < schedules.Count; i++) {
            if (schedules[i].RecurrenceInterval % 7 != 0) continue;
            var weeklyRecurrenceInterval = schedules[i].RecurrenceInterval / 7;
            schedules[i].UpdateRecurrence(RecurrenceType.Weekly, daysOfWeek: [schedules[i].StartDate.DayOfWeek], interval: weeklyRecurrenceInterval);
            for (var j = i + 1; j < schedules.Count; j++) {
                if (schedules[j].RecurrenceInterval 
                    != schedules[i].RecurrenceInterval * 7) continue;
                
                if (schedules[j].Slots.Count != schedules[i].Slots.Count || 
                    schedules[j].Slots.Any(s => !schedules[i].Slots.Contains(s)))
                    continue;
                
                schedules[i].DaysOfWeek.Add(schedules[j].StartDate.DayOfWeek);
                schedules.RemoveAt(j);
                j--;
            }
        }

        return schedules;
    }
    
    private static int? FindShiftWith(Schedule schedule1, Schedule schedule2) {
        if (schedule1.RecurrenceInterval 
            != schedule2.RecurrenceInterval) return null;

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

        var beforeMidnight = new Schedule(
            schedule.Slots
                .Select(s => s.BeforeMidnight)
                .Where(s => !s.IsEmpty),
            schedule.StartDate, 
            schedule.EndDate
        );
        
        beforeMidnight.UpdateRecurrence(
            type: schedule.RecurrenceType, 
            daysOfWeek: schedule.DaysOfWeek, 
            interval: schedule.RecurrenceInterval
        );

        var afterMidnight = new Schedule(
            schedule.Slots
                .Select(s => s.AfterMidnight)
                .Where(s => !s.IsEmpty),
            schedule.StartDate.AddDays(1),
            schedule.EndDate?.AddDays(1)
        );

        var shiftedDaysOfWeek = schedule.DaysOfWeek
            .Select(d => d.ToNextDayOfWeek())
            .ToHashSet();
        
        afterMidnight.UpdateRecurrence(
            type: schedule.RecurrenceType, 
            daysOfWeek: shiftedDaysOfWeek, 
            interval: schedule.RecurrenceInterval
        );
        
        return [beforeMidnight, afterMidnight];
    }
    public static List<ISequence> ToSequencesList(this Schedule schedule) {
        var splits = SplitOnDayBoundary(schedule);
        string[] tags = ["before", "after"];
        List<ISequence> sequences = [];

        for (var i = 0; i < splits.Count; i++) {
            switch (splits[i].RecurrenceType) {
                case RecurrenceType.Daily:
                    sequences.Add(
                        SequenceFactory.Create(
                            splits[i].StartDate.DayNumber,
                            splits[i].EndDate?.DayNumber,
                            splits[i].RecurrenceInterval, tags[i])
                    );
                    break;

                case RecurrenceType.Weekly:
                    if (splits[i].DaysOfWeek.Count == 0) break;
                    
                    foreach (var day in splits[i].DaysOfWeek)
                    {
                        var start = splits[i].StartDate.ToFirstDayOfWeek();
                        while (start.DayOfWeek != day)
                            start = start.AddDays(1);
                    
                        var sequence = SequenceFactory.Create(
                            start.DayNumber,
                            splits[i].EndDate?.DayNumber,
                            splits[i].RecurrenceInterval * 7,
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
        var periods = new List<Slot>();

        foreach (var sequence in sequences) {
            if (sequence.IsMember(date.DayNumber)) {
                periods.AddRange(sequence.Tag == "before"
                    ? schedule.SlotsBeforeMidnight()
                    : schedule.SlotsAfterMidnight());
            }
        }

        return periods;
    }

    public static List<Slot> SlotsBeforeMidnight(this Schedule schedule) 
        => schedule.Slots.Select(s => s.BeforeMidnight).Where(s => !s.IsEmpty).ToList();

    public static List<Slot> SlotsAfterMidnight(this Schedule schedule) 
        => schedule.Slots.Select(s => s.AfterMidnight).Where(s => !s.IsEmpty).ToList();

    public static TimeSpan OverallSlotsSpan(this IEnumerable<Slot> slots) {
        var slotsList = slots.ToList();
        return (slotsList.MaxBy(s => s.EndSpan)?.EndSpan 
               - slotsList.MinBy(s => s.StartSpan)?.StartSpan)!.Value;
    }
}