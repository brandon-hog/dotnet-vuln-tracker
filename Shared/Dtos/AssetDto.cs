namespace Shared.Dtos;

public record AssetDto(
    Guid Id, 
    string Hostname, 
    string IpAddress, 
    string Cpe,
    decimal TotalRiskScore, 
    IEnumerable<VulnerabilityDto> Vulnerabilities);
    