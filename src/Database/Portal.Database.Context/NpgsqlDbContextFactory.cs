using Microsoft.Extensions.Logging;
using Portal.Common.Models.Enums;
using Portal.Database.Context.Roles;

namespace Portal.Database.Context
{
    public class NpgsqlDbContextFactory: IDbContextFactory
    {
        private readonly GuestDbContext _guestDbContext;
        private readonly UserDbContext _userDbContext;
        private readonly AdminDbContext _adminDbContext;
        private readonly ILogger<NpgsqlDbContextFactory> _logger;

        public NpgsqlDbContextFactory( 
            AdminDbContext adminDbContext, 
            UserDbContext userDbContext, 
            GuestDbContext guestDbContext, 
            ILogger<NpgsqlDbContextFactory> logger)
        {
            _adminDbContext = adminDbContext;
            _userDbContext = userDbContext;
            _guestDbContext = guestDbContext;
            _logger = logger;
        }

        public PortalDbContext GetDbContext()
        {
            return _adminDbContext;
        }
        
        public PortalDbContext GetDbContext(Role role)
        {
            
            PortalDbContext context;

            switch (role)
            {
                case Role.Administrator:
                    _logger.LogInformation("The context {DbContext} was got by the role {Permissions}", nameof(AdminDbContext), role);
                    context = _adminDbContext;
                    break;
                case Role.User:
                    _logger.LogInformation("The context {DbContext} was got by the role {Permissions}", nameof(UserDbContext), role);
                    context = _userDbContext;
                    break;
                default:
                    _logger.LogInformation("The context {DbContext} was got by the role {Permissions}", nameof(GuestDbContext), role);
                    context = _guestDbContext;
                    break;
            }
            
            
            return context;
        }
    }
}
