using Application.Interfaces;

namespace Api.Workers;

/// <summary>
/// Periodically syncs CVE data from the NVD API, then recomputes asset–vulnerability links by
/// matching each asset’s CPE to stored CPE match criteria (vulnerable matches only).
/// CPE 2.3 format: cpe:2.3:part:vendor:product:version:update:edition:language:sw_edition:target_sw:target_hw:other
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
        var assetRepository = scope.ServiceProvider.GetRequiredService<IAssetRepository>();
        var vulnerabilityRepository = scope.ServiceProvider.GetRequiredService<IVulnerabilityRepository>();
        var nvdService = scope.ServiceProvider.GetRequiredService<INvdService>();

        if (!await vulnerabilityRepository.AnyAsync())
        {
            logger.LogInformation("Vulnerability table is empty. Seeding from NVD...");
            await nvdService.InitializeDatabase();
            logger.LogInformation("Initial NVD seed completed.");
        }
        else
        {
            logger.LogInformation("Performing 24-hour NVD delta update...");
            await nvdService.UpdateDatabase();
            logger.LogInformation("NVD delta update completed.");
        }

        logger.LogInformation("Recomputing asset-vulnerability correlations...");
        await assetRepository.SyncAssetVulnerabilitiesAsync(cancellationToken);
        logger.LogInformation("Asset-vulnerability correlation completed.");
    }
}