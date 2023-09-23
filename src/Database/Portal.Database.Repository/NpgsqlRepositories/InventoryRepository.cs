using Microsoft.EntityFrameworkCore;
using Portal.Common.Converter;
using Portal.Common.Models;
using Portal.Common.Models.Enums;
using Portal.Database.Context;
using Portal.Database.Core.Repositories;

namespace Portal.Database.Repositories.NpgsqlRepositories;

public class InventoryRepository: BaseRepository, IInventoryRepository
{
    private readonly IDbContextFactory _contextFactory;

    public InventoryRepository(IDbContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
    }
    
    public Task<List<Inventory>> GetAllInventoryAsync(Role role)
    {
        return _contextFactory.GetDbContext(role)
            .Inventories
            .Select(i => InventoryConverter.ConvertDbModelToAppModel(i))
            .ToListAsync();
    }

    public async Task<Inventory> GetInventoryByIdAsync(Guid inventoryId, Role role)
    {
        var inventory = await _contextFactory.GetDbContext(role)
            .Inventories.FirstAsync(i => i.Id == inventoryId);

        return InventoryConverter.ConvertDbModelToAppModel(inventory);
    }

    public async Task InsertInventoryAsync(Inventory inventory, Role role)
    {
        var context = _contextFactory.GetDbContext(role);
        var inventoryDb = InventoryConverter.ConvertAppModelToDbModel(inventory);
        
        await context.Inventories.AddAsync(inventoryDb);
        await context.SaveChangesAsync();
    }

    public async Task UpdateInventoryAsync(Inventory inventory, Role role)
    {
        var context = _contextFactory.GetDbContext(role);
        var inventoryDb = await context.Inventories.FirstAsync(inv => inv.Id == inventory.Id);

        inventoryDb.Name = inventory.Name;
        inventoryDb.Description = inventory.Description;
        inventoryDb.YearOfProduction = inventory.YearOfProduction;
        inventoryDb.IsWrittenOff = inventory.IsWrittenOff;
        
        await context.SaveChangesAsync();
    }

    public async Task DeleteInventoryAsync(Guid inventoryId, Role role)
    {
        var context = _contextFactory.GetDbContext(role);
        var inventory = await context.Inventories.FirstAsync(i => i.Id == inventoryId);

        context.Inventories.Remove(inventory);
        await context.SaveChangesAsync();
    }
}