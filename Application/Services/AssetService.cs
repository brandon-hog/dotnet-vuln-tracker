using Application.Interfaces;
using Domain.Entities;
using Shared.Dtos;

namespace Application.Services;

public class AssetService(IAssetRepository repository, ICurrentUser currentUser) : IAssetService
{
    public async Task<Guid> Create(CreateAsset request, CancellationToken cancellationToken)
    {
        // Instantiate the Domain entity (enforcing validation rules)
        var asset = new Asset(request.Hostname, request.IpAddress, request.Cpe, currentUser.Id!);
        
        // Persist via Infrastructure
        await repository.AddAsync(asset, cancellationToken);
        
        // Return the ID for the API response
        return asset.Id;
    }

    public async Task Delete(DeleteAsset request, CancellationToken cancellationToken)
    {
        var asset = await repository.GetByIdAsync(request.Id, cancellationToken);

        if (asset is null)
        {
            return;
        }

        await repository.DeleteAsync(asset, cancellationToken);
    }

    public async Task<bool> Update(UpdateAsset request, CancellationToken cancellationToken)
    {
        var asset = await repository.GetByIdAsync(request.Id, cancellationToken);

        if (asset is null)
        {
            return false;
        }

        asset.UpdateDetails(request.Hostname, request.IpAddress, request.Cpe);
        await repository.UpdateAsync(asset, cancellationToken);
        await repository.SyncAssetByIdVulnerabilitiesAsync(request.Id, cancellationToken);
        return true;
    }

    public async Task<AssetDto?> GetById(GetAssetById request, CancellationToken cancellationToken)
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
                v.BaseSeverity?.ToString() ?? string.Empty,
                v.BaseScore ?? 0));

        return new AssetDto(
            asset.Id,
            asset.Hostname,
            asset.IpAddress,
            asset.Cpe,
            asset.CalculateTotalRiskScore(), // Expose calculated domain logic safely
            vulnerabilities);
    }

    public async Task<PagedResponse<AssetDto>> GetPaged(GetAssetsPaged request, CancellationToken cancellationToken)
    {
        var assets = await repository.GetPagedAssets(request.PaginationFilter, cancellationToken);
        
        return assets;
    }
}