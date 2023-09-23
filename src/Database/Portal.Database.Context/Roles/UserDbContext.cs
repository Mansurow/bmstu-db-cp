using Microsoft.EntityFrameworkCore;
using Portal.Database.Models;

namespace Portal.Database.Context.Roles;

public class UserDbContext: PortalDbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
    {
    }
}