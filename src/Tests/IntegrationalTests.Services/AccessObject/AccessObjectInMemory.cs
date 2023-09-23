using Portal.Database.Context;

namespace IntegrationalTests.Services.AccessObject;

public class AccessObjectInMemory: PortalAccessObject
{
    public AccessObjectInMemory(): base()
    {
        InitRepo(new InMemoryDbContextFactory());
    }
}