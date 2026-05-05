using Application.Interfaces;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Application.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Add in allowed origins from the appsettings.json
var allowedOrigins = builder.Configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>();

// Add cors for the frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        // Blazor WASM local port
        policy.WithOrigins(allowedOrigins ?? ["http://localhost"]) 
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Tells MediatR to scan the Application assembly to find all the Handlers
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(IAssetRepository).Assembly));

// Register EF Core DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register the national vulnerability db api
builder.Services.AddHttpClient<NvdService>(client =>
{
    client.BaseAddress = new Uri("https://services.nvd.nist.gov/rest/json/cves/2.0");
    client.DefaultRequestHeaders.Add("User-Agent", "DotnetVulnTracker"); 
});

// Register the NVD service
builder.Services.AddScoped<INvdService, NvdService>();

// Register the background worker to sync the db
builder.Services.AddHostedService<Api.Workers.CveSyncWorker>();

// Register Repository (AddScoped = one instance per HTTP request)
builder.Services.AddScoped<IAssetRepository, AssetRepository>();

// Add endpoint explorer to discover minimal apis (identity core)
builder.Services.AddEndpointsApiExplorer();

// Add the swagger ui
builder.Services.AddSwaggerGen();

// Add Identity services and configure it to EF Core database
builder.Services.AddIdentityApiEndpoints<IdentityUser>()
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddAuthorization();
builder.Services.AddControllers();

var app = builder.Build();

// Initialize the database if not initialized, and updates the schema without deleting data
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // Applies pending migrations or creates the database if it doesn't exist
    dbContext.Database.Migrate(); 
}

// Middleware definition
app.UseSwagger();
app.UseSwaggerUI();

// Add in cors
//app.UseHttpsRedirection(); Include in production, in dev we have no SSL/TLS cert
app.UseCors("AllowBlazorClient");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapIdentityApi<IdentityUser>();

app.Run();
