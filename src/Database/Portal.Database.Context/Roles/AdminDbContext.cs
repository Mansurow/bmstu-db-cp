using Microsoft.EntityFrameworkCore;
using Portal.Database.Models;

namespace Portal.Database.Context.Roles;

public class AdminDbContext: PortalDbContext
{
    public AdminDbContext(DbContextOptions<AdminDbContext> options) : base(options)
    {
    }
}