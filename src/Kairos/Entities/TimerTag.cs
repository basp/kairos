namespace Kairos.Entities;

public class TimerTag
{
    public Guid TimerId { get; init; }

    public Guid TagId { get; init; }
    
    public Timer Timer { get; init; } = null!;
    
    public Tag Tag { get; init; } = null!;
}