using Shared.Dtos;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class AssetRepository(AppDbContext context) : IAssetRepository
{
    public async Task<Asset?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Assets
            .Include(a => a.Vulnerabilities)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Asset>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Assets
            .Include(a => a.Vulnerabilities)
            .AsNoTracking() // Performance optimization: prevents EF from tracking changes on read-only lists
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Asset asset, CancellationToken cancellationToken = default)
    {
        await context.Assets.AddAsync(asset, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Asset asset, CancellationToken cancellationToken = default)
    {
        context.Assets.Remove(asset);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Asset asset, CancellationToken cancellationToken = default)
    {
        context.Assets.Update(asset);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task SyncAssetVulnerabilitiesAsync(CancellationToken cancellationToken = default)
    {
        var assets = await context.Assets
            .Include(a => a.Vulnerabilities)
            .ToListAsync(cancellationToken);

        if (assets.Count == 0) return;

        var cpes = assets
            .Select(a => a.Cpe)
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Distinct()
            .ToList();

        if (cpes.Count == 0)
        {
            foreach (var asset in assets) asset.ReplaceVulnerabilities([]);
            await context.SaveChangesAsync(cancellationToken);
            return;
        }

        var matches = await context.CpeMatches
            .Where(c => c.Vulnerable && cpes.Contains(c.Criteria))
            .Select(c => new { c.Criteria, c.Vulnerability })
            .ToListAsync(cancellationToken);

        var vulnsByCpe = matches
            .GroupBy(m => m.Criteria)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.Vulnerability).DistinctBy(v => v.Id).ToList());

        foreach (var asset in assets)
        {
            var matched = vulnsByCpe.TryGetValue(asset.Cpe ?? string.Empty, out var list)
                ? (IEnumerable<Vulnerability>)list
                : Array.Empty<Vulnerability>();
            asset.ReplaceVulnerabilities(matched);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResponse<AssetDto>> GetPagedAssets(PaginationFilter paginationFilter, CancellationToken cancellationToken)
    {
        var query = context.Assets.AsQueryable();

        // Apply the search filter before counting or paginating
        if (!string.IsNullOrWhiteSpace(paginationFilter.SearchQuery))
        {
            var searchTerm = paginationFilter.SearchQuery.ToLower();
            query = query.Where(a => 
                a.Hostname.ToLower().Contains(searchTerm) || 
                a.IpAddress.ToLower().Contains(searchTerm));
        }

        // Get the total count
        var totalRecords = await query.CountAsync();
        // Get the paged data
        var pagedData = await query
            .Skip((paginationFilter.PageNumber - 1) * paginationFilter.PageSize)
            .Take(paginationFilter.PageSize)
            .Include(a => a.Vulnerabilities)
            .ToListAsync();

        // Convert the assets into the asset Dtos
        PagedResponse<AssetDto> pagedAssetDtos = new PagedResponse<AssetDto>()
        {
            PageNumber = paginationFilter.PageNumber,
            PageSize = paginationFilter.PageSize,
            TotalRecords = totalRecords,
            Data = []
        };

        foreach (var asset in pagedData)
        {
            var vulnerabilities = asset.Vulnerabilities.Select(v => 
                new VulnerabilityDto(
                    v.Id, 
                    v.Descriptions.FirstOrDefault(d => d.Lang == "en")?.Value ?? string.Empty, 
                    v.CvssV31BaseSeverity?.ToString() ?? string.Empty, // Convert Enum to string for readable JSON
                    v.CvssV31BaseScore ?? 0));

            pagedAssetDtos.Data.Add(new AssetDto(
                asset.Id,
                asset.Hostname,
                asset.IpAddress,
                asset.Cpe,
                asset.CalculateTotalRiskScore(), // Expose calculated domain logic safely
                vulnerabilities));
        }

        return pagedAssetDtos;
    }
}