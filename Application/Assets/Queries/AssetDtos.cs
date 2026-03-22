namespace Application.Assets.Queries;

public record VulnerabilityDto(string CveId, string Description, string Severity, decimal CvssScore);

public record AssetDto(
    Guid Id, 
    string Hostname, 
    string IpAddress, 
    decimal TotalRiskScore, 
    IEnumerable<VulnerabilityDto> Vulnerabilities);