using Application.Assets.Commands;
using Application.Assets.Queries;
using Shared.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class AssetsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateAsset([FromBody] CreateAssetCommand command, CancellationToken cancellationToken)
    {
        // MediatR finds the exact handler for this command and executes it
        var assetId = await mediator.Send(command, cancellationToken);
        
        // Returns HTTP 201 Created with the new ID
        return CreatedAtAction(nameof(GetAssetById), new { id = assetId }, new { Id = assetId });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAssetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetAssetByIdQuery(id);
        var assetDto = await mediator.Send(query, cancellationToken);
        
        return assetDto is not null ? Ok(assetDto) : NotFound();
    }

    [HttpGet]
    public async Task<IActionResult> GetAssetsPaged([FromQuery] PaginationFilter pageFilters, CancellationToken cancellationToken)
    {
        var query = new GetAssetsPagedQuery(pageFilters);
        var assetDto = await mediator.Send(query, cancellationToken);
        
        return assetDto is not null ? Ok(assetDto) : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsset(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteAssetCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateAsset(Guid id, [FromBody] UpdateAssetDto request, CancellationToken cancellationToken)
    {
        var updated = await mediator.Send(
            new UpdateAssetCommand(id, request.Hostname, request.IpAddress),
            cancellationToken);

        return updated ? NoContent() : NotFound();
    }
}