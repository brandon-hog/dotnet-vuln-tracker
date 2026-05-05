using System.Text.Json;
using Shared.Dtos;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Api.Workers;

/// <summary>
///  Grabs the first 10 vulnerabilties, and checks if any of the assets
///  have a matching keyword in the CVE description from their hostname.
///  If so, it assigns the asset to the vulnerability. 
/// 
///  CPE string format:
///  pe:2.3:part:vendor:product:version:update:edition:language:sw_edition:target_sw:target_hw:other
/// </summary>
public class CveSyncWorker(IServiceScopeFactory scopeFactory, ILogger<CveSyncWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromHours(24));

        do
        {
            try
            {
                logger.LogInformation("Starting NVD CVE synchronization...");
                await FetchAndStoreCvesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Fatal error during CVE synchronization.");
            }
        } 
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task FetchAndStoreCvesAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IAssetRepository>();
        var nvdService = scope.ServiceProvider.GetRequiredService<INvdService>();

        // TODO figure out when to seed the database with the NVD data

        // TODO Update the database with the latest vulnerabilities
        await nvdService.UpdateDatabase();
        
        // TODO update the assets with the latest vulnerabilities
        
    }

    private static Severity MapSeverity(string? nvdSeverity) => nvdSeverity?.ToUpperInvariant() switch
    {
        "LOW" => Severity.Low,
        "MEDIUM" => Severity.Medium,
        "HIGH" => Severity.High,
        "CRITICAL" => Severity.Critical,
        _ => Severity.Low // Default fallback for unassigned/null severities
    };

    private static bool IsAssetVulnerable(Asset asset, string cveDescription)
    {
        if (string.IsNullOrWhiteSpace(asset.Hostname) || string.IsNullOrWhiteSpace(cveDescription)) 
            return false;
        
        // Simulated Correlation: Flags if the asset hostname parts exist in the CVE description
        var keywords = asset.Hostname.Split('-');
        return keywords.Any(k => cveDescription.Contains(k, StringComparison.OrdinalIgnoreCase));
    }
}