namespace Kairos.Entities;

using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

[Index(
    nameof(WorkspaceId), 
    nameof(Name),
    IsUnique = true)]
public class Tag
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    
    public Guid WorkspaceId { get; init; }

    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public Workspace Workspace { get; set; } = null!;
    
    public ICollection<Timer> Timers { get; set; } =
        new List<Timer>();
}