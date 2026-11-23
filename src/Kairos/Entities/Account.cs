namespace Kairos.Entities;

using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

[Index(
    nameof(Issuer),
    nameof(Subject),
    IsUnique = true)]
public class Account
{
    public Guid Id { get; init; } = Guid.CreateVersion7();

    [MaxLength(200)]
    public string Issuer { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Subject { get; set; } = string.Empty;
    
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    
    public ICollection<Workspace> Workspaces { get; init; } = 
        new List<Workspace>();
}