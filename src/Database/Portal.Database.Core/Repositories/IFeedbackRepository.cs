using Portal.Common.Models;
using Portal.Common.Models.Enums;

namespace Portal.Database.Core.Repositories;

/// <summary>
/// Интерфейс репозитория отзывов
/// </summary>
public interface IFeedbackRepository
{
    /// <summary>
    /// Получить все отзывы по зоне
    /// </summary>
    /// <param name="zoneId">Идентификатор зоны</param>
    /// <returns>Список всех отзывов зоны</returns>
    Task<List<Feedback>> GetAllFeedbackByZoneAsync(Guid zoneId, Role role);
    
    /// <summary>
    /// Получить все отзывы по пользователю
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <returns>Список всех отзывов пользователя</returns>
    Task<List<Feedback>> GetAllFeedbackUserAsync(Guid userId, Role role);
    
    /// <summary>
    /// Получить все отзывы 
    /// </summary>
    /// <returns>Список всех отзывов</returns>
    Task<List<Feedback>> GetAllFeedbackAsync(Role role);
    
    /// <summary>
    /// Получить отзыв
    /// </summary>
    /// <param name="feedbackId">Идентификатор отзыва</param>
    /// <returns>Отзыв</returns>
    Task<Feedback> GetFeedbackAsync(Guid feedbackId, Role role);
    
    /// <summary>
    /// Добавить отзыв
    /// </summary>
    /// <param name="feedback">Данные нового отзыва</param>
    /// <returns></returns>
    Task InsertFeedbackAsync(Feedback feedback, Role role);
    
    /// <summary>
    /// Обновить отзыв
    /// </summary>
    /// <param name="feedback">Данные отзыва на обновление</param>
    /// <returns></returns>
    Task UpdateFeedbackAsync(Feedback feedback, Role role);
    
    /// <summary>
    /// Удаление отзыва
    /// </summary>
    /// <param name="feedbackId">Идентификатор отзыва</param>
    /// <returns></returns>
    Task DeleteFeedbackAsync(Guid feedbackId, Role role);
}
