using Application.Interfaces;

namespace Api.Workers;

public class CveSyncWorker(IServiceScopeFactory scopeFactory, ILogger<CveSyncWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromHours(24));

        do
        {
            try
            {
                logger.LogInformation("Starting CVE synchronization at: {time}", DateTimeOffset.Now);
                await FetchAndStoreCvesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while syncing CVEs.");
            }
        } 
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task FetchAndStoreCvesAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IAssetRepository>();
        var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();

        var client = httpClientFactory.CreateClient("NvdApi");
        
        // TODO fetch data from NVD API 2.0
        var response = await client.GetAsync("?resultsPerPage=5", cancellationToken);
        
        if (response.IsSuccessStatusCode)
        {
            // TODO deserialize JSON
            logger.LogInformation("Successfully fetched CVEs from NVD.");
            
            // TODO update relevant assets
        }
    }
}