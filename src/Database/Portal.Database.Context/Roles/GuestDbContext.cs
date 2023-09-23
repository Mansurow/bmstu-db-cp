using Microsoft.EntityFrameworkCore;
using Portal.Database.Models;

namespace Portal.Database.Context.Roles;

public class GuestDbContext: PortalDbContext
{
    public GuestDbContext(DbContextOptions<GuestDbContext> options) : base(options)
    {
    }
}