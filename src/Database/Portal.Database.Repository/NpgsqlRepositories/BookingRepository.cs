using Microsoft.EntityFrameworkCore;
using Portal.Common.Converter;
using Portal.Common.Models;
using Portal.Common.Models.Enums;
using Portal.Database.Context;
using Portal.Database.Core.Repositories;

namespace Portal.Database.Repositories.NpgsqlRepositories;

/// <summary>
/// Репозиторий бронирования
/// </summary>
public class BookingRepository: BaseRepository, IBookingRepository
{
    private readonly IDbContextFactory _contextFactory;

    public BookingRepository(IDbContextFactory contextFactory)
    {
        _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
    }

    public Task<List<Booking>> GetAllBookingAsync(Role role)
    {
        return 
            _contextFactory.GetDbContext(role)
            .Bookings
            .OrderBy(b => b.Date)
            .Select(b => BookingConverter.ConvertDbModelToAppModel(b))
            .ToListAsync();
    }

    public Task<List<Booking>> GetBookingByUserAsync(Guid userId, Role role) 
    {
        return _contextFactory.GetDbContext(role)
            .Bookings.Where(b => b.UserId == userId)
            .OrderBy(b => b.Date)
            .Select(b => BookingConverter.ConvertDbModelToAppModel(b))
            .ToListAsync();
    }

    public Task<List<Booking>> GetBookingByZoneAsync(Guid zoneId, Role role)
    {
        return _contextFactory.GetDbContext(role)
            .Bookings.Where(b => b.ZoneId == zoneId)
            .OrderBy(b => b.Date)
            .Select(b => BookingConverter.ConvertDbModelToAppModel(b))
            .ToListAsync();
    }

    public Task<List<Booking>> GetBookingByUserAndZoneAsync(Guid userId, Guid zoneId, Role role) 
    {
        return _contextFactory.GetDbContext(role)
            .Bookings.Where(b => b.UserId == userId && b.ZoneId == zoneId)
            .OrderBy(b => b.Date)
            .Select(b => BookingConverter.ConvertDbModelToAppModel(b))
            .ToListAsync();
    }
    
    public async Task<Booking> GetBookingByIdAsync(Guid bookingId, Role role)
    {
        var booking = await _contextFactory.GetDbContext(role)
            .Bookings.FirstAsync(b => b.Id == bookingId);

        return BookingConverter.ConvertDbModelToAppModel(booking);
    }

    public async Task InsertBookingAsync(Booking createBooking, Role role)
    {
        var context = _contextFactory.GetDbContext(role);
        
        var booking = BookingConverter.ConvertAppModelToDbModel(createBooking);
        await context.Bookings.AddAsync(booking);
        await context.SaveChangesAsync();
    }

    public async Task UpdateNoActualBookingAsync(Guid bookingId, Role role) 
    {
        var context = _contextFactory.GetDbContext(role);
        
        var booking = await context.Bookings.FirstAsync(b => b.Id == bookingId);
        booking.Status = BookingStatus.Done;
        await context.SaveChangesAsync();
    }

    public async Task UpdateBookingAsync(Booking updateBooking, Role role) 
    {
        var context = _contextFactory.GetDbContext(role);
        
        var booking = await context.Bookings.FirstAsync(b => b.Id == updateBooking.Id);

        // booking.UserId = updateBooking.UserId;
        // booking.ZoneId = updateBooking.ZoneId;
        booking.PackageId = updateBooking.PackageId;
        booking.Package = await context.Packages.FirstAsync(p => p.Id == updateBooking.PackageId);
        booking.Status = updateBooking.Status;
        booking.AmountPeople = updateBooking.AmountPeople;
        booking.Date = updateBooking.Date;
        booking.StartTime = updateBooking.StartTime;
        booking.EndTime = updateBooking.EndTime;
        booking.TotalPrice = updateBooking.TotalPrice;
        booking.Date = updateBooking.Date;
            
        await context.SaveChangesAsync();
    }

    public async Task DeleteBookingAsync(Guid bookingId, Role role) 
    {
        var context = _contextFactory.GetDbContext(role);
        
        var booking = await context.Bookings.FirstAsync(b => b.Id == bookingId);
        context.Bookings.Remove(booking);
        await context.SaveChangesAsync();
    }
}
