using Microsoft.EntityFrameworkCore;
using Portal.Common.Models.Enums;
using Portal.Database.Context.Roles;

namespace Portal.Database.Context;

public interface IDbContextFactory
{ 
    PortalDbContext GetDbContext();
    
    PortalDbContext GetDbContext(Role role);
}
