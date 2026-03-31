using Shared.Dtos;
using Application.Interfaces;
using MediatR;

public record GetAssetsPagedQuery(PaginationFilter PaginationFilter) : IRequest<PagedResponse<AssetDto>>;

public class GetAssetsPagedHandler(IAssetRepository repository)
    : IRequestHandler<GetAssetsPagedQuery, PagedResponse<AssetDto>>
{
    public async Task<PagedResponse<AssetDto>> Handle(GetAssetsPagedQuery request, CancellationToken cancellationToken)
    {
        var assets = await repository.GetPagedAssets(request.PaginationFilter, cancellationToken);
        
        return assets;
    }
}