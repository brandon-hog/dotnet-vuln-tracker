using Application.Interfaces;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Register EF Core DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Repository (AddScoped = one instance per HTTP request)
builder.Services.AddScoped<IAssetRepository, AssetRepository>();

var app = builder.Build();

app.UseAuthorization();
app.MapControllers();

app.Run();
