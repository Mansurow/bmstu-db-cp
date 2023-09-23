using System.Data.SqlTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic.CompilerServices;
using Portal.Common.Models;
using Portal.Common.Models.Dto;
using Portal.Common.Models.Enums;
using Portal.Database.Core.Repositories;
using Portal.Services.PackageService.Exceptions;
using Portal.Services.ZoneService.Exceptions;

namespace Portal.Services.ZoneService;

/// <summary>
/// Сервис зон
/// </summary>
public class ZoneService: IZoneService
{
    private readonly IZoneRepository _zoneRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IPackageRepository _packageRepository;
    private readonly ILogger<ZoneService> _logger;

    public ZoneService(IZoneRepository zoneRepository, 
        IInventoryRepository inventoryRepository,
        IPackageRepository packageRepository,
        ILogger<ZoneService> logger) 
    {
        _zoneRepository = zoneRepository ?? throw new ArgumentNullException(nameof(zoneRepository));
        _inventoryRepository = inventoryRepository ?? throw new ArgumentNullException(nameof(inventoryRepository));
        _packageRepository = packageRepository ?? throw new ArgumentNullException(nameof(packageRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<List<Zone>> GetAllZonesAsync(Role permissions)
    {
        return _zoneRepository.GetAllZonesAsync(permissions);
    }

    public async Task<Zone> GetZoneByIdAsync(Guid zoneId, Role permissions)
    {
        try
        {
            var zone = await _zoneRepository.GetZoneByIdAsync(zoneId, permissions);
            
            return zone;
        }
        catch (InvalidOperationException e)
        {
            _logger.LogError(e, "Zone with id: {ZoneId} not found", zoneId);
            throw new ZoneNotFoundException($"Zone with id: {zoneId} not found");
        }
        catch (DbUpdateException e)
        {
            _logger.LogError(e, "No access to the Zones table");
            throw new ZoneAccessException("No access to the Zones table");
        }
    }
    
    public async Task<Guid> AddZoneAsync(string name, string address, double size, int limit, Role permissions)
    {
        try
        {
            try
            {
                var zone = await _zoneRepository.GetZoneByNameAsync(name, permissions);

                _logger.LogError("This name \"{ZoneName}\" of zone exists.", zone.Name);
                throw new ZoneNameExistException($"This name \"{zone.Name}\" of zone exists.");
            }
            catch (InvalidOperationException)
            {
                _logger.LogInformation("This name \"{ZoneName}\" of zone not found.", name);
                var newZone = new Zone(Guid.NewGuid(), name, address, size, limit, 0.0,
                    new List<Inventory>(),
                    new List<Package>());
                await _zoneRepository.InsertZoneAsync(newZone, permissions);

                return newZone.Id;
            }
        }
        catch (DbUpdateException e)
        {
            if (e.InnerException is Npgsql.PostgresException && e.InnerException.Message.Contains("42501"))
            {
                _logger.LogError(e, "No access to the Zones table");
                throw new ZoneAccessException("No access to the Zones table");
            }
            
            _logger.LogError(e, "Error while creating zone");
            throw new ZoneCreateException("Zone has not been created");
        }
    }
    
    public async Task<Guid> AddZoneAsync(string name, string address, double size, int limit, List<Inventory> inventories, List<Package> packages, Role permissions)
    {
        try
        {
            try
            {
                var zone = await _zoneRepository.GetZoneByNameAsync(name, Role.Administrator);
                
                _logger.LogError("This name \"{ZoneName}\" of zone exists.", zone.Name);
                throw new ZoneNameExistException($"This name \"{zone.Name}\" of zone exists.");
            }
            catch (InvalidOperationException)
            {
                _logger.LogInformation("This name \"{ZoneName}\" of zone not found.", name);
                var newZone = new Zone(Guid.NewGuid(), name, address, size, limit, 0.0, inventories, packages);
                await _zoneRepository.InsertZoneAsync(newZone, permissions);

                return newZone.Id;
            }
        }
        catch (DbUpdateException e)
        {
            if (e.InnerException is Npgsql.PostgresException && e.InnerException.Message.Contains("42501"))
            {
                _logger.LogError(e, "No access to the Zones table");
                throw new ZoneAccessException("No access to the Zones table");
            }
            
            _logger.LogError(e, "Error while creating zone");
            throw new ZoneCreateException("Zone has not been created");
        }
    }

    public async Task UpdateZoneAsync(Zone updateZone, Role permissions)
    {
        try
        {
            // await _zoneRepository.GetZoneByNameAsync(updateZone.Zone);

            foreach (var inv in updateZone.Inventories)
            {
                try
                {
                    await _inventoryRepository.GetInventoryByIdAsync(inv.Id, permissions);
                }
                catch (Exception)
                {
                    inv.Id = Guid.NewGuid();
                    await _inventoryRepository.InsertInventoryAsync(inv, permissions);
                }
            }
            
            await _zoneRepository.UpdateZoneAsync(updateZone, permissions);
        }
        catch (InvalidOperationException e)
        {
            _logger.LogError(e, "Zone with id: {ZoneId} not found", updateZone.Id);
            throw new ZoneNotFoundException($"Zone with id: {updateZone.Id} not found");
        }
        catch (DbUpdateException e)
        {
            if (e.InnerException is Npgsql.PostgresException && e.InnerException.Message.Contains("42501"))
            {
                _logger.LogError(e, "No access to the Zones table");
                throw new ZoneAccessException("No access to the Zones table");
            }
            
            _logger.LogError(e, "Error while updating zone: {ZoneId}", updateZone.Id);
            throw new ZoneUpdateException($"Zone with id: {updateZone.Id} was not updated");
        }
    }
    
    public async Task AddInventoryAsync(Guid zoneId, List<CreateInventoryDto> inventories, Role permissions)
    {
        try
        {
            var zone = await _zoneRepository.GetZoneByIdAsync(zoneId, permissions);
            
            // Инвентарь может находится в нескольких зонах - а должно быть каждый инвентарь для определенной зоны
            // var zoneInventory = zone.Inventories.FirstOrDefault(i => i.Id == inventory.Id);
            // if (zoneInventory is not null)
            // {
            //     _logger.LogError("Inventory: {InventoryId} already has been included in zone in zone: {ZoneId}", inventory.Id, zone.Id);
            //     throw new ZoneExistsInventoryException($"Inventory with id: {inventory.Id} already has been included in zone with id: {zoneId}");
            // }

            foreach (var inventory in inventories)
            {
                var zoneInventory = new Inventory(Guid.NewGuid(), zoneId, inventory.Name, inventory.Description,
                    DateOnly.Parse(inventory.YearOfProduction));
            
                zone.AddInventory(zoneInventory);

                await _inventoryRepository.InsertInventoryAsync(zoneInventory, permissions);
            }
            
            await _zoneRepository.UpdateZoneAsync(zone, Role.Administrator);
        }
        catch (InvalidOperationException e)
        {
            if (e.InnerException is Npgsql.PostgresException && e.InnerException.Message.Contains("42501"))
            {
                _logger.LogError(e, "No access to the Zones table");
                throw new ZoneAccessException("No access to the Zones table");
            }
            
            _logger.LogError(e, "Zone with id: {ZoneId} not found", zoneId);
            throw new ZoneNotFoundException($"Zone with id: {zoneId} not found");
        }
        catch (DbUpdateException e)
        {
            if (e.InnerException is Npgsql.PostgresException && e.InnerException.Message.Contains("42501"))
            {
                _logger.LogError(e, "No access to the Zones table");
                throw new ZoneAccessException("No access to the Zones table");
            }
            
            _logger.LogError(e, "Error while adding inventory in zone: {ZoneId}", zoneId);
            throw new ZoneUpdateException($"Zone with id: {zoneId} has not been updated");
        }
    }
    
    public async Task AddPackageAsync(Guid zoneId, List<Guid> packages, Role permissions)
    {
        try
        {
            var zone = await _zoneRepository.GetZoneByIdAsync(zoneId, permissions);
           
            foreach (var packageId in packages)
            {
                try
                {
                    var package = await _packageRepository.GetPackageByIdAsync(packageId, permissions);

                    var zonePackage = zone.Packages.FirstOrDefault(p => p.Id == packageId);
                    if (zonePackage is not null)
                    {
                        _logger.LogError("Package: {PackageId} already has been included in zone in zone: {ZoneId}",
                            packageId, zone.Id);
                        throw new ZonePackageExistsException(
                            $"Package with id: {packageId} for zone with id: {zoneId} already exists");
                    }

                    zone.AddPackage(package);
                }
                catch (InvalidOperationException e)
                {
                    _logger.LogError(e, "Package with id: {PackageId} not found", packageId);
                    throw new PackageNotFoundException($"Package with id: {packageId} not found");
                }
            }
            
            await _zoneRepository.UpdateZoneAsync(zone, permissions);
        }
        catch (InvalidOperationException e)
        {
            _logger.LogError(e, "Zone with id: {ZoneId} not found", zoneId);
            throw new ZoneNotFoundException($"Zone with id: {zoneId} not found");
        }
        catch (DbUpdateException e)
        {
            if (e.InnerException is Npgsql.PostgresException && e.InnerException.Message.Contains("42501"))
            {
                _logger.LogError(e, "No access to the Zones table");
                throw new ZoneAccessException("No access to the Zones table");
            }
            
            _logger.LogError(e, "Error while adding package for zone: {ZoneId}", zoneId);
            throw new ZoneUpdateException($"Zone with id: {zoneId} has not been updated");
        }
    }
    
    public async Task AddPackageAsync(Guid zoneId, Guid packageId, Role permissions)
    {
        try
        {
            var zone = await _zoneRepository.GetZoneByIdAsync(zoneId, permissions);
            try
            {
                var package = await _packageRepository.GetPackageByIdAsync(packageId, permissions);
                
                var zonePackage = zone.Packages.FirstOrDefault(p => p.Id == packageId);
                if (zonePackage is not null)
                {
                    _logger.LogError("Package: {PackageId} already has been included in zone in zone: {ZoneId}", packageId, zone.Id);
                    throw new ZonePackageExistsException($"Package with id: {packageId} for zone with id: {zoneId} already exists");
                }
                
                zone.AddPackage(package);
                await _zoneRepository.UpdateZoneAsync(zone, Role.Administrator);
            }
            catch (InvalidOperationException e)
            {
                _logger.LogError(e, "Package with id: {PackageId} not found", packageId);
                throw new PackageNotFoundException($"Package with id: {packageId} not found");
            }
        }
        catch (InvalidOperationException e)
        {
            _logger.LogError(e, "Zone with id: {ZoneId} not found", zoneId);
            throw new ZoneNotFoundException($"Zone with id: {zoneId} not found");
        }
        catch (DbUpdateException e)
        {
            if (e.InnerException is Npgsql.PostgresException && e.InnerException.Message.Contains("42501"))
            {
                _logger.LogError(e, "No access to the Zones table");
                throw new ZoneAccessException("No access to the Zones table");
            }
            
            _logger.LogError(e, "Error while adding package for zone: {ZoneId}", zoneId);
            throw new ZoneUpdateException($"Zone with id: {zoneId} has not been updated");
        }
    }
    
    public async Task RemoveZoneAsync(Guid zoneId, Role permissions)
    {
        try
        {
            await _zoneRepository.DeleteZoneAsync(zoneId, permissions);
        }
        catch (InvalidOperationException e)
        {
            _logger.LogError(e, "Zone with id: {ZoneId} not found", zoneId);
            throw new ZoneNotFoundException($"Zone with id: {zoneId} not found");
        }
        catch (DbUpdateException e)
        {
            if (e.InnerException is Npgsql.PostgresException && e.InnerException.Message.Contains("42501"))
            {
                _logger.LogError(e, "No access to the Zones table");
                throw new ZoneAccessException("No access to the Zones table");
            }
            
            _logger.LogError(e, "Error while removing zone: {ZoneId}", zoneId);
            throw new ZoneRemoveException($"Zone was not updated: {zoneId}");
        }
    }
}
