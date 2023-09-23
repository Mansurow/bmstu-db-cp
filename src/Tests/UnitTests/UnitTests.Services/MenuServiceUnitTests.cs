using Microsoft.Extensions.Logging.Abstractions;
using Portal.Services.MenuService;
using Xunit;
using Moq;
using Portal.Common.Models;
using Portal.Common.Models.Enums;
using Portal.Database.Core.Repositories;
using Portal.Services.MenuService.Exceptions;

namespace UnitTests.Services;

public class MenuServiceUnitTests
{
    private readonly IMenuService _menuService;
    private readonly Mock<IMenuRepository> _mockMenuRepository = new();

    private const Role Permissions = Role.Administrator;
    
    public MenuServiceUnitTests()
    {
        _menuService = new MenuService(_mockMenuRepository.Object,
            NullLogger<MenuService>.Instance);
    }

    [Fact]
    public async Task GetAllDishesOkTest()
    {
        // Arrange
        var menu = CreateMockMenu();

        _mockMenuRepository.Setup(s => s.GetAllDishesAsync(It.IsAny<Role>()))
                           .ReturnsAsync(menu);

        // Act
        var actualMenu = await _menuService.GetAllDishesAsync(Permissions);
        
        // Assert
        Assert.Equal(menu, actualMenu);
    }

    [Fact]
    public async Task GetAllDishesEmptyTest()
    {
        // Arrange
        var menu = CreateEmptyMockMenu();

        _mockMenuRepository.Setup(s => s.GetAllDishesAsync(It.IsAny<Role>()))
                           .ReturnsAsync(menu);

        // Act
        var actualMenu = await _menuService.GetAllDishesAsync(Permissions);

        // Assert
        Assert.Equal(menu.Count, actualMenu.Count);
        Assert.Equal(menu, actualMenu);
    }

    [Fact]
    public async Task GetDishByIdOkTest()
    {
        // Arrange
        var menu = CreateMockMenu();
        var expectedDish = menu.First();

        _mockMenuRepository.Setup(s => s.GetDishByIdAsync(It.IsAny<Guid>(), It.IsAny<Role>()))
                           .ReturnsAsync((Guid id, Role p) => menu.First(d => d.Id == id));

        // Act
        var actualDish = await _menuService.GetDishByIdAsync(expectedDish.Id, Permissions);

        // Assert
        Assert.Equal(expectedDish, actualDish);
    }

    [Fact]
    public async Task GetDishByIdEmptyTest()
    {
        // Arrange
        // var menu = CreateEmptyMockMenu();
        var dishId = Guid.NewGuid();

        _mockMenuRepository.Setup(s => s.GetDishByIdAsync(dishId, It.IsAny<Role>()))
            .ThrowsAsync(new InvalidOperationException());

        // Act
        async Task<Dish> Action() => await _menuService.GetDishByIdAsync(dishId, Permissions);

        // Assert
        await Assert.ThrowsAsync<DishNotFoundException>(Action);
    }

    [Fact]
    public async Task AddDishOkTest()
    {
        // Arrange
        var menu = CreateMockMenu();
        var dishId = Guid.NewGuid();
        var dish = CreateMockDish(dishId);

        _mockMenuRepository.Setup(s => s.GetAllDishesAsync(It.IsAny<Role>()))
                           .ReturnsAsync(menu);

        _mockMenuRepository.Setup(s => s.InsertDishAsync(It.IsAny<Dish>(), It.IsAny<Role>()))
                           .Callback((Dish d, Role p) => menu.Add(d));

        // Act
        await _menuService.AddDishAsync(dish.Name, dish.Type, dish.Price, dish.Description, Permissions);
        var actualDish = menu.Last();

        // Assert
        Assert.Equal(actualDish.Name, dish.Name);
        Assert.Equal(actualDish.Type, dish.Type);
        Assert.Equal(actualDish.Price, dish.Price);
        Assert.Equal(actualDish.Description, dish.Description);
    }

    [Fact]
    public async Task UpdateDishOkTest()
    {
        // Arrange
        var menu = CreateMockMenu();
        var dishId = menu[^1].Id;
        var expectedDish = CreateMockDish(dishId);

        _mockMenuRepository.Setup(s => s.GetDishByIdAsync(It.IsAny<Guid>(), It.IsAny<Role>()))
                           .ReturnsAsync((Guid id, Role p) => menu.First(d => d.Id == id));

        _mockMenuRepository.Setup(s => s.UpdateDishAsync(It.IsAny<Dish>(), It.IsAny<Role>()))
                           .Callback((Dish d, Role p) =>
                           {
                               menu.FindAll(e => e.Id == d.Id).ForEach
                               (e =>
                               {
                                   e.Name = d.Name;
                                   e.Type = d.Type;
                                   e.Price = d.Price;
                                   e.Description = d.Description;
                               });

                           });

        // Act
        await _menuService.UpdateDishAsync(expectedDish, Permissions);
        var actualDish = menu.Last();

        // Assert
        Assert.Equal(expectedDish.Id, actualDish.Id);
        Assert.Equal(expectedDish.Name, actualDish.Name);
        Assert.Equal(expectedDish.Type, actualDish.Type);
        Assert.Equal(expectedDish.Price, actualDish.Price);
        Assert.Equal(expectedDish.Description, actualDish.Description);

    }

    [Fact]
    public async Task UpdateDishNotExistsTest()
    {
        // Arrange
        var menu = CreateMockMenu();
        var dishId = Guid.NewGuid();
        var expectedDish = CreateMockDish(dishId);

        _mockMenuRepository.Setup(s => s.GetDishByIdAsync(It.IsAny<Guid>(), It.IsAny<Role>()))
            .ThrowsAsync(new InvalidOperationException());

        _mockMenuRepository.Setup(s => s.UpdateDishAsync(It.IsAny<Dish>(), It.IsAny<Role>()))
           .Callback((Dish d, Role p) =>
           {
               menu.FindAll(e => e.Id == d.Id).ForEach
               (e =>
               {
                   e.Name = d.Name;
                   e.Type = d.Type;
                   e.Price = d.Price;
                   e.Description = d.Description;
               });
           });

        // Act
        Task Action() => _menuService.UpdateDishAsync(expectedDish, Permissions);

        // Assert
        await Assert.ThrowsAsync<DishNotFoundException>(Action);
    }

    [Fact]
    public async Task UpdateDishEmptyTest()
    {
        // Arrange
        var menu = CreateEmptyMockMenu();
        var dishId = Guid.NewGuid();
        var expectedDish = CreateMockDish(dishId);

        _mockMenuRepository.Setup(s => s.GetDishByIdAsync(It.IsAny<Guid>(), It.IsAny<Role>()))
                .ThrowsAsync(new InvalidOperationException());

        _mockMenuRepository.Setup(s => s.UpdateDishAsync(It.IsAny<Dish>(), It.IsAny<Role>()))
               .Callback((Dish d, Role p) =>
               {
                   menu.FindAll(e => e.Id == d.Id).ForEach
                   (e =>
                   {
                       e.Name = d.Name;
                       e.Type = d.Type;
                       e.Price = d.Price;
                       e.Description = d.Description;
                   });

               });

        // Act
        Task Action() => _menuService.UpdateDishAsync(expectedDish, It.IsAny<Role>());

        // Assert
        await Assert.ThrowsAsync<DishNotFoundException>(Action);
    }

    [Fact]
    public async Task DeleteDishOkTest()
    {
        // Arrange
        var menu = CreateMockMenu();
        var dishId = menu[0].Id;
        var expectedCount = menu.Count - 1;
        _mockMenuRepository.Setup(s => s.GetDishByIdAsync(It.IsAny<Guid>(), It.IsAny<Role>()))
                .ReturnsAsync(menu.First(m => m.Id == dishId));

        _mockMenuRepository.Setup(s => s.DeleteDishAsync(It.IsAny<Guid>(), It.IsAny<Role>()))
               .Callback((Guid id, Role p) =>
               {
                   var dish = menu.First(m => m.Id == id);
                   menu.Remove(dish);
               });

        // Act
        await _menuService.RemoveDishAsync(dishId, Permissions);
        var actualCount = menu.Count;

        // Assert
        Assert.Equal(expectedCount, actualCount);
    }

    [Fact]
    public async Task RemoveDishNotExistsTest()
    {
        // Arrange
        // var menu = CreateMockMenu();
        var dishId = Guid.NewGuid();

        _mockMenuRepository.Setup(s => s.DeleteDishAsync(It.IsAny<Guid>(), It.IsAny<Role>()))
            .ThrowsAsync(new InvalidOperationException());
        
        // Act
        Task Action() => _menuService.RemoveDishAsync(dishId, Permissions);

        // Assert
        await Assert.ThrowsAsync<DishNotFoundException>(Action);
    }

    [Fact]
    public async Task RemoveDishEmptyTest()
    {
        // Arrange
        // var menu = CreateEmptyMockMenu();
        var dishId = Guid.NewGuid();

        _mockMenuRepository.Setup(s => s.DeleteDishAsync(It.IsAny<Guid>(), It.IsAny<Role>()))
            .ThrowsAsync(new InvalidOperationException());

        // Act
        Task Action() => _menuService.RemoveDishAsync(dishId, Permissions);

        // Assert
        await Assert.ThrowsAsync<DishNotFoundException>(Action);
    }

    private Dish CreateMockDish(Guid id)
    {
        return new Dish(id, "name+1", DishType.Salat, 130, "bigfoot");
    }

    private List<Dish> CreateMockMenu()
    {
        return new List<Dish>()
        {
            new Dish(Guid.NewGuid(), "Dish1", DishType.FirstCourse, 350, "description 1"),
            new Dish(Guid.NewGuid(), "Dish2", DishType.SecondCourse, 250, "description 2"),
            new Dish(Guid.NewGuid(), "Dish3", DishType.FirstCourse, 120, "description 3")
        };
    }

    private List<Dish> CreateEmptyMockMenu()
    {
        return new List<Dish>();
    }
}
