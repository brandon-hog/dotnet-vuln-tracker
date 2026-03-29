using Application.Assets.Commands;
using Application.Assets.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
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
}