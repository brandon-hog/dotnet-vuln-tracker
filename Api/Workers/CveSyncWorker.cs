using System.Text.Json;
using Api.Dtos;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Api.Workers;

/**
 Grabs the first 10 vulnerabilties, and checks if any of the assets
 have a matching keyword in the CVE description from their hostname.
 If so, it assigns the asset to the vulnerability.
*/
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
        var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();

        var client = httpClientFactory.CreateClient("NvdApi");
        
        // Fetching 10 results for portfolio demonstration. 
        var response = await client.GetAsync("?resultsPerPage=10", cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Failed to fetch from NVD. HTTP Status: {StatusCode}", response.StatusCode);
            return;
        }

        // Performance Optimization: Read as Stream instead of String to minimize memory allocation
        await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        
        var nvdData = await JsonSerializer.DeserializeAsync<NvdResponse>(
            contentStream, 
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, 
            cancellationToken);

        if (nvdData?.Vulnerabilities == null || nvdData.Vulnerabilities.Count == 0)
        {
            logger.LogInformation("No vulnerabilities returned from NVD API.");
            return;
        }

        var assets = await repository.GetAllAsync(cancellationToken);
        var assetList = assets.ToList();

        if (assetList.Count == 0)
        {
            logger.LogInformation("No assets in database to scan. Skipping correlation.");
            return;
        }

        foreach (var wrapper in nvdData.Vulnerabilities)
        {
            var cve = wrapper.Cve;
            if (string.IsNullOrWhiteSpace(cve.Id)) continue;

            // Safely extract deeply nested NVD data, providing safe defaults if missing
            var description = cve.Descriptions.FirstOrDefault(d => d.Lang == "en")?.Value ?? "No description available.";
            var cvssData = cve.Metrics?.CvssMetricV31?.FirstOrDefault()?.CvssData;
            
            decimal score = cvssData?.BaseScore ?? 0m;
            Severity severity = MapSeverity(cvssData?.BaseSeverity);

            var domainVuln = new Vulnerability(cve.Id, description, severity, score);

            foreach (var asset in assetList)
            {
                if (IsAssetVulnerable(asset, description))
                {
                    // Domain method (AddVulnerability) handles the duplicate CVE check
                    asset.AddVulnerability(domainVuln);
                    await repository.UpdateAsync(asset, cancellationToken);
                    logger.LogInformation("Flagged Asset {Hostname} with {CveId}", asset.Hostname, cve.Id);
                }
            }
        }
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