using Microsoft.EntityFrameworkCore;
using Portal.Common.Converter;
using Portal.Common.Models;
using Portal.Common.Models.Enums;
using Portal.Database.Context;
using Portal.Database.Core.Repositories;

namespace Portal.Database.Repositories.NpgsqlRepositories;

public class MenuRepository: BaseRepository, IMenuRepository
{
    private readonly IDbContextFactory _contextFactory;

    public MenuRepository(IDbContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
    }
    
    public Task<List<Dish>> GetAllDishesAsync(Role role)
    {
        return _contextFactory.GetDbContext(role)
            .Menu
            .Select(d => MenuConverter.ConvertDbModelToAppModel(d))
            .ToListAsync();
    }
    
    public async Task<Dish> GetDishByIdAsync(Guid dishId, Role role)
    {
        var dish = await _contextFactory.GetDbContext(role)
            .Menu.FirstAsync(d => d.Id == dishId);

        return MenuConverter.ConvertDbModelToAppModel(dish);
    }

    public async Task InsertDishAsync(Dish dish, Role role)
    {
        var context = _contextFactory.GetDbContext(role);
        var dishDb = MenuConverter.ConvertAppModelToDbModel(dish);
        
        await context.Menu.AddAsync(dishDb);
        await context.SaveChangesAsync();
    }

    public async Task UpdateDishAsync(Dish dish, Role role)
    {
        var context = _contextFactory.GetDbContext(role);
        var updatedDish = await context.Menu.FirstAsync(d => d.Id == dish.Id);
        updatedDish.Name = dish.Name;
        updatedDish.Type = dish.Type;
        updatedDish.Price = dish.Price;
        updatedDish.Description = dish.Description;
        
        await context.SaveChangesAsync();
    }

    public async Task DeleteDishAsync(Guid dishId, Role role)
    {
        var context = _contextFactory.GetDbContext(role);
        var dishDb = await context.Menu.FirstAsync(d => d.Id == dishId);

        context.Menu.Remove(dishDb);
        await context.SaveChangesAsync();
    }
}