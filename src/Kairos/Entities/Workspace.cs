namespace Kairos.Entities;

using System.ComponentModel.DataAnnotations;

public class Workspace
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    
    public Guid AccountId { get; set; }
    
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    public DateTimeOffset CreatedAt { get; init; } =
        DateTimeOffset.UtcNow;
    
    public Account Account { get; init; } = null!;
    
    public ICollection<Timer> Timers { get; init; } = 
        new List<Timer>();
    
    public ICollection<Tag> Tags { get; init; } = 
        new List<Tag>();
}