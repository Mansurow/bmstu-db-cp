using Portal.Common.Models;
using Portal.Common.Models.Enums;

namespace Portal.Database.Core.Repositories;

/// <summary>
/// Интерфейс репозитория зоны
/// </summary>
public interface IZoneRepository
{
    /// <summary>
    /// Получить все зоны
    /// </summary>
    /// <returns>Список всех зон</returns>
    Task<List<Zone>> GetAllZonesAsync(Role role);
    
    /// <summary>
    /// Получить зону
    /// </summary>
    /// <param name="zoneId">Идентификатор зоны</param>
    /// <returns>Зона</returns>
    Task<Zone> GetZoneByIdAsync(Guid zoneId, Role role);
    
    /// <summary>
    /// Получить зоны по названию
    /// </summary>
    /// <param name="name">Название зоны</param>
    /// <returns>Зона</returns>
    Task<Zone> GetZoneByNameAsync(string name, Role role);
    
    /// <summary>
    /// Добавить зону
    /// </summary>
    /// <param name="zone">Данные новой зоны</param>
    /// <returns></returns>
    Task InsertZoneAsync(Zone zone, Role role);
    
    /// <summary>
    /// Обновить зоны
    /// </summary>
    /// <param name="zone">Данные зоны для обновления</param>
    /// <returns></returns>
    Task UpdateZoneAsync(Zone zone, Role role);
    
    /// <summary>
    /// Обновить рейтинг зоны
    /// </summary>
    /// <param name="zoneId">Идентификатор зоны</param>
    /// <param name="rating">Новый рейнтиг зоны</param>
    /// <returns></returns>
    Task UpdateZoneRatingAsync(Guid zoneId, double rating, Role role);
    
    /// <summary>
    /// Удалить зону
    /// </summary>
    /// <param name="zoneId">Идентифкатор зоны</param>
    /// <returns></returns>
    Task DeleteZoneAsync(Guid zoneId, Role role);
}
