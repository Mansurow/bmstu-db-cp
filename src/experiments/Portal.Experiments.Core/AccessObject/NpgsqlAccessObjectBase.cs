using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Portal.Database.Context;
using Portal.Database.Core.Repositories;
using Portal.Database.Repositories.NpgsqlRepositories;

namespace Portal.Experiments.Core.AccessObject;

public class NpgsqlAccessObjectBase: IDisposable
{
    private readonly PortalDbContext _context;
    public IMenuRepository MenuRepository { get; set; }
    public IUserRepository UserRepository { get; set; }
    public IFeedbackRepository FeedbackRepository { get; set; }
    public IInventoryRepository InventoryRepository { get; set; }
    public IPackageRepository PackageRepository { get; set; }
    public IZoneRepository ZoneRepository { get; set; }
    public IBookingRepository BookingRepository { get; set; }
    
    public NpgsqlAccessObjectBase()
    {
        var connectionString = "Host=localhost;Port=5432;Database=PortalDbExp2;User Id=postgres;Password=postgres;";
        
        var pgContextOptionsBuilder = new DbContextOptionsBuilder<PortalDbContext>()
            .UseNpgsql(connectionString);
        _context = new PortalDbContext(pgContextOptionsBuilder.Options);
        
        var contextFactory = new TestDbContextFactory(_context);
        
        MenuRepository = new MenuRepository(contextFactory);
        UserRepository = new UserRepository(contextFactory);
        FeedbackRepository = new FeedbackRepository(contextFactory);
        InventoryRepository = new InventoryRepository(contextFactory);
        PackageRepository = new PackageRepository(contextFactory);
        ZoneRepository = new ZoneRepository(contextFactory);
        BookingRepository = new BookingRepository(contextFactory);
    }
    
    public virtual void Dispose()
    {
        _context.Zones.RemoveRange(_context.Zones);
        _context.Packages.RemoveRange(_context.Packages);
        _context.Users.RemoveRange(_context.Users);
        _context.Bookings.RemoveRange(_context.Bookings);
        
        _context.SaveChanges();
    }
}