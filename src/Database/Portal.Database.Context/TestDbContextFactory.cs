using Portal.Common.Models.Enums;

namespace Portal.Database.Context;

public class TestDbContextFactory: IDbContextFactory
{
    private readonly PortalDbContext _portalDbContext;
    
    public TestDbContextFactory(PortalDbContext context)
    {
        _portalDbContext = context;
    }
    
    public PortalDbContext GetDbContext()
    {
        return _portalDbContext;
    }

    public PortalDbContext GetDbContext(Role role)
    {
        return _portalDbContext;
    }
}