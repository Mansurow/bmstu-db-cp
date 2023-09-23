using Portal.Common.Models;
using Portal.Common.Models.Enums;

namespace Portal.Database.Core.Repositories;

/// <summary>
/// Интерфейс репозитория пользователя
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Получить всех пользователя
    /// </summary>
    /// <returns>Список пользователей</returns>
    Task<List<User>> GetAllUsersAsync(Role role);

    /// <summary>
    /// Получить всех админов
    /// </summary>
    /// <returns>Список админов</returns>
    Task<List<User>> GetAdmins();
    
    /// <summary>
    /// Получить пользователя
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <returns>Пользователь</returns>
    Task<User> GetUserByIdAsync(Guid userId, Role role);
    
    /// <summary>
    /// Получить пользователя по электронной почтой
    /// </summary>
    /// <param name="email">Электронная почта</param>
    /// <returns>Пользователь</returns>
    Task<User> GetUserByEmailAsync(string email, Role role);
    
    /// <summary>
    /// Добавить пользователя
    /// </summary>
    /// <param name="user">Данные новые пользователя</param>
    /// <returns></returns>
    Task InsertUserAsync(User user, Role role);
    
    /// <summary>
    /// Обновить пользователя
    /// </summary>
    /// <param name="user">Данные пользователя для обновления</param>
    /// <returns></returns>
    Task UpdateUserAsync(User user, Role role);
    
    /// <summary>
    /// Удалить пользователя
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <returns></returns>
    Task DeleteUserAsync(Guid userId, Role role);
}
