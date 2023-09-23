using Portal.Common.Models;
using Portal.Common.Models.Enums;

namespace Portal.Database.Core.Repositories;

/// <summary>
/// Интерфейс репозитория инвентаря
/// </summary>
public interface IInventoryRepository
{
    /// <summary>
    /// Получить весь инвентарь
    /// </summary>
    /// <returns>Список всего инвентаря</returns>
    Task<List<Inventory>> GetAllInventoryAsync(Role role);
    
    /// <summary>
    /// Получить инвентарь
    /// </summary>
    /// <param name="inventoryId">Идентификатор инвентаря</param>
    /// <returns>Инвентарь</returns>
    Task<Inventory> GetInventoryByIdAsync(Guid inventoryId, Role role);
    
    /// <summary>
    /// Добавить инвентарь 
    /// </summary>
    /// <param name="inventory">Данные новго инвентаря</param>
    /// <returns></returns>
    Task InsertInventoryAsync(Inventory inventory, Role role);
    
    /// <summary>
    /// Обновить инвентарь
    /// </summary>
    /// <param name="inventory">Данные инвентаря для обновления</param>
    /// <returns></returns>
    Task UpdateInventoryAsync(Inventory inventory, Role role);
    
    /// <summary>
    /// Удалить инвентарь
    /// </summary>
    /// <param name="inventoryId">Идентификатор инвентаря</param>
    /// <returns></returns>
    Task DeleteInventoryAsync(Guid inventoryId, Role role);
}
