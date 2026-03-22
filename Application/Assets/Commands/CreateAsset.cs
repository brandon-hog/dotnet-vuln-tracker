using Application.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Assets.Commands;

public record CreateAssetCommand(string Hostname, string IpAddress) : IRequest<Guid>;

public class CreateAssetCommandHandler(IAssetRepository repository) 
    : IRequestHandler<CreateAssetCommand, Guid>
{
    public async Task<Guid> Handle(CreateAssetCommand request, CancellationToken cancellationToken)
    {
        // Instantiate the Domain entity (enforcing validation rules)
        var asset = new Asset(request.Hostname, request.IpAddress);
        
        // Persist via Infrastructure
        await repository.AddAsync(asset, cancellationToken);
        
        // Return the ID for the API response
        return asset.Id;
    }
}