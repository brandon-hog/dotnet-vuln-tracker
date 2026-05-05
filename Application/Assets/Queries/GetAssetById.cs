using Application.Interfaces;
using MediatR;
using Shared.Dtos;

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
                v.Id, 
                v.Descriptions.FirstOrDefault(d => d.Lang == "en")?.Value ?? string.Empty, 
                v.CvssV31BaseSeverity?.ToString() ?? string.Empty,
                v.CvssV31BaseScore ?? 0));

        return new AssetDto(
            asset.Id,
            asset.Hostname,
            asset.IpAddress,
            asset.Cpe,
            asset.CalculateTotalRiskScore(), // Expose calculated domain logic safely
            vulnerabilities);
    }
}