using JetBrains.Annotations;
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

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<KairosDbContext>();
    await db.Database.ExecuteSqlRawAsync("PRAGMA journal_mode = WAL;");
}

// NOTE: For development purposes, not part of the public API.
app.MapPost(
    "/accounts",
    async (KairosDbContext db) =>
    {
        var resource = new Account
        {
            Issuer = "dev-kairos",
            Subject = "dev-kairos",
            CreatedAt = DateTimeOffset.UtcNow,
        };

        await db.Accounts.AddAsync(resource);
        await db.SaveChangesAsync();

        var response = new
        {
            id = resource.Id,
            iss = resource.Issuer,
            sub = resource.Subject,
            created_at = resource.CreatedAt,
        };

        return Results.Ok(response);
    });

// NOTE: For development purposes, not part of the public API.
app.MapGet(
    "/accounts",
    async (KairosDbContext db) =>
    {
        var accounts = await db
            .Accounts
            .AsNoTracking()
            .ToListAsync();

        var response = accounts
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
    async (LinkGenerator links, KairosDbContext db, CreateWorkspace request) =>
    {
        var account = await db
            .Accounts
            .FirstOrDefaultAsync();

        if (account is null)
        {
            return Results.NotFound();
        }

        var workspace = new Workspace
        {
            Name = request.Name,
            AccountId = account.Id,
        };

        await db.Workspaces.AddAsync(workspace);
        await db.SaveChangesAsync();

        var response = new
        {
            id = workspace.Id,
            name = workspace.Name,
            created_at = workspace.CreatedAt,
        };

        var uri = links.GetPathByName(
            "GetWorkspace", new
            {
                workspaceId = workspace.Id,
            });

        return Results.Created(
            uri,
            response);
    });

app.MapGet(
        "/workspaces",
        async (KairosDbContext db) =>
        {
            var account = await db
                .Accounts
                .Include(x => x.Workspaces)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (account is null)
            {
                return Results.NotFound();
            }

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
        })
    .WithName("ListWorkspaces");

app.MapGet(
        "/workspaces/{workspaceId:guid}",
        async (KairosDbContext db, Guid workspaceId) =>
        {
            var response = await db
                .Workspaces
                .Where(x => x.Id == workspaceId)
                .Include(x => x.Timers)
                .AsNoTracking()
                .Select(x => new
                {
                    id = x.Id,
                    name = x.Name,
                    created_at = x.CreatedAt
                })
                .FirstOrDefaultAsync();

            return response is null
                ? Results.NotFound()
                : Results.Ok(response);
        })
    .WithName("GetWorkspace");

app.MapPost(
    "/workspaces/{workspaceId:guid}/timers",
    async (
        LinkGenerator links,
        KairosDbContext db,
        CreateTimer request,
        Guid workspaceId) =>
    {
        var timer = new Timer
        {
            StartedAtUtc = DateTimeOffset.UtcNow,
            Name = request.Name,
            WorkspaceId = workspaceId,
        };

        await db.Timers.AddAsync(timer);
        await db.SaveChangesAsync();

        var response = new
        {
            id = timer.Id,
            name = timer.Name,
            workspace_id = timer.WorkspaceId,
            duration = timer.Duration,
            started_at = timer.StartedAtUtc,
            created_at = timer.CreatedAt,
        };

        var uri = links.GetPathByName(
            "GetTimer",
            new
            {
                workspaceId,
                timerId = timer.Id
            });

        return Results.Created(uri, response);
    });

app.MapGet(
        "/workspaces/{workspaceId:guid}/timers",
        async (KairosDbContext db, Guid workspaceId) =>
        {
            var response = await db
                .Timers
                .Where(x => x.WorkspaceId == workspaceId)
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
                .ToListAsync();

            return Results.Ok(response);
        })
    .WithName("ListTimers");

app.MapGet(
        "/workspaces/{workspaceId:guid}/timers/{timerId:guid}",
        async (KairosDbContext db, Guid workspaceId, Guid timerId) =>
        {
            var timer = await db
                .Timers
                .FirstOrDefaultAsync(x =>
                    x.Id == timerId &&
                    x.WorkspaceId == workspaceId);

            if (timer is null)
            {
                return Results.NotFound();
            }

            var response = new
            {
                id = timer.Id,
                created_at = timer.CreatedAt,
                started_at = timer.StartedAtUtc,
                duration = timer.Duration,
                workspace_id = timer.WorkspaceId,
            };

            return Results.Ok(response);
        })
    .WithName("GetTimer");

app.MapPost(
    "/workspaces/{workspaceId:guid}/timers/{timerId:guid}/start",
    async (
        KairosDbContext db,
        Guid workspaceId,
        Guid timerId) =>
    {
        var timer = await db
            .Timers
            .FirstOrDefaultAsync(x => x.Id == timerId && x.WorkspaceId == workspaceId);

        if (timer is null)
        {
            return Results.NotFound();
        }

        timer.StoppedAtUtc = null;
        await db.SaveChangesAsync();

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
    async (
        KairosDbContext db,
        Guid workspaceId,
        Guid timerId) =>
    {
        var timer = await db
            .Timers
            .FirstOrDefaultAsync(x => x.Id == timerId && x.WorkspaceId == workspaceId);

        if (timer is null)
        {
            return Results.NotFound();
        }

        if (!timer.StoppedAtUtc.HasValue)
        {
            timer.StoppedAtUtc = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
        }

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

[UsedImplicitly]
internal sealed record CreateWorkspace(
    string Name);

[UsedImplicitly]
internal sealed record CreateTimer(
    string? Name);