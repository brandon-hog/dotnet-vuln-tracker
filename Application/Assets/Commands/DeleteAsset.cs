using Application.Interfaces;
using MediatR;

namespace Application.Assets.Commands;

public record DeleteAssetCommand(Guid Id) : IRequest;

public class DeleteAssetCommandHandler(IAssetRepository repository)
    : IRequestHandler<DeleteAssetCommand>
{
    public async Task Handle(DeleteAssetCommand request, CancellationToken cancellationToken)
    {
        var asset = await repository.GetByIdAsync(request.Id, cancellationToken);

        if (asset is null)
        {
            return;
        }

        await repository.DeleteAsync(asset, cancellationToken);
    }
}
