using Core.Entities;
using Core.Sequences;

namespace Core.Overlap;

public interface IOverlapDetector {
    public bool IsOverlapping(Schedule s1, Schedule s2);
    public ISequence? Detect(Schedule s1, Schedule s2);
}