using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portal.Common.Models;
using Portal.Common.Models.Enums;
using Portal.Services.InventoryServices;
using Portal.Services.InventoryServices.Exceptions;

namespace Portal.Controllers;

/// <summary>
/// Контроллер инвентаря 
/// </summary>
[ApiController]
[Route("api/v1/inventory/")]
public class InventoryController: ControllerBase
{
    private readonly IInventoryService _inventoryService;
    private readonly ILogger<InventoryController> _logger;
    
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

            if (claim is null)
                return Role.UnAuthorized;
            
            return Enum.Parse<Role>(claim.Value);
        }
    }
    
    public InventoryController(IInventoryService inventoryService, ILogger<InventoryController> logger)
    {
        _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Получить информации о инвентаре
    /// </summary>
    /// <returns>Список инвентаря</returns>
    /// <response code="200">Ok. Список инвентаря.</response>
    /// <response code="400">Bad request. Некорректные данные.</response>
    /// <response code="401">Unauthorized. Пользователь неавторизован.</response>
    /// <response code="403">Forbidden. У пользователя недостаточно прав доступа.</response>
    /// <response code="500">Internal server error. Ошибка на стороне сервера.</response>
    [HttpGet]
    [Authorize(Roles = nameof(Role.Administrator))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<Inventory>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetInventories()
    {
        try
        {
            var inventories = await _inventoryService.GetAllInventoriesAsync(Permissions);

            return Ok(inventories);
        }
        catch (InventoryAccessException e)
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
    /// Списать инвентарь
    /// </summary>
    /// <param name="inventoryId" example="f0fe5f0b-cfad-4caf-acaf-f6685c3a5fc6">Идентификатор инвентаря</param>
    /// <response code="204">NotContent. Инвентарь успешно списан.</response>
    /// <response code="400">Bad request. Некорректные данные.</response>
    /// <response code="401">Unauthorized. Пользователь неавторизован.</response>
    /// <response code="403">Forbidden. У пользователя недостаточно прав доступа.</response>
    /// <response code="404">NotFound. Инвентарь не найден.</response>
    /// <response code="500">Internal server error. Ошибка на стороне сервера.</response>
    [HttpPatch("{inventoryId:guid}")]
    [Authorize(Roles = nameof(Role.Administrator))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> MakeOldStatus(Guid inventoryId)
    {
        try
        {
            var inventory = await _inventoryService.GetInventoryByIdAsync(inventoryId, Permissions);
            inventory.IsWrittenOff = true;
            await _inventoryService.UpdateInventoryAsync(inventory, Permissions);

            return StatusCode(StatusCodes.Status204NoContent);
        }
        catch (InventoryNotFoundException e)
        {
            _logger.LogError(e, "Inventory {InventoryId} not found", inventoryId);
            return NotFound(new
            {
                message = e.Message
            });
        }
        catch (InventoryAccessException e)
        {
            _logger.LogError(e, "Access Denied for user {UserId} with permissions {Permissions}", UserId, Permissions);
            return StatusCode(StatusCodes.Status403Forbidden, new
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