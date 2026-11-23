namespace Kairos.Entities;

using System.ComponentModel.DataAnnotations;

public class Timer
{
    public Guid Id { get; init; } = Guid.CreateVersion7();

    [MaxLength(200)]
    public string? Name { get; init; }
    
    public Guid WorkspaceId { get; init; }
    
    public DateTimeOffset StartedAtUtc { get; init; } = DateTimeOffset.UtcNow;
    
    public DateTimeOffset? StoppedAtUtc { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    
    public TimeSpan? Duration => 
        this.StoppedAtUtc.HasValue 
            ? this.StoppedAtUtc.Value - this.StartedAtUtc 
            : null;
    
    public Workspace Workspace { get; init; } = null!;
    
    public ICollection<Tag> Tags { get; init; } =
        new List<Tag>();
}