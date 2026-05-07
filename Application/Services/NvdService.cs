namespace Application.Services;

using Application.Interfaces;
using System.Net.Http.Json;
using System.Text.Json;
using Shared.Dtos;
using Domain.Entities;

public class NvdService(HttpClient httpClient, IVulnerabilityRepository vulnerabilityRepository) : INvdService
{
    private Vulnerability MapNvdResponseToVulnerability(NvdVulnerabilityWrapper nvdVulnerability)
    {
        var cve = nvdVulnerability.Cve;

        // Try 3.1 first, if not available fallback on v2
        var cvssData = cve.Metrics?.CvssMetricV31?.FirstOrDefault()?.CvssData;

        // In v2 severity is outside cvssdata, but otherwise it is inside
        var baseSeverity = cvssData?.BaseSeverity;

        if (cvssData is null)
        {
            cvssData = cve.Metrics?.CvssMetricV2?.FirstOrDefault()?.CvssData;
            baseSeverity = cve.Metrics?.CvssMetricV2?.FirstOrDefault()?.BaseSeverity;
        }

        return new Vulnerability
        {
            Id = cve.Id,
            Published = cve.Published,
            LastModified = cve.LastModified,
            VulnStatus = cve.VulnStatus,

            BaseScore = cvssData?.BaseScore,
            BaseSeverity = baseSeverity,

            Descriptions = cve.Descriptions
                .Select(d => new Domain.Entities.CveDescription
                {
                    Lang = d.Lang,
                    Value = d.Value,
                })
                .ToList(),

            References = cve.References
                .Select(r => new Domain.Entities.CveReference
                {
                    Url = r.Url,
                    Source = r.Source,
                })
                .ToList(),

            RawMetricsJson = cve.Metrics is null
                ? null
                : JsonSerializer.Serialize(cve.Metrics),

            CpeMatches = cve.Configurations
                .SelectMany(c => c.Nodes)
                .SelectMany(n => n.CpeMatch)
                .Select(m => new CpeMatch
                {
                    Criteria = m.Criteria,
                    MatchCriteriaId = m.MatchCriteriaId,
                    Vulnerable = m.Vulnerable,
                })
                .ToList(),
        };
    }

    private async Task FetchAndUpsertPagesAsync(string extraQueryParams)
    {
        int startIndex = 0;
        const int resultsPerPage = 2000;
        int totalResults = int.MaxValue;

        while (startIndex < totalResults)
        {
            var url = $"https://services.nvd.nist.gov/rest/json/cves/2.0/?resultsPerPage={resultsPerPage}&startIndex={startIndex}{extraQueryParams}";

            var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var data = await response.Content.ReadFromJsonAsync<NvdResponse>()
                       ?? throw new InvalidOperationException("NVD returned an empty response.");

            totalResults = data.TotalResults;

            var entities = data.Vulnerabilities
                .Select(MapNvdResponseToVulnerability)
                .ToList();

            if (entities.Count > 0)
            {
                await vulnerabilityRepository.BulkUpsertAsync(entities);
            }

            startIndex += resultsPerPage;

            if (startIndex < totalResults)
            {
                // Stay below the 5-requests-per-30s anonymous NVD quota.
                await Task.Delay(TimeSpan.FromSeconds(6));
            }
        }
    }

    public Task InitializeDatabase() => FetchAndUpsertPagesAsync(extraQueryParams: string.Empty);

    public Task UpdateDatabase()
    {
        var end = DateTime.UtcNow;
        var start = end.AddHours(-24);

        static string Format(DateTime dt) => dt.ToString("yyyy-MM-ddTHH:mm:ss.fff");

        var qs = $"&lastModStartDate={Uri.EscapeDataString(Format(start))}" +
                 $"&lastModEndDate={Uri.EscapeDataString(Format(end))}";

        return FetchAndUpsertPagesAsync(qs);
    }
}