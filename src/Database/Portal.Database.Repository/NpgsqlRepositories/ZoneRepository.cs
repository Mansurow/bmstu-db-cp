using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Portal.Common.Converter;
using Portal.Common.Models;
using Portal.Common.Models.Enums;
using Portal.Database.Context;
using Portal.Database.Core.Repositories;
using Portal.Database.Models;

namespace Portal.Database.Repositories.NpgsqlRepositories;

public class ZoneRepository: BaseRepository, IZoneRepository
{
    private readonly IDbContextFactory _contextFactory;

    public ZoneRepository(IDbContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public Task<List<Zone>> GetAllZonesAsync(Role role)
    {
        return _contextFactory.GetDbContext(role)
            .Zones
            .Include(z => z.Inventories)
            .Include(z => z.Packages)
            .AsNoTracking()
            .Select(z => ZoneConverter.ConvertDbModelToAppModel(z))
            .ToListAsync();
    }

    public async Task<Zone> GetZoneByIdAsync(Guid zoneId, Role role)
    {
        var zone = await _contextFactory.GetDbContext(role)
            .Zones
            .Include(z => z.Inventories)
            .Include(z => z.Packages)
            .AsNoTracking()
            .FirstAsync(z => z.Id == zoneId);

        return ZoneConverter.ConvertDbModelToAppModel(zone);
    }

    public async Task<Zone> GetZoneByNameAsync(string name, Role role)
    {
        var zone = await _contextFactory.GetDbContext(role)
            .Zones
            .FirstAsync(z => string.Equals(z.Name, name, StringComparison.CurrentCultureIgnoreCase));

        return ZoneConverter.ConvertDbModelToAppModel(zone);
    }

    public async Task InsertZoneAsync(Zone zone, Role role)
    {
        var context = _contextFactory.GetDbContext(role);
        var zoneDb = ZoneConverter.ConvertAppModelToDbModel(zone);

        await context.Zones.AddAsync(zoneDb);
        await context.SaveChangesAsync();
    }

    public async Task UpdateZoneAsync(Zone zone, Role role)
    {
        var context = _contextFactory.GetDbContext(role);
        var zoneDb =  await context.Zones
            .Include(z => z.Inventories)
            .Include(z => z.Packages)
            .FirstAsync(z => z.Id == zone.Id);

        zoneDb.Name = zone.Name;
        // zoneDb.Rating = zone.Rating;
        zoneDb.Limit = zone.Limit;
        zoneDb.Address = zone.Address;
        zoneDb.Size = zone.Size;
        zoneDb.Inventories = zone.Inventories.Select(InventoryConverter.ConvertAppModelToDbModel).ToList();
        // zoneDb.Packages = zone.Packages.Select(PackageConverter.ConvertAppModelToDbModel).ToList();

        var packages = new List<PackageDbModel>();
        foreach (var package in zone.Packages)
        {
            var packageDb = await context.Packages.FirstAsync(p => p.Id == package.Id);
            packages.Add(packageDb);
        }

        zoneDb.Packages = packages;
        
        // context.Zones.Update(zoneDb);
        await context.SaveChangesAsync();
    }

    public async Task UpdateZoneRatingAsync(Guid zoneId, double rating, Role role)
    {
        var context = _contextFactory.GetDbContext(role);
        var zone = await context.Zones.FirstAsync(z => z.Id == zoneId);
        zone.Rating = rating;
        
        context.Zones.Update(zone);
        await context.SaveChangesAsync();
    }

    public async Task DeleteZoneAsync(Guid zoneId, Role role)
    {
        var context = _contextFactory.GetDbContext(role);
        var zone = await context.Zones.FirstAsync(z => z.Id == zoneId);

        context.Remove(zone);
        await context.SaveChangesAsync();
    }
}