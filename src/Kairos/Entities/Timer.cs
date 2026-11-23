namespace Kairos.Entities;

using System.ComponentModel.DataAnnotations;

public class Timer
{
    public Guid Id { get; init; } = Guid.CreateVersion7();

    [MaxLength(200)]
    public string? Name { get; set; }
    
    public Guid WorkspaceId { get; init; }
    
    public Workspace Workspace { get; init; } = null!;
    
    public DateTimeOffset StartedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    
    public DateTimeOffset? EndedAtUtc { get; set; }
    
    public TimeSpan? Duration => 
        this.EndedAtUtc.HasValue 
            ? this.EndedAtUtc.Value - this.StartedAtUtc 
            : null;
    
    public ICollection<Tag> Tags { get; init; } =
        new List<Tag>();
}