using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Npgsql.Internal.TypeMapping;
using Portal.Common.Converter;
using Portal.Common.Models;
using Portal.Common.Models.Enums;
using Portal.Database.Context;
using Portal.Database.Core.Repositories;

namespace Portal.Database.Repositories.NpgsqlRepositories;

public class UserRepository: BaseRepository, IUserRepository
{
    private readonly IDbContextFactory _contextFactory;

    public UserRepository(IDbContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
    }
    
    public Task<List<User>> GetAllUsersAsync(Role role)
    {
        return _contextFactory.GetDbContext(role)
            .Users
            .Select(u => UserConverter.ConvertDbModelToAppModel(u))
            .ToListAsync();
    }

    public async Task<User> GetUserByIdAsync(Guid userId, Role role)
    {
        var user = await _contextFactory.GetDbContext(role)
            .Users.FirstAsync(u => u.Id == userId);

        return UserConverter.ConvertDbModelToAppModel(user);
    }

    public Task<List<User>> GetAdmins()
    {
        return _contextFactory.GetDbContext(Role.Administrator)
            .Users.Where(u => u.Role == Role.Administrator)
            .Select(u => UserConverter.ConvertDbModelToAppModel(u))
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<User> GetUserByEmailAsync(string email, Role role)
    {
        var user = await _contextFactory.GetDbContext(role)
            .Users.FirstAsync(u => u.Email == email);

        return UserConverter.ConvertDbModelToAppModel(user);
    }

    public async Task InsertUserAsync(User user, Role role)
    {
        var context = _contextFactory.GetDbContext(role);
        var userDb = UserConverter.ConvertAppModelToDbModel(user);
        
        await context.Users.AddAsync(userDb);
        await context.SaveChangesAsync();
    }

    public async Task UpdateUserAsync(User user, Role role)
    {
        var context = _contextFactory.GetDbContext(role);
        var userDb = await context.Users.FirstAsync(u => u.Id == user.Id);

        userDb.LastName = user.LastName;
        userDb.FirstName = user.FirstName;
        userDb.MiddleName = user.MiddleName;
        userDb.Birthday = user.Birthday;
        userDb.Email = user.Email;
        userDb.Gender = user.Gender;
        userDb.Phone = user.Phone;
        userDb.Role = user.Role;
        userDb.PasswordHash = user.PasswordHash;

        await context.SaveChangesAsync();
    }

    public async Task DeleteUserAsync(Guid userId, Role role)
    {
        var context = _contextFactory.GetDbContext(role);
        var user = await context.Users.FirstAsync(u => u.Id == userId);
        
        context.Users.Remove(user);
        await context.SaveChangesAsync();
    }
}