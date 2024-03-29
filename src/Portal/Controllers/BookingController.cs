using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portal.Common.Models;
using Portal.Common.Models.Dto;
using Portal.Common.Models.Enums;
using Portal.Services.BookingService;
using Portal.Services.BookingService.Exceptions;
using Portal.Services.PackageService;
using Portal.Services.PackageService.Exceptions;
using Portal.Services.ZoneService.Exceptions;

namespace Portal.Controllers;

/// <summary>
/// Контроллер бронирования
/// </summary>
[ApiController]
[Route("api/v1/bookings/")]
public class BookingController: ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly IPackageService _packageService;
    private readonly ILogger<BookingController> _logger;
    
    private Guid UserId {
        get
        {
            var claim = User.Claims.SingleOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            return claim is null ? Guid.Empty : Guid.Parse(claim.Value);
        }
    }
    private Role Permissions
    {
        get
        {
            var claim = User.Claims.SingleOrDefault(c => c.Type == ClaimTypes.Role);

            return claim is null ? Role.UnAuthorized : Enum.Parse<Role>(claim.Value);
        }
    }
    
    /// <summary>
    /// Конструктор контроллера бронирования
    /// </summary>
    /// <param name="bookingService">Сервис бронирования зон</param>
    /// <param name="packageService">Сервис бронирования пакетов</param>
    /// <param name="logger">Инструмент логгирования</param>
    /// <exception cref="ArgumentNullException">Ошибка происходит, если параметры переданы неверно</exception>
    public BookingController(IBookingService bookingService, IPackageService packageService, ILogger<BookingController> logger)
    {
        _bookingService = bookingService ?? throw new ArgumentNullException(nameof(bookingService));
        _packageService = packageService ?? throw new ArgumentNullException(nameof(packageService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Получить брони зон
    /// </summary>
    /// <returns>Список бронь зон</returns>
    /// <response code="200">OK. Список бронь зон.</response>
    /// <response code="400">Bad request. Некорректные данные</response>
    /// <response code="401">Unauthorized. Пользователь неавторизован.</response>
    /// <response code="403">Forbidden. У пользователя недостаточно прав доступа.</response>
    /// <response code="500">Internal server error. Ошибка на стороне сервера.</response>
    [HttpGet]
    [Authorize(Roles = nameof(Role.Administrator))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<Booking>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetBookings()
    {
        try
        {
            var bookings = await _bookingService.GetAllBookingAsync(Permissions);

            return Ok(bookings);
        }
        catch (BookingAccessException e)
        {
            _logger.LogError(e, "Access Denied for user {UserId} with permissions {Permissions}", UserId, Permissions);
            return StatusCode(StatusCodes.Status403Forbidden,new
            {
                messsage = e.Message
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Internal server error");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = e.Message
            });
        }
    }
    
    /// <summary>
    /// Получить бронь по идентификатору
    /// </summary>
    /// <returns>Список бронь зон</returns>
    /// <response code="200">OK. Данные брони.</response>
    /// <response code="400">Bad request. Некорректные данные</response>
    /// <response code="401">Unauthorized. Пользователь неавторизован.</response>
    /// <response code="403">Forbidden. У пользователя недостаточно прав доступа.</response>
    /// <response code="404">NotFound. Бронь зоны не найдена.</response>
    /// <response code="500">Internal server error. Ошибка на стороне сервера.</response>
    [HttpGet("{bookingId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<Booking>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetBooking([FromRoute] Guid bookingId)
    {
        try
        {
            
            // if (bookingId == Guid.Empty)
            // {
            //     var bookings = await _bookingService.GetBookingByUserAsync(UserId, Permissions);
            //
            //     var tempBooking = bookings.FirstOrDefault(b => b.Status == BookingStatus.TemporaryReserved);
            //
            //     if (tempBooking is null)
            //     {
            //         return NotFound();
            //     }
            //
            //     return Ok(tempBooking);
            // }

            var booking = await _bookingService.GetBookingByIdAsync(bookingId, Permissions);

            return Ok(booking);
        }
        catch (BookingAccessException e)
        {
            _logger.LogError(e, "Access Denied for user {UserId} with permissions {Permissions}", UserId, Permissions);
            return StatusCode(StatusCodes.Status403Forbidden,new
            {
                messsage = e.Message
            });
        }
        catch (BookingNotFoundException e)
        {
            _logger.LogError(e, "Booking {BookingId} not found", bookingId);
            return NotFound(new
            {
                message = e.Message
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Internal server error");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = e.Message
            });
        }
    }
    
    /// <summary>
    /// Получить все брони пользователя
    /// </summary>
    /// <param name="userId" example="f0fe5f0b-cfad-4caf-acaf-f6685c3a5fc6">Идентификатор пользователя</param>
    /// <returns>Список все бронь пользователя</returns>
    /// <response code="200">OK. Список все бронь пользователя.</response>
    /// <response code="400">Bad request. Некорректные данные</response>
    /// <response code="401">Unauthorized. Пользователь неавторизован.</response>
    /// <response code="403">Forbidden. У пользователя недостаточно прав доступа.</response>
    /// <response code="500">Internal server error. Ошибка на стороне сервера.</response>
    [HttpGet("user/{userId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<Booking>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserBookings(Guid userId)
    {
        try
        {
            var bookings = await _bookingService.GetBookingByUserAsync(userId, Permissions);

            return Ok(bookings);
        }
        catch (BookingAccessException e)
        {
            _logger.LogError(e, "Access Denied for user {UserId} with permissions {Permissions}", UserId, Permissions);
            return StatusCode(StatusCodes.Status403Forbidden,new
            {
                messsage = e.Message
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Internal server error");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = e.Message
            });
        }
    }
    
    /// <summary>
    /// Получить все брони определенной зоны
    /// </summary>
    /// <returns>Список все бронь пользователя</returns>
    /// <param name="zoneId" example="f0fe5f0b-cfad-4caf-acaf-f6685c3a5fc6">Идентификатор зоны</param>
    /// <response code="200">OK. Список все бронь пользователя.</response>
    /// <response code="400">Bad request. Некорректные данные</response>
    /// <response code="401">Unauthorized. Пользователь неавторизован.</response>
    /// <response code="403">Forbidden. У пользователя недостаточно прав доступа.</response>
    /// <response code="500">Internal server error. Ошибка на стороне сервера.</response>
    [HttpGet("zone/{zoneId:guid}")]
    [Authorize(Roles = nameof(Role.Administrator))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<Booking>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetZoneBookings(Guid zoneId)
    {
        try
        {
            var bookings = await _bookingService.GetBookingByZoneAsync(zoneId, Permissions);

            return Ok(bookings);
        }
        catch (BookingAccessException e)
        {
            _logger.LogError(e, "Access Denied for user {UserId} with permissions {Permissions}", UserId, Permissions);
            return StatusCode(StatusCodes.Status403Forbidden,new
            {
                messsage = e.Message
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Internal server error");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = e.Message
            });
        }
    }

    /// <summary>
    /// Получить свободное время для бронирования зоны
    /// </summary>
    /// <param name="zoneId" example="f0fe5f0b-cfad-4caf-acaf-f6685c3a5fc6">Идентификатор зоны</param>
    /// <param name="date" example="10.08.2023">Дата бронирования</param>
    /// <returns>Список свободного времени для бронирования зоны</returns>
    /// <response code="200">OK. Список свободного времени для бронирования зоны.</response>
    /// <response code="400">Bad request. Некорректные данные</response>
    /// <response code="401">Unauthorized. Пользователь неавторизован.</response>
    /// <response code="403">Forbidden. У пользователя недостаточно прав доступа.</response>
    /// <response code="500">Internal server error. Ошибка на стороне сервера.</response>
    [HttpGet("freetime/{zoneId:guid}&{date}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<FreeTime>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetFreeTime([FromRoute] Guid zoneId, [FromRoute] string date)
    {
        try
        {
            var freeTime = await _bookingService.GetFreeTimeAsync(zoneId, DateOnly.Parse(date), Permissions);

            return Ok(freeTime);
        }
        catch (BookingAccessException e)
        {
            _logger.LogError(e, "Access Denied for user {UserId} with permissions {Permissions}", UserId, Permissions);
            return StatusCode(StatusCodes.Status403Forbidden,new
            {
                messsage = e.Message
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Internal server error");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = e.Message
            });
        }
    }
    
    /// <summary>
    /// Забронировать зоны 
    /// </summary>
    /// <param name="bookingDto">Данные для создании брони зоны</param>
    /// <returns>Список свободного времени для бронирования зоны</returns>
    /// <response code="204">NoContent. Бронь успешно создана.</response>
    /// <response code="400">Bad request. Некорректные данные</response>
    /// <response code="401">Unauthorized. Пользователь неавторизован.</response>
    /// <response code="403">Forbidden. У пользователя недостаточно прав доступа.</response>
    /// <response code="500">Internal server error. Ошибка на стороне сервера.</response>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PostBooking([FromBody] CreateBookingDto bookingDto)
    {
        try
        {
            var bookingId = await _bookingService.AddBookingAsync(bookingDto.UserId, bookingDto.ZoneId,
                bookingDto.PackageId,
                bookingDto.Date, bookingDto.StartTime, bookingDto.EndTime,
                Permissions);

            return Ok(new { bookingId });
        }
        catch (BookingAccessException e)
        {
            _logger.LogError(e, "Access Denied for user {UserId} with permissions {Permissions}", UserId, Permissions);
            return StatusCode(StatusCodes.Status403Forbidden,new
            {
                messsage = e.Message
            });
        }
        catch (BookingExistsException e)
        {
            _logger.LogError(e, "Zone: {ZoneId} already has been reversed by user: {UserId}", bookingDto.ZoneId,
                bookingDto.UserId);
            return BadRequest(new
            {
                message = e.Message
            });
        }
        catch (BookingReversedException e)
        {
            _logger.LogError(e, "Zone: {ZoneId} already has been reversed", bookingDto.ZoneId);
            return BadRequest(new
            {
                message = e.Message
            });
        }
        catch (ZoneNotFoundException e)
        {
            _logger.LogError(e, "Zone: {ZoneId} not found", bookingDto.ZoneId);
            return BadRequest(new
            {
                message = e.Message
            });
        }
        catch (PackageNotFoundException e)
        {
            _logger.LogError(e, "Package: {PackageId} not found", bookingDto.PackageId);
            return BadRequest(new
            {
                message = e.Message
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Internal server error");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = e.Message
            });
        }
    }
    
    /// <summary>
    /// Подтвердить зону 
    /// </summary>
    /// <param name="bookingDto">Данные для подтверждении брони зоны</param>
    /// <returns>Список свободного времени для бронирования зоны</returns>
    /// <response code="204">NoContent. Бронь успешно подтверждена.</response>
    /// <response code="400">Bad request. Некорректные данные</response>
    /// <response code="401">Unauthorized. Пользователь неавторизован.</response>
    /// <response code="403">Forbidden. У пользователя недостаточно прав доступа.</response>
    /// <response code="404">NotFound. Бронь зоны не найдена.</response>
    /// <response code="500">Internal server error. Ошибка на стороне сервера.</response>
    [HttpPatch]
    [Authorize(Roles = nameof(Role.User))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ConfirmBooking([FromBody] ConfirmBookingDto bookingDto)
    {
        try
        {
            // TODO: проверка на подтверждение только собственной брони   

            var booking = await _bookingService.GetBookingByIdAsync(bookingDto.Id, Permissions);

            var package = await _packageService.GetPackageById(booking.PackageId, Permissions);
            var totalPrice = 0.0;
            if (package.Type == PackageType.Usual)
            {
                double hours = (bookingDto.EndTime - bookingDto.StartTime).Hours;
                double minutes = (bookingDto.EndTime - bookingDto.StartTime).Minutes;
                totalPrice = package.Price * (hours + minutes / 60);
            }
            else
            {
                totalPrice = package.Price;
            }

            booking.Date = bookingDto.Date;
            booking.StartTime = bookingDto.StartTime;
            booking.EndTime = bookingDto.EndTime;
            booking.AmountPeople = bookingDto.AmountPeople;
            booking.PackageId = bookingDto.PackageId;
            booking.TotalPrice = totalPrice;
            booking.ChangeStatus(BookingStatus.Reserved);

            await _bookingService.UpdateBookingAsync(booking, Permissions);

            return StatusCode(StatusCodes.Status204NoContent);
        }
        catch (BookingExceedsLimitException e)
        {
            // TODO: logger
            return BadRequest(new
            {
                message = e.Message
            });
        }
        catch (BookingNotFoundException e)
        {
            _logger.LogError(e, "Booking {BookingId} not found", bookingDto.Id);
            return NotFound(new
            {
                message = e.Message
            });
        }
        catch (BookingAccessException e)
        {
            _logger.LogError(e, "Access Denied for user {UserId} with permissions {Permissions}", UserId, Permissions);
            return StatusCode(StatusCodes.Status403Forbidden,new
            {
                messsage = e.Message
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Internal server error");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = e.Message
            });
        }
    }
    
    /// <summary>
    /// Отменить бронь зоны 
    /// </summary>
    /// <param name="bookingId" example="f0fe5f0b-cfad-4caf-acaf-f6685c3a5fc6">Идентификатор брони зоны</param>
    /// <returns>Список свободного времени для бронирования зоны</returns>
    /// <response code="204">NoContent. Бронь успешно подтверждена.</response>
    /// <response code="400">Bad request. Некорректные данные</response>
    /// <response code="401">Unauthorized. Пользователь неавторизован.</response>
    /// <response code="403">Forbidden. У пользователя недостаточно прав доступа.</response>
    /// <response code="404">NotFound. Бронь зоны не найдена.</response>
    /// <response code="500">Internal server error. Ошибка на стороне сервера.</response>
    [HttpDelete("{bookingId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CancelBooking([FromRoute] Guid bookingId)
    {
        try
        {
            // TODO: проверка на отмену собственной брони или админом

            await _bookingService.ChangeBookingStatusAsync(bookingId, BookingStatus.Cancelled, Permissions);

            return StatusCode(StatusCodes.Status204NoContent);
        }
        catch (BookingNotFoundException e)
        {
            _logger.LogError(e, "Booking {BookingId} not found", bookingId);
            return NotFound(new
            {
                message = e.Message
            });
        }
        catch (BookingAccessException e)
        {
            _logger.LogError(e, "Access Denied for user {UserId} with permissions {Permissions}", UserId, Permissions);
            return StatusCode(StatusCodes.Status403Forbidden,new
            {
                messsage = e.Message
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Internal server error");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = e.Message
            });
        }
    }
}