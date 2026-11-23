namespace Kairos.Entities;

using System.ComponentModel.DataAnnotations;

public class Timer
{
    public Guid Id { get; init; } = Guid.CreateVersion7();

    [MaxLength(200)]
    public string? Name { get; init; }
    
    public Guid WorkspaceId { get; init; }
    
    public Workspace Workspace { get; init; } = null!;
    
    public ICollection<Tag> Tags { get; set; } =
        new List<Tag>();
}