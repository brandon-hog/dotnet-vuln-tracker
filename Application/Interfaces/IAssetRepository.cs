using Application.Assets.Dtos;
using Domain.Entities;

namespace Application.Interfaces;

public interface IAssetRepository
{
    Task<Asset?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Asset>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Asset asset, CancellationToken cancellationToken = default);
    Task UpdateAsync(Asset asset, CancellationToken cancellationToken = default);
    Task<PagedResponse<AssetDto>> GetPagedAssets(PaginationFilter paginationFilter, CancellationToken cancellationToken = default);
}