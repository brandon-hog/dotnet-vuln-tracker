using Shared.Dtos;

namespace Application.Interfaces;

public interface IAssetService
{
    public Task<Guid> Create(CreateAsset request, CancellationToken cancellationToken);

    public Task Delete(DeleteAsset request, CancellationToken cancellationToken);

    public Task<bool> Update(UpdateAsset request, CancellationToken cancellationToken);

    public Task<AssetDto?> GetById(GetAssetById request, CancellationToken cancellationToken);

    public Task<PagedResponse<AssetDto>> GetPaged(GetAssetsPaged request, CancellationToken cancellationToken);
}