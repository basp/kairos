using Kairos;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<KairosDbContext>(options =>
{
    var connectionString =
        builder
            .Configuration
            .GetConnectionString("DefaultConnection");
    
    options.UseSqlite(connectionString);
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    // options.SerializerOptions.Converters.Add
    
    options.SerializerOptions.ReferenceHandler = 
        System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();