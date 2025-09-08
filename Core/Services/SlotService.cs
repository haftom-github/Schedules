using Core.Entities;
using Core.Options;
using Core.Sequences;
using Core.ValueObjects;

namespace Core.Services;

public static class SlotService {
    public static List<Slot> GenerateSlots(
        TimeSpan slotSpan, DateOnly date, List<WorkSchedule> workSchedules, List<BlockedSchedule> blockedSchedules) {
        
        List<Slot> workingSlots = [];
        List<Slot> blockedSlots = [];

        foreach (var workSchedule in workSchedules) {
            var slots = workSchedule.SlotsAtDate(date);
            workingSlots.AddRange(slots);
        }

        foreach (var blockedSchedule in blockedSchedules) {
            var slots = blockedSchedule.SlotsAtDate(date);
            blockedSlots.AddRange(slots);
        }

        var availableSlots = Generate(slotSpan, workingSlots, blockedSlots);

        return availableSlots;
    }

    private static List<Slot> Generate(TimeSpan minSpan, List<Slot> working, List<Slot> blocking) {
        for (var i = 0; i < working.Count; i++) {
            foreach (var slot in blocking) {
                if (!working[i].IsPositive) break;
                var (org, biProd) = Block(working[i], slot);
                working[i] = org;
                if(biProd != null) working.Insert(i+1, biProd);
            }
        }

        return SliceFilterAndSort(minSpan, working);
    }

    private static List<Slot> SliceFilterAndSort(TimeSpan minSpan, List<Slot> slots) {

        // slice each slot to a minimum slotSpan
        for (var i = 0; i < slots.Count; i++) {
            if (!slots[i].IsPositive 
                || slots[i].Span < 2 * minSpan) 
                continue;
            
            slots.Insert(i+1, new Slot(slots[i].Start.Add(minSpan), slots[i].Span - minSpan));
            slots[i] = new Slot(slots[i].Start, minSpan);
            
        }
        
        var filtered = slots.Where(p => p.IsPositive && p.Span >= minSpan).ToList();
        filtered.Sort((a, b) => a.Start.CompareTo(b.Start));
        return filtered;
    }

    private static (Slot orgi, Slot? biprod) Block(Slot working, Slot blocking) {
        var org = new Slot(working.Start, Min(blocking.Start, working.End));
        var biProd = new Slot(Max(blocking.End, working.Start), working.End);

        if (!org.IsPositive) (org, biProd) = (biProd, org);
        if (!biProd.IsPositive) biProd = null;
        return (org, biProd);
    }

    private static List<ISequence> ToSequenceList(Schedule schedule) {
        switch (schedule.RecurrenceType) {
            case RecurrenceType.Daily:
                return [
                    SequenceFactory.Create(schedule.StartDate.DayNumber, schedule.EndDate?.DayNumber,
                        schedule.RecurrenceInterval)
                ];
            
            case RecurrenceType.Weekly:
                List<ISequence> sequences = [];
                var start = schedule.StartDate.ToFirstDayOfWeek();
                foreach (var day in schedule.DaysOfWeek) {
                    while (start.DayOfWeek != day) start = start.AddDays(1);
                    var sequence = SequenceFactory.Create(start.DayNumber, schedule.EndDate?.DayNumber, schedule.RecurrenceInterval*7);
                    if (sequence.Start < schedule.StartDate.DayNumber) sequence = sequence.StartFromIndex(1);
                    if (sequence != null)
                        sequences.Add(sequence);
                }
                return sequences;
            
            default:
                throw new NotImplementedException();
        }
    }
    
    private static TimeOnly Min(TimeOnly a, TimeOnly b) => a < b ? a : b;
    private static TimeOnly Max(TimeOnly a, TimeOnly b) => a > b ? a : b;
}