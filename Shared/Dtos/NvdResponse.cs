namespace Shared.Dtos;

using System.Text.Json.Serialization;

public class NvdResponse
{
    [JsonPropertyName("resultsPerPage")]
    public int ResultsPerPage { get; set; }

    [JsonPropertyName("startIndex")]
    public int StartIndex { get; set; }

    [JsonPropertyName("totalResults")]
    public int TotalResults { get; set; }

    [JsonPropertyName("format")]
    public string Format { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; set; } 

    [JsonPropertyName("vulnerabilities")]
    public List<NvdVulnerabilityWrapper> Vulnerabilities { get; set; } = [];
}

public class NvdVulnerabilityWrapper
{
    [JsonPropertyName("cve")]
    public CveItem Cve { get; set; } = new();
}

public class CveItem
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("published")]
    public DateTimeOffset Published { get; set; }

    [JsonPropertyName("lastModified")]
    public DateTimeOffset LastModified { get; set; }

    [JsonPropertyName("vulnStatus")]
    public string VulnStatus { get; set; } = string.Empty;

    [JsonPropertyName("descriptions")]
    public List<CveDescription> Descriptions { get; set; } = [];

    // Nullable because newly published CVEs often do not have CVSS scores assigned yet
    [JsonPropertyName("metrics")]
    public CveMetrics? Metrics { get; set; } 

    [JsonPropertyName("references")]
    public List<NvdReference> References { get; set; } = [];

    [JsonPropertyName("configurations")]
    public List<NvdConfiguration> Configurations { get; set; } = [];
}

public class NvdReference
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("source")]
    public string? Source { get; set; }
}

public class NvdConfiguration
{
    [JsonPropertyName("nodes")]
    public List<NvdNode> Nodes { get; set; } = [];
}

public class NvdNode
{
    [JsonPropertyName("cpeMatch")]
    public List<NvdCpeMatch> CpeMatch { get; set; } = [];
}

public class NvdCpeMatch
{
    [JsonPropertyName("vulnerable")]
    public bool Vulnerable { get; set; }

    [JsonPropertyName("criteria")]
    public string Criteria { get; set; } = string.Empty;

    [JsonPropertyName("matchCriteriaId")]
    public string MatchCriteriaId { get; set; } = string.Empty;
}

public class CveDescription
{
    [JsonPropertyName("lang")]
    public string Lang { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;
}

public class CveMetrics
{
    // NVD categorizes metrics by version. V3.1 is the modern standard
    [JsonPropertyName("cvssMetricV31")]
    public List<CvssMetricV31>? CvssMetricV31 { get; set; }
}

public class CvssMetricV31
{
    [JsonPropertyName("cvssData")]
    public CvssData CvssData { get; set; } = new();
}

public class CvssData
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("baseScore")]
    public decimal BaseScore { get; set; }

    [JsonPropertyName("baseSeverity")]
    public string BaseSeverity { get; set; } = string.Empty;
}