namespace Core.Services;

public static class SlotService {
    public static List<Slot> GenerateSlots(
        TimeSpan slotSpan, DateOnly date, List<Schedule> workSchedules, List<Schedule> blockedSchedules) {
        
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

        var availableSlots = GenerateInternal(slotSpan, workingSlots, blockedSlots);

        return availableSlots;
    }

    private static List<Slot> GenerateInternal(TimeSpan minSpan, List<Slot> working, List<Slot> blocking) {
        for (var i = 0; i < working.Count; i++) {
            foreach (var slot in blocking) {
                if (working[i].IsEmpty) break;
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
            if (slots[i].IsEmpty 
                || slots[i].Span < 2 * minSpan) 
                continue;
            
            slots.Insert(i+1, new Slot(slots[i].StartSpan + minSpan, slots[i].EndSpan));
            slots[i] = new Slot(slots[i].StartSpan, slots[i+1].StartSpan);
            
        }
        
        var filtered = slots.Where(p => !p.IsEmpty && p.Span >= minSpan).ToList();
        filtered.Sort((a, b) => a.StartSpan.CompareTo(b.StartSpan));
        return filtered;
    }

    private static (Slot orgi, Slot? biprod) Block(Slot working, Slot blocking) {
        var org = working with { EndSpan = Min(blocking.StartSpan, working.EndSpan) };
        var biProd = working with { StartSpan = Max(blocking.EndSpan, working.StartSpan) };

        if (org.IsEmpty) (org, biProd) = (biProd, org);
        if (biProd.IsEmpty) biProd = null;
        return (org, biProd);
    }
    
    private static TimeSpan Min(TimeSpan a, TimeSpan b) => a < b ? a : b;
    private static TimeSpan Max(TimeSpan a, TimeSpan b) => a > b ? a : b;
}