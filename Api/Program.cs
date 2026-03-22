using Application.Interfaces;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Tells MediatR to scan the Application assembly to find all the Handlers
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(IAssetRepository).Assembly));

// Register EF Core DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register the national vulnerability db api
builder.Services.AddHttpClient("NvdApi", client =>
{
    client.BaseAddress = new Uri("https://services.nvd.nist.gov/rest/json/cves/2.0");
    // NVD API requires a User-Agent header
    client.DefaultRequestHeaders.Add("User-Agent", "DotnetVulnTracker"); 
});

// Register the background worker to sync the db
builder.Services.AddHostedService<Api.Workers.CveSyncWorker>();

// Register Repository (AddScoped = one instance per HTTP request)
builder.Services.AddScoped<IAssetRepository, AssetRepository>();

var app = builder.Build();

app.UseAuthorization();
app.MapControllers();

app.Run();
