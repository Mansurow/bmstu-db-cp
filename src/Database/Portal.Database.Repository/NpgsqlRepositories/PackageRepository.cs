using Microsoft.EntityFrameworkCore;
using Portal.Common.Converter;
using Portal.Common.Models;
using Portal.Common.Models.Enums;
using Portal.Database.Context;
using Portal.Database.Core.Repositories;
using Portal.Database.Models;

namespace Portal.Database.Repositories.NpgsqlRepositories;

public class PackageRepository: BaseRepository, IPackageRepository
{
    private readonly IDbContextFactory _contextFactory;

    public PackageRepository(IDbContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public Task<List<Package>> GetAllPackagesAsync(Role role)
    {
        return _contextFactory.GetDbContext(role)
            .Packages
            .Include(p => p.Zones)
            .Include(p => p.Dishes)
            .AsNoTracking()
            .Select(p => PackageConverter.ConvertDbModelToAppModel(p))
            .ToListAsync();
    }

    public async Task<Package> GetPackageByIdAsync(Guid packageId, Role role)
    {
        var package = await _contextFactory.GetDbContext(role)
            .Packages
            .Include(p => p.Zones)
            .Include(p => p.Dishes)
            .AsNoTracking()
            .FirstAsync(p => p.Id == packageId);

        return PackageConverter.ConvertDbModelToAppModel(package);
    }

    public async Task InsertPackageAsync(Package package, Role role)
    {
        var context = _contextFactory.GetDbContext(role);
        var packageDb = new PackageDbModel(
                package.Id,
                package.Name,
                package.Type,
                package.Price,
                package.RentalTime,
                package.Description
            );
        
        await context.Packages.AddAsync(packageDb);
        
        /*var dishes = package.Dishes.Select(MenuConverter.ConvertAppModelToDbModel).ToList();
        packageDb.Dishes = dishes;*/

        foreach (var dish in package.Dishes)
        {
            packageDb.Dishes.Add(context.Menu.First(d => d.Id == dish.Id));
        }
        
        await context.SaveChangesAsync();
    }

    public async Task UpdatePackageAsync(Package package, Role role)
    {
        var context = _contextFactory.GetDbContext(role);
        var packageDb = await context.Packages
            .Include(p => p.Zones)    
            .Include(p => p.Dishes)
            .FirstAsync(p => p.Id == package.Id);

        packageDb.Name = package.Name;
        packageDb.Description = package.Description;
        packageDb.Type = package.Type;
        packageDb.RentalTime = package.RentalTime;
        packageDb.Price = package.Price;
        packageDb.Dishes = package.Dishes.Select(MenuConverter.ConvertAppModelToDbModel).ToList();
        // packageDb.Zones = package.Zones.Select(ZoneConverter.ConvertAppModelToDbModel).ToList();
        
        await context.SaveChangesAsync();
    }

    public async Task DeletePackageAsync(Guid packageId, Role role)
    {
        var context = _contextFactory.GetDbContext(role);
        var package = await context.Packages.FirstAsync(p => p.Id == packageId);

        context.Packages.Remove(package);
        await context.SaveChangesAsync();
    }
}