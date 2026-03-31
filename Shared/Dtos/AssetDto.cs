namespace Shared.Dtos;

public record AssetDto(
    Guid Id, 
    string Hostname, 
    string IpAddress, 
    decimal TotalRiskScore, 
    IEnumerable<VulnerabilityDto> Vulnerabilities);
    