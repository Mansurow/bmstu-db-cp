using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Portal.Common.Models;
using Portal.Common.Models.Dto;
using Portal.Common.Models.Enums;
using Portal.Services.BookingService;
using Portal.Services.BookingService.Configuration;
using Portal.Services.OauthService;
using Portal.Services.PackageService;
using Portal.Services.ZoneService;

namespace Portal.Experiments.Core.AccessObject;

public class AccessObject: NpgsqlAccessObjectBase
{
    public IZoneService ZoneService { get; }
    public IPackageService PackageService { get; }
    public IOauthService OauthService { get;  }
    public IBookingService BookingService { get; }
    
    public IOptions<BookingServiceConfiguration> BookingServiceConfiguration { get; }
    
    public AccessObject() : base()
    {
        BookingServiceConfiguration = Options.Create(
            new BookingServiceConfiguration()
            {
                StartTimeWorking = "0:00:00",
                EndTimeWorking = "24:00:00"
            });

        ZoneService = new ZoneService(ZoneRepository, 
            InventoryRepository, 
            PackageRepository, 
            new NullLogger<ZoneService>());

        PackageService = new PackageService(PackageRepository, 
            MenuRepository,
            new NullLogger<PackageService>());

        OauthService = new OauthService(UserRepository,
            new NullLogger<OauthService>());

        BookingService = new BookingService(BookingRepository,
            PackageRepository,
            ZoneRepository,
            new NullLogger<BookingService>(),
            BookingServiceConfiguration);
    }

    public async Task<List<Guid>> CreateZones(int count)
    {
        var zonesId = new List<Guid>();
        
        for (var i = 0; i < count; i++)
        {
             var id = await ZoneService.AddZoneAsync($"Zone{i}", $"address{i}", 10, 10, Role.Administrator);
             await ZoneService.AddInventoryAsync(id, new List<CreateInventoryDto>()
             {
                new CreateInventoryDto($"Zone{i} inventory1", "inventory1", "10.12.2022"),
                new CreateInventoryDto($"Zone{i} inventory2", "inventory1", "10.12.2022"),
                new CreateInventoryDto($"Zone{i} inventory3", "inventory1", "10.12.2022"),
                new CreateInventoryDto($"Zone{i} inventory4", "inventory1", "10.12.2022"),
                new CreateInventoryDto($"Zone{i} inventory5", "inventory1", "10.12.2022"),
                new CreateInventoryDto($"Zone{i} inventory6", "inventory1", "10.12.2022"),
                new CreateInventoryDto($"Zone{i} inventory7", "inventory1", "10.12.2022"),
                new CreateInventoryDto($"Zone{i} inventory8", "inventory1", "10.12.2022"),
                new CreateInventoryDto($"Zone{i} inventory9", "inventory1", "10.12.2022"),
                new CreateInventoryDto($"Zone{i} inventory10", "inventory1", "10.12.2022"),
             }, Role.Administrator);
             zonesId.Add(id);
        }

        return zonesId;
    }

    public async Task<List<Guid>> CreateUsers(int count)
    {
        var ids = new List<Guid>();
        
        for (var i = 0; i < count; i++)
        { 
            var id = Guid.NewGuid();
            await OauthService.Registrate(
                new User(id, 
                $"Фамилия{i}", $"Имя{i}", $"Отчество{i}", DateOnly.FromDateTime(DateTime.Now), Gender.Unknown,
                $"email{i}@gmail.ru"), 
                $"password123{i}");
            ids.Add(id);
        }

        return ids;
    }
    
    public async Task<List<Guid>> CreateBookings(int count, List<Guid> users, List<Guid> zones, List<Guid> packages)
    {
        var ids = new List<Guid>();
        var indexZoneId = 0;
        for (var i = 0; i < count;)
        {
            foreach (var user in users)
            {
                var id = await BookingService.AddBookingAsync(user, zones[indexZoneId], packages[indexZoneId], 
                    DateOnly.FromDateTime(DateTime.Today),
                    new TimeOnly(11, 0, 0),
                    new TimeOnly(23, 0, 0),
                    Role.Administrator);
                ids.Add(id);
                i++;
            }
            indexZoneId += 1;
        }

        return ids;
    }
    
    public async Task<List<Guid>> CreatePackages(int count)
    {
        var ids = new List<Guid>();
        
        for (var i = 0; i < count; i++)
        {
            var id = await PackageService.AddPackageAsync($"Пакет{i}", PackageType.Usual, 350, 2, $"Описание пакета оплаты {i}",  new List<Guid>(), Role.Administrator);
            ids.Add(id);
        }

        return ids;
    }
    
    public List<Zone> CreateMockZones(int count)
    {
        var zones = new List<Zone>();

        for(var i = 0; i < count; i++)
        {
            zones.Add(new Zone(Guid.NewGuid(), $"Zone{i}", $"address{i}", 10, 10, 0.0, new List<Inventory>(), new List<Package>()));
        }

        return zones;
    }

    public List<Package> CreateMockPackages(int count)
    {
        var zones = new List<Package>();

        for(var i = 0; i < count; i++)
        {
            zones.Add(new Package(Guid.NewGuid(), $"Пакет{i}", PackageType.Usual, 350, 2,
                $"Описание пакета оплаты {i}", new List<Zone>(), new List<Dish>()));
        }

        return zones;
    }
}