﻿using Portal.Common.Models;
using Portal.Common.Models.Enums;
using Portal.Services.BookingService.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portal.Database.Core.Repositories;
using Portal.Services.BookingService.Configuration;
using Portal.Services.PackageService.Exceptions;
using Portal.Services.ZoneService.Exceptions;

namespace Portal.Services.BookingService
{
    /// <summary>
    ///  Сервис бронирования зон
    /// </summary>
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IPackageRepository _packageRepository;
        private readonly IZoneRepository _zoneRepository;
        private readonly ILogger<BookingService> _logger;
        private readonly BookingServiceConfiguration _config;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bookingRepository"></param>
        /// <param name="packageRepository"></param>
        /// <param name="zoneRepository"></param>
        /// <param name="logger"></param>
        /// <param name="config"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public BookingService(IBookingRepository bookingRepository, 
            IPackageRepository packageRepository,
            IZoneRepository zoneRepository,
            ILogger<BookingService> logger, 
            IOptions<BookingServiceConfiguration> config)
        {
            _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
            _packageRepository = packageRepository ?? throw new ArgumentNullException(nameof(packageRepository));
            _zoneRepository = zoneRepository ?? throw new ArgumentNullException(nameof(zoneRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config is null ? throw new ArgumentNullException(nameof(config)): config.Value;
        }

        public async Task<List<Booking>> GetAllBookingAsync(Role permissions)
        {
            try
            {
                var bookings = await _bookingRepository.GetAllBookingAsync(permissions);

                await UpdateNoActualBookingsAsync(bookings, permissions);

                return bookings;
            }
            catch (DbUpdateException e)
            {
                if (e.InnerException is Npgsql.PostgresException && e.InnerException.Message.Contains("42501"))
                {
                    _logger.LogError(e, "No access to the Bookings table");
                    throw new BookingAccessException("No access to the Bookings table");
                }
                
                _logger.LogError(e,"Error while updating no actual bookings while getting all bookings");
                throw new BookingUpdateException("No actual bookings were not updated");
            }
        }

        public async Task<List<Booking>> GetBookingByUserAsync(Guid userId, Role permissions)
        {
            try
            {
                var bookings = await _bookingRepository.GetBookingByUserAsync(userId, permissions);

                await UpdateNoActualBookingsAsync(bookings, permissions);

                return bookings;
            }
            catch (DbUpdateException e)
            {
                _logger.LogError(e,"Error while updating no actual bookings when getting bookings for user: {UserId}", userId);
                throw new BookingUpdateException("No actual bookings were not updated");
            }
        }
        
        private async Task UpdateNoActualBookingsAsync(List<Booking> bookings, Role permissions)
        {
            foreach (var b in bookings.Where(b => b.IsBookingExpired() 
                                                  && b.Status is not BookingStatus.Done
                                                  && b.Status is not BookingStatus.TemporaryReserved))
            {
                b.ChangeStatus(BookingStatus.Done);
                await _bookingRepository.UpdateNoActualBookingAsync(b.Id, permissions);
            }

            foreach (var booking in bookings.Where(IsTemporaryBookingExpired))
            {
                await _bookingRepository.DeleteBookingAsync(booking.Id, permissions);
            }
        }
        
        public async Task<List<Booking>> GetBookingByZoneAsync(Guid zoneId, Role permissions)
        {
            try
            {
                var bookings = await _bookingRepository.GetBookingByZoneAsync(zoneId, permissions);

                await UpdateNoActualBookingsAsync(bookings, permissions);

                return bookings;
            }
            catch (DbUpdateException e)
            {
                _logger.LogError(e, "Error while updating no actual bookings when getting bookings for zone: {ZoneId}", zoneId);
                throw new BookingUpdateException("No actual bookings were not updated");
            }
            
        }
        
        public async Task<Booking> GetBookingByIdAsync(Guid bookingId, Role permissions)
        {
            try
            {
                var booking = await _bookingRepository.GetBookingByIdAsync(bookingId, permissions);

                if (IsTemporaryBookingExpired(booking))
                {
                    await _bookingRepository.DeleteBookingAsync(bookingId, permissions);
                    
                    _logger.LogError("Bookings with id: {BookingId} is expired", bookingId);
                    throw new BookingNotFoundException($"Bookings with id: {bookingId} is expired");
                }
                
                if (booking.IsBookingExpired())
                {
                    booking.ChangeStatus(BookingStatus.Done);
                    await _bookingRepository.UpdateNoActualBookingAsync(booking.Id, permissions);
                }
                
                return booking;
            }
            catch (InvalidOperationException e)
            {
                _logger.LogError(e,"Bookings with id: {BookingId} not found", bookingId);
                throw new BookingNotFoundException($"Bookings with id: {bookingId} not found");
            }
            catch (DbUpdateException e)
            {
                _logger.LogError(e, "Error while updating no actual booking: {BookingId}", bookingId);
                throw new BookingUpdateException("No actual bookings were not updated");
            }
        }

        private bool IsTemporaryBookingExpired(Booking booking)
        {
            return booking.Status is BookingStatus.TemporaryReserved && 
                   DateTime.UtcNow - booking.CreateDateTime > _config.TemporaryReservedBookingTime;
        }
        
        public async Task<List<FreeTime>> GetFreeTimeAsync(Guid zoneId, DateOnly date, Role permissions)
        {
            var bookings = (await GetBookingByZoneAsync(zoneId, permissions))
                .FindAll(e => e.Date == date && e.IsActualStatus())
                .OrderBy(e => e.StartTime).ToList();

            if (bookings.Count == 0)
            {
                return new List<FreeTime>()
                {
                    new FreeTime(_config.StartTimeWorking, 
                        _config.EndTimeWorking)
                };
            }
            
            var freeTimes = new List<FreeTime>();
            var startTimeWork = TimeOnly.Parse(_config.StartTimeWorking);
            var endTimeWork = TimeOnly.Parse(_config.EndTimeWorking);
            
            for (var i = 0; i <= bookings.Count; i++)
            {
                FreeTime addFreeTime;
                if (i == 0)
                {
                    addFreeTime = new FreeTime(startTimeWork, bookings[i].StartTime);
                }
                else if (i == bookings.Count - 1)
                {
                    addFreeTime = new FreeTime(bookings[i].EndTime, endTimeWork);
                }
                else if (i > bookings.Count - 1)
                {
                    if (freeTimes.Last().EndTime != endTimeWork)
                        addFreeTime = new FreeTime(bookings[i - 1].EndTime, endTimeWork);
                    else
                        continue;
                }
                else
                {
                    addFreeTime = new FreeTime(bookings[i - 1].EndTime, bookings[i].StartTime);
                }
                
                if (addFreeTime.EndTime - addFreeTime.StartTime >= new TimeSpan(1, 0, 0))
                    freeTimes.Add(addFreeTime);
            }

            return freeTimes.OrderBy(f => f.StartTime).ToList();
        }

        public async Task<Guid> AddBookingAsync(Guid userId, Guid zoneId, Guid packageId, DateOnly date, TimeOnly startTime, TimeOnly endTime, Role permissions)
        {
            try
            {
                var zone = await _zoneRepository.GetZoneByIdAsync(zoneId, permissions);
                var totalPrice = 0.0;
                
                try
                {
                    await _packageRepository.GetPackageByIdAsync(packageId, permissions);
                }
                catch (InvalidOperationException e)
                {
                    _logger.LogError(e, "Package with id: {PackageId} not found", packageId);
                    throw new PackageNotFoundException($"Package with id: {packageId} not found");
                }
                
                var booking = (await _bookingRepository.GetBookingByUserAndZoneAsync(userId, zoneId, permissions))
                    .FirstOrDefault(b => b.Date == date);
                if (booking is not null)
                {
                    _logger.LogError("User with id: {UserId} reversed for zone with id: {ZoneId}", userId, zoneId);
                    throw new BookingExistsException($"User with id: {userId} reversed for zone with id: {zoneId}");
                }
                
                if (!await IsFreeTimeAsync(date, startTime, endTime, permissions))
                {
                    _logger.LogError("Zone full or partial reversed on {Date} from {StartTime} to {EndTime}", date, startTime, endTime);
                    throw new BookingReversedException(
                        $"Zone full or partial reversed on {date} from {startTime} to {endTime}");
                }

                var newBooking = new Booking(Guid.NewGuid(), zoneId, userId, packageId,
                    zone.Limit, BookingStatus.TemporaryReserved,
                    date, startTime, endTime, DateTime.UtcNow, false, totalPrice);
                await _bookingRepository.InsertBookingAsync(newBooking, permissions);

                return newBooking.Id;
            }
            catch (InvalidOperationException e)
            {
                _logger.LogError(e, "Zone with id: {ZoneId} not found", zoneId);
                throw new ZoneNotFoundException($"Zone with id: {zoneId} not found");
            }
            catch (DbUpdateException e)
            {
                _logger.LogError(e, "Error while creating booking");
                throw new BookingCreateException("Booking has bot been created");
            }
        }
        
        public async Task<bool> IsFreeTimeAsync(DateOnly date, TimeOnly startTime, TimeOnly endTime, Role permissions)
        {
            var bookings =  (await _bookingRepository.GetAllBookingAsync(permissions))
                .FindAll(b => b.Date == date);

            var startTimeWork = TimeOnly.Parse(_config.StartTimeWorking);
            var endTimeWork = TimeOnly.Parse(_config.EndTimeWorking);
            
            if (date < DateOnly.FromDateTime(DateTime.UtcNow) 
                || endTime > endTimeWork
                || startTime < startTimeWork)
                return false;
            
            
            return bookings.Count == 0 
                   || bookings.All(b => (b.StartTime < startTime && b.EndTime <= startTime) 
                                        || (b.StartTime >= endTime && b.EndTime > startTime));
        }
        
        public async Task ChangeBookingStatusAsync(Guid bookingId, BookingStatus status, Role permissions)
        {
            try
            {
                var booking =  await _bookingRepository.GetBookingByIdAsync(bookingId, permissions);

                if (!booking.IsSuitableStatus(status))
                {
                    _logger.LogError("Changing for booking with id: {BookingId} for user: {UserId} isn't suitable for next step", booking.Id, booking.UserId);
                    throw new BookingNotSuitableStatusException(
                        $"Changing for booking with id: {booking.Id} for user: {booking.UserId} isn't suitable for next step");
                }

                booking.ChangeStatus(status);
                await _bookingRepository.UpdateBookingAsync(booking, permissions);
            }
            catch (InvalidOperationException e)
            {
                _logger.LogError(e,"Bookings with id: {BookingId} not found", bookingId);
                throw new BookingNotFoundException($"Booking with id: {bookingId} not found");
            }
            catch (DbUpdateException e)
            {
                _logger.LogError(e, "Error while updating no actual bookings when getting changing status for booking: {BookingId}", bookingId);
                throw new BookingUpdateException($"No actual booking: {bookingId} was not updated");
            }
        }

        public async Task UpdateBookingAsync(Booking updateBooking, Role permissions)
        {
            try
            {
                var booking = await _bookingRepository.GetBookingByIdAsync(updateBooking.Id, Role.Administrator);

                var zone = await _zoneRepository.GetZoneByIdAsync(updateBooking.ZoneId, Role.Administrator);
                if (zone is not null && zone.Limit < updateBooking.AmountPeople)
                {
                    _logger.LogError("Exceed limit amount of people for booking with id: {BookingId}", booking.Id);
                    throw new BookingExceedsLimitException(
                        $"Exceed limit amount of people for booking with id: {updateBooking.Id}");
                }

                if (booking.IsChangeDateTime(updateBooking))
                {
                    _logger.LogError("Changing date or time for booking with id: {BookingId}", booking.Id);
                    throw new BookingChangeDateTimeException(
                        $"Changing date or time for booking with id: {booking.Id}");
                }

                // if (booking.Status != BookingStatus.NoActual && booking.IsBookingExpired())
                //     booking.ChangeStatus(BookingStatus.NoActual);

                await _bookingRepository.UpdateBookingAsync(updateBooking, Role.Administrator);
            }
            catch (InvalidOperationException e)
            {
                _logger.LogError(e, "Bookings with id: {BookingId} not found", updateBooking.Id);
                throw new BookingNotFoundException($"Booking with id: {updateBooking.Id} not found");
            }
        }

        public async Task RemoveBookingAsync(Guid bookingId, Role permissions)
        {
            try
            {
                await _bookingRepository.DeleteBookingAsync(bookingId, permissions);
            }
            catch (InvalidOperationException e)
            {
                _logger.LogError(e, "Bookings with id: {BookingId} not found", bookingId);
                throw new BookingNotFoundException($"Booking with id: {bookingId} not found");
            }
            catch (DbUpdateException e)
            {
                _logger.LogError(e, "Error while removing booking: {BookingId}", bookingId);
                throw new BookingRemoveException($"Booking with id: {bookingId} has not been removed");
            }
        }
    }
}
