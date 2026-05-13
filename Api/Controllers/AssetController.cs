using Shared.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Application.Interfaces;

namespace Api.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class AssetsController(IAssetService assetService) : ControllerBase
{
    public string? GetUserId()
    {
        return HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsset([FromBody] CreateAsset command, CancellationToken cancellationToken)
    {
        var assetId = await assetService.Create(command, cancellationToken);
        
        // Returns HTTP 201 Created with the new ID
        return CreatedAtAction(nameof(GetAssetById), new { id = assetId }, new { Id = assetId });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAssetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetAssetById(id);
        var assetDto = await assetService.GetById(query, cancellationToken);
        
        return assetDto is not null ? Ok(assetDto) : NotFound();
    }

    [HttpGet]
    public async Task<IActionResult> GetAssetsPaged([FromQuery] PaginationFilter pageFilters, CancellationToken cancellationToken)
    {
        var query = new GetAssetsPaged(pageFilters);
        var assetDto = await assetService.GetPaged(query, cancellationToken);
        
        return assetDto is not null ? Ok(assetDto) : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsset(Guid id, CancellationToken cancellationToken)
    {
        await assetService.Delete(new DeleteAsset(id), cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateAsset(Guid id, [FromBody] UpdateAssetDto request, CancellationToken cancellationToken)
    {
        var updated = await assetService.Update(
            new UpdateAsset(id, request.Hostname, request.IpAddress, request.Cpe),
            cancellationToken);

        return updated ? NoContent() : NotFound();
    }
}