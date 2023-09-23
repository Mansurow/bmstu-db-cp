using Microsoft.EntityFrameworkCore;
using Portal.Common.Models.Enums;

namespace Portal.Database.Context;

public class InMemoryDbContextFactory: IDbContextFactory
{
    private readonly string _dbName;
    public InMemoryDbContextFactory()
    {
        _dbName = "PortalDbTest" + Guid.NewGuid();
    }

    // Заглушка
    public PortalDbContext GetDbContext(Role role)
    {

        var builder = new DbContextOptionsBuilder<PortalDbContext>();
        builder.UseInMemoryDatabase(_dbName);
        var context = new PortalDbContext(builder.Options);

        return context;
    }
    
    public PortalDbContext GetDbContext()
    {

        var builder = new DbContextOptionsBuilder<PortalDbContext>();
        builder.UseInMemoryDatabase(_dbName);
        var context = new PortalDbContext(builder.Options);

        return context;
    }
}
