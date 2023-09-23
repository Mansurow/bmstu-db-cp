using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portal.Common.Models;
using Portal.Common.Models.Dto;
using Portal.Common.Models.Enums;
using Portal.Services.MenuService;
using Portal.Services.MenuService.Exceptions;

namespace Portal.Controllers;

/// <summary>
/// Контроллер меню блюд
/// </summary>
[ApiController]
[Route("api/v1/menu")]
public class MenuController: ControllerBase
{
    private readonly IMenuService _menuService;
    private readonly ILogger<MenuController> _logger;

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
    /// Конструктор контроллера меню блюд
    /// </summary>
    /// <param name="menuService">Сервис меню блюд</param>
    /// <param name="logger">Инструмент логгирования</param>
    /// <exception cref="ArgumentNullException">Ошибка происходит, если парметры переданы неверно</exception>
    public MenuController(IMenuService menuService, ILogger<MenuController> logger)
    {
        _menuService = menuService ?? throw new ArgumentNullException(nameof(menuService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Получить меню блюд
    /// </summary>
    /// <returns>Возвращает список блюд меню.</returns>
    /// <response code="200">OK. Возвращает список блюд меню.</response>
    /// <response code="400">Bad request. Некорректные данные.</response>
    /// <response code="500">Internal server error. Ошибка на стороне сервера.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<Dish>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMenu()
    {
        try
        {
            var menu = await _menuService.GetAllDishesAsync(Permissions);

            return Ok(menu);
        }
        catch (DishAccessException e)
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
    /// Получить блюдо
    /// </summary>
    /// <returns>Возвращает данные блюда.</returns>
    /// <response code="200">OK. Возвращает данные блюда</response>
    /// <response code="400">Bad request. Некорректные данные.</response>
    /// <response code="404">NotFound. Блюдо не найдено.</response>
    /// <response code="500">Internal server error. Ошибка на стороне сервера.</response>
    [HttpGet("{dishId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Dish))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDish(Guid dishId)
    {
        try
        {
            var dish = await _menuService.GetDishByIdAsync(dishId, Permissions);

            return Ok(dish);
        }
        catch (DishAccessException e)
        {
            _logger.LogError(e, "Access Denied for user {UserId} with permissions {Permissions}", UserId, Permissions);
            return StatusCode(StatusCodes.Status403Forbidden,new
            {
                messsage = e.Message
            });
        }
        catch (DishNotFoundException e)
        {
            _logger.LogError(e, "Dish not found: {DishId}", dishId);
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
    /// Добавить блюдо
    /// </summary>
    /// <returns>Возвращается идентификатор добавляемого блюда</returns>
    /// <response code="200">OK. Возвращается идентификатор добавляемого блюда.</response>
    /// <response code="400">Bad request. Некорректные данные.</response>
    /// <response code="401">Unauthorized. Пользователь неавторизован.</response>
    /// <response code="403">Forbidden. У пользователя недостаточно прав доступа.</response>
    /// <response code="500">Internal server error. Ошибка на стороне сервера.</response>
    [HttpPost]
    [Authorize(Roles = nameof(Role.Administrator))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Guid))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PostDish([FromBody] CreateDishDto dish)
    {
        try
        {
            var dishId = await _menuService.AddDishAsync(dish.Name, dish.Type, dish.Price, dish.Description, Permissions);

            return Ok(new
            {
                dishId
            });
        }
        catch (DishAccessException e)
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
    /// Обновить данные о блюде
    /// </summary>
    /// <param name="dish">Данные для обновления блюда</param>
    /// <response code="204">NoContent. Обновление успешно произошло.</response>
    /// <response code="400">Bad request. Некорректные данные.</response>
    /// <response code="401">Unauthorized. Пользователь неавторизован.</response>
    /// <response code="403">Forbidden. У пользователя недостаточно прав доступа.</response>
    /// <response code="404">NotFound. Блюдо не найдено.</response>
    /// <response code="500">Internal server error. Ошибка на стороне сервера.</response>
    [HttpPut]
    [Authorize(Roles = nameof(Role.Administrator))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PutDish([FromBody] Dish dish)
    {
        try
        {
            await _menuService.UpdateDishAsync(dish, Permissions);

            return StatusCode(StatusCodes.Status204NoContent);
        }
        catch (DishAccessException e)
        {
            _logger.LogError(e, "Access Denied for user {UserId} with permissions {Permissions}", UserId, Permissions);
            return StatusCode(StatusCodes.Status403Forbidden,new
            {
                messsage = e.Message
            });
        }
        catch (DishNotFoundException e)
        {
            _logger.LogError(e, "Dish not found: {DishId}", dish.Id);
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
    /// Удалить блюдо
    /// </summary>
    /// <param name="dishId">Идентификатор блюда</param>
    /// <response code="204">NoContent. Удаление успешно произошло.</response>
    /// <response code="400">Bad request. Некорректные данные.</response>
    /// <response code="401">Unauthorized. Пользователь неавторизован.</response>
    /// <response code="403">Forbidden. У пользователя недостаточно прав доступа.</response>
    /// <response code="404">NotFound. Блюдо не найдено.</response>
    /// <response code="500">Internal server error. Ошибка на стороне сервера.</response>
    [Authorize(Roles = nameof(Role.Administrator))]
    [HttpDelete("{dishId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteDish([FromRoute] Guid dishId)
    {
        try
        {
            await _menuService.RemoveDishAsync(dishId, Permissions);

            return StatusCode(StatusCodes.Status204NoContent);
        }
        catch (DishAccessException e)
        {
            _logger.LogError(e, "Access Denied for user {UserId} with permissions {Permissions}", UserId, Permissions);
            return StatusCode(StatusCodes.Status403Forbidden,new
            {
                messsage = e.Message
            });
        }
        catch (DishNotFoundException e)
        {
            _logger.LogError(e, "Dish not found: {DishId}", dishId);
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
}