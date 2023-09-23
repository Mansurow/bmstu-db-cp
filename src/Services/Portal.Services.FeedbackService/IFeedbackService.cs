using Portal.Common.Models;
using Portal.Common.Models.Enums;
using Portal.Services.FeedbackService.Exceptions;
using Portal.Services.ZoneService.Exceptions;

namespace Portal.Services.FeedbackService
{
    /// <summary>
    /// Интерфейс сервиса отзывов
    /// </summary>
    public interface IFeedbackService
    {
        /// <summary>
        /// Получить все отзывы зоны
        /// </summary>
        /// <param name="zoneId">Идентификатор зоны</param>
        /// <returns>Список отзывов зоны</returns>
        /// <exception cref="ZoneNotFoundException">Зона не найдена</exception>
        Task<List<Feedback>> GetAllFeedbackByZoneAsync(Guid zoneId, Role permissions);
        
        /// <summary>
        /// Получить все отзывы
        /// </summary>
        /// <returns>Список всех отзывов</returns>
        Task<List<Feedback>> GetAllFeedbackAsync(Role permissions);
        
        /// <summary>
        /// Добавить отзыв 
        /// </summary>
        /// <param name="zoneId">Идентификатор зоны</param>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <param name="mark">Оценка</param>
        /// <param name="description">Сообщение отзыва</param>
        /// <returns>Идентификатор нового отзыва</returns>
        Task<Guid> AddFeedbackAsync(Guid zoneId, Guid userId, double mark, string description, Role permissions);
        
        /// <summary>
        /// Обновить отзыв
        /// </summary>
        /// <param name="feedback">Данные отзыв для обновления</param>
        /// <exception cref="FeedbackUpdateException">При обновлении отзыва</exception>
        Task UpdateFeedbackAsync(Feedback feedback, Role permissions);
        
        /// <summary>
        /// Обновить рейтинг зоны
        /// </summary>
        /// <param name="zoneId">Идентификатор зоны</param>
        /// <exception cref="ZoneUpdateException">При обновлении рейтинга зоны</exception>
        Task UpdateZoneRatingAsync(Guid zoneId, Role permissions);
        
        /// <summary>
        /// Удалить отзыв
        /// </summary>
        /// <param name="feedbackId">Идентификатор зоны</param>
        /// <exception cref="FeedbackNotFoundException">Отзыв не найден</exception>
        /// <exception cref="FeedbackRemoveException">При удалении рейтинга зоны</exception>
        Task RemoveFeedbackAsync(Guid feedbackId, Role permissions);
    }
}
