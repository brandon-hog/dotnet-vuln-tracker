using Application.Interfaces;
using MediatR;

namespace Application.Assets.Commands;

public record UpdateAssetCommand(Guid Id, string Hostname, string IpAddress) : IRequest<bool>;

public class UpdateAssetCommandHandler(IAssetRepository repository)
    : IRequestHandler<UpdateAssetCommand, bool>
{
    public async Task<bool> Handle(UpdateAssetCommand request, CancellationToken cancellationToken)
    {
        var asset = await repository.GetByIdAsync(request.Id, cancellationToken);

        if (asset is null)
        {
            return false;
        }

        asset.UpdateDetails(request.Hostname, request.IpAddress);
        await repository.UpdateAsync(asset, cancellationToken);
        return true;
    }
}
