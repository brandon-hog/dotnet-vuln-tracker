using Application.Interfaces;
using MediatR;
using Application.Assets.Dtos;

namespace Application.Assets.Queries;

public record GetAssetByIdQuery(Guid Id) : IRequest<AssetDto?>;

public class GetAssetByIdQueryHandler(IAssetRepository repository) 
    : IRequestHandler<GetAssetByIdQuery, AssetDto?>
{
    public async Task<AssetDto?> Handle(GetAssetByIdQuery request, CancellationToken cancellationToken)
    {
        var asset = await repository.GetByIdAsync(request.Id, cancellationToken);
        
        if (asset is null)
        {
            return null;
        }

        var vulnerabilities = asset.Vulnerabilities.Select(v => 
            new VulnerabilityDto(
                v.CveId, 
                v.Description, 
                v.Severity.ToString(), // Convert Enum to string for readable JSON
                v.CvssScore));

        return new AssetDto(
            asset.Id,
            asset.Hostname,
            asset.IpAddress,
            asset.CalculateTotalRiskScore(), // Expose calculated domain logic safely
            vulnerabilities);
    }
}