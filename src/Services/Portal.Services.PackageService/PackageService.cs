using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Portal.Common.Models;
using Portal.Common.Models.Enums;
using Portal.Database.Core.Repositories;
using Portal.Services.MenuService.Exceptions;
using Portal.Services.PackageService.Exceptions;

namespace Portal.Services.PackageService;

/// <summary>
/// Сервис управления пакетами
/// </summary>
public class PackageService: IPackageService
{
    private readonly IPackageRepository _packageRepository;
    private readonly IMenuRepository _menuRepository;
    private readonly ILogger<PackageService> _logger;

    public PackageService(IPackageRepository packageRepository, IMenuRepository menuRepository, ILogger<PackageService> logger)
    {
        _packageRepository = packageRepository ?? throw new ArgumentNullException(nameof(packageRepository));
        _menuRepository = menuRepository ?? throw new ArgumentNullException(nameof(menuRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<List<Package>> GetPackagesAsync(Role permissions)
    {
        return _packageRepository.GetAllPackagesAsync(permissions);
    }

    public async Task<Package> GetPackageById(Guid packageId, Role permissions)
    {
        try
        {
            var package = await _packageRepository.GetPackageByIdAsync(packageId, permissions);

            return package;
        }
        catch (InvalidOperationException e)
        {
            _logger.LogError(e, "Package with id: {PackageId} not found", packageId);
            throw new PackageNotFoundException($"Package with id: {packageId} not found");
        }
        catch (DbUpdateException e)
        {
            _logger.LogError(e, "No access to the Packages table");
            throw new PackageAccessException("No access to the Packages table");
        }
    }

    public async Task<Guid> AddPackageAsync(string name, PackageType type, double price,
        int rentalTime, string description, List<Guid> dishes, Role permissions)
    {
        try
        {
            var package = new Package(Guid.NewGuid(), name, type, price, rentalTime, description, new List<Zone>(), new List<Dish>());

            foreach (var dishId in dishes)
            {
                try
                {
                    var dish = await _menuRepository.GetDishByIdAsync(dishId, permissions);
                    
                    // TODO: Проверка что блюдо уже добавлено в пакет
                    
                    package.Dishes.Add(dish);
                }
                catch (InvalidOperationException e)
                {
                    _logger.LogError(e, "Dish: {DishId} not found for adding in package: {PackageId}", dishId, package.Id);
                    throw new DishNotFoundException($"Dish: {dishId} not found for adding in package: {package.Id}");
                }
            }

            await _packageRepository.InsertPackageAsync(package, permissions);

            return package.Id;
        }
        catch (DbUpdateException e)
        {
            if (e.InnerException is Npgsql.PostgresException && e.InnerException.Message.Contains("42501"))
            {
                _logger.LogError(e, "No access to the Packages table");
                throw new PackageAccessException("No access to the Packages table");
            }
            
            _logger.LogError(e, "Error while creating package");
            throw new PackageCreateException("Package has not been created");
        }
        
    }

    public async Task UpdatePackageAsync(Package package, Role permissions)
    {
        try
        {
            // TODO: если блюд нет или зон нет - проверка - выкидывать ошибку
            
            await _packageRepository.UpdatePackageAsync(package, permissions);
        }
        catch (InvalidOperationException e)
        {
            _logger.LogError(e, "Package with id: {PackageId} not found", package.Id);
            throw new PackageNotFoundException($"Package with id: {package.Id} not found");
        }
        catch (DbUpdateException e)
        {
            if (e.InnerException is Npgsql.PostgresException && e.InnerException.Message.Contains("42501"))
            {
                _logger.LogError(e, "No access to the Packages table");
                throw new PackageAccessException("No access to the Packages table");
            }
            
            _logger.LogError(e, "Error while updating package: {PackageId}", package.Id);
            throw new PackageUpdateException($"Package with id: {package.Id} has not been updated");
        }
    }

    public async Task RemovePackageAsync(Guid packageId, Role permissions)
    {
        try
        {
            await _packageRepository.DeletePackageAsync(packageId, permissions);
        }
        catch (InvalidOperationException e)
        {
            _logger.LogError(e, "Package with id: {PackageId} not found", packageId);
            throw new PackageNotFoundException($"Package with id: {packageId} not found");
        }
        catch (DbUpdateException e)
        {
            if (e.InnerException is Npgsql.PostgresException && e.InnerException.Message.Contains("42501"))
            {
                _logger.LogError(e, "No access to the Packages table");
                throw new PackageAccessException("No access to the Packages table");
            }
            
            _logger.LogError(e, "Error while removing package: {PackageId}", packageId);
            throw new PackageRemoveException($"Package with id: {packageId} has not been removed");
        }
    }
}
