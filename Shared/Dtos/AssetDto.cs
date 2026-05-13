namespace Shared.Dtos;

public record AssetDto(
    Guid Id, 
    string Hostname, 
    string IpAddress, 
    string Cpe,
    decimal TotalRiskScore, 
    IEnumerable<VulnerabilityDto> Vulnerabilities);

public record CreateAsset(string Hostname, string IpAddress, string Cpe);

public record DeleteAsset(Guid Id);

public record UpdateAsset(Guid Id, string Hostname, string IpAddress, string Cpe);

public record GetAssetById(Guid Id);

public record GetAssetsPaged(PaginationFilter PaginationFilter);