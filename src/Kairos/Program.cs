using Kairos;
using Kairos.Entities;
using Microsoft.EntityFrameworkCore;
using Timer = Kairos.Entities.Timer;

const string DefaultConnection = "DefaultConnection";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<KairosDbContext>(options =>
{
    var connectionString =
        builder
            .Configuration
            .GetConnectionString(DefaultConnection);

    options.UseSqlite(connectionString);
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options
        .SerializerOptions
        .Converters
        .Add(new GuidJsonConverter());
});

var app = builder.Build();

// NOTE: For development purposes, not pat of the public API.
app.MapPost(
    "/accounts",
    (KairosDbContext db) =>
    {
        var resource = new Account
        {
            Issuer = "dev-kairos",
            Subject = "dev-kairos",
            CreatedAt = DateTimeOffset.UtcNow,
        };
        
        db.Accounts.Add(resource);
        db.SaveChanges();

        var response = new
        {
            id = resource.Id,
            iss = resource.Issuer,
            sub = resource.Subject,
            created_at = resource.CreatedAt,
        };
        
        return Results.Ok(response);
    });

// NOTE: For development purposes, not pat of the public API.
app.MapGet(
    "/accounts",
    (KairosDbContext db) =>
    {
        var response = db
            .Accounts
            .AsNoTracking()
            .ToList()
            .Select(x => new
            {
                id = x.Id,
                iss = x.Issuer,
                sub = x.Subject,
                created_at = x.CreatedAt,
            })
            .ToArray();
        
        return Results.Ok(response);
    });

app.MapPost(
    "/workspaces",
    (KairosDbContext db, CreateWorkspace request) =>
    {
        var account = db
            .Accounts
            .FirstOrDefault();
        
        var workspace = new Workspace
        {
            Name = request.Name,
            AccountId = account!.Id,
        };

        db.Workspaces.Add(workspace);
        db.SaveChanges();

        var response = new
        {
            id = workspace.Id,
            name = workspace.Name,
            created_at = workspace.CreatedAt,
        };
        
        return Results.Ok(response);
    });

app.MapGet(
    "/workspaces",
    (KairosDbContext db) =>
    {
        var account = db
            .Accounts
            .Include(
                x => x.Workspaces)
            .AsNoTracking()
            .First();

        var response = account
            .Workspaces
            .Select(x => new
            {
                id = x.Id,
                name = x.Name,
                created_at = x.CreatedAt,
            })
            .ToArray();
        
        return Results.Ok(response);
    });

app.MapGet(
    "/workspaces/{workspaceId:guid}",
    (KairosDbContext db, Guid workspaceId) =>
    {
        var response = db
            .Workspaces
            .Where(x => x.Id == workspaceId)
            .Include(x => x.Timers)
            .AsNoTracking()
            .Select(x => new
            {
                id = x.Id,
                name = x.Name,
                created_at = x.CreatedAt,
                timers = x.Timers.Select(y => new
                {
                    id = y.Id,
                    name = y.Name,
                    started_at = y.StartedAtUtc,
                    stopped_at = y.StoppedAtUtc,
                    duration = y.Duration,
                    created_at = y.CreatedAt,
                }),
            })
            .First();
        
        return Results.Ok(response);
    });

app.MapPost(
    "/workspaces/{workspaceId:guid}/timers",
    (KairosDbContext db, CreateTimer request, Guid workspaceId) =>
    {
        var timer = new Timer
        {
            StartedAtUtc = DateTimeOffset.UtcNow,
            Name = request.Name,
            WorkspaceId = workspaceId,
        };

        db.Timers.Add(timer);
        db.SaveChanges();

        var response = new
        {
            id = timer.Id,
            name = timer.Name,
            workspace_id = timer.WorkspaceId,
            duration = timer.Duration,
            started_at = timer.StartedAtUtc,
            created_at = timer.CreatedAt,
        };
        
        return Results.Ok(response);
    });

app.MapGet(
    "/workspaces/{workspaceId:guid}/timers",
    (KairosDbContext db, Guid workspaceId) =>
    {
        var response = db
            .Timers
            .Where(x => x.WorkspaceId == workspaceId)
            .ToList()
            .Select(x => new
            {
                id = x.Id,
                name = x.Name,
                started_at = x.StartedAtUtc,
                stopped_at = x.StoppedAtUtc,
                created_at = x.CreatedAt,
                duration = x.Duration,
                workspace_id = x.WorkspaceId,
            })
            .ToArray();

        return Results.Ok(response);
    });

app.MapPost(
    "/workspaces/{workspaceId:guid}/timers/{timerId:guid}/start",
    (KairosDbContext db, Guid workspaceId, Guid timerId) =>
    {
        var timer = db
            .Timers
            .First(x => x.Id == timerId && x.WorkspaceId == workspaceId);

        timer.StoppedAtUtc = null;
        db.SaveChanges();
        
        var response = new
        {
            id = timer.Id,
            created_at = timer.CreatedAt,
            started_at = timer.StartedAtUtc,
            duration = timer.Duration,
            workspace_id = timer.WorkspaceId,
        };

        return Results.Ok(response);
    });

app.MapPost(
    "/workspaces/{workspaceId:guid}/timers/{timerId:guid}/stop",
    (KairosDbContext db, Guid workspaceId, Guid timerId) =>
    {
        var timer = db
            .Timers
            .First(x => x.Id == timerId && x.WorkspaceId == workspaceId);

        timer.StoppedAtUtc = DateTimeOffset.UtcNow;
        db.SaveChanges();

        var response = new
        {
            id = timer.Id,
            started_at = timer.StartedAtUtc,
            stopped_at = timer.StoppedAtUtc,
            created_at = timer.CreatedAt,
            duration = timer.Duration,
        };

        return Results.Ok(response);
    });

app.Run();

internal sealed record CreateWorkspace(
    string Name);
    
internal sealed record CreateTimer(
    string? Name);