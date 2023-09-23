using Portal.Database.Context;
using Microsoft.EntityFrameworkCore;
using Portal.Common.Models;
using Portal.Common.Models.Enums;
using Portal.Configuration;
using Portal.Database.Context.Roles;
using Portal.Database.Repositories.NpgsqlRepositories;
using Portal.Services.OauthService;
using Portal.Services.UserService;
using Serilog;

namespace Portal.Extensions;

public static class WebApplicationProviderExtension
{
    public static async Task<WebApplication> MigrateDatabaseAsync(this WebApplication app)
    {
        // Миграция только через администратора
        // await using var context = app.Services.GetRequiredService<PortalDbContext>(); - Scope or Trans
        var context = app.Services.GetRequiredService<AdminDbContext>();
        await context.Database.MigrateAsync();
        
        return app;
    }
    
    public static async Task<WebApplication> AddPortalAdministrator(this WebApplication app)
    {
        var adminOptions = app.Configuration.GetSection("AdministratorConfiguration").Get<AdministratorConfiguration>();

        using var serviceScope = app.Services.CreateScope();
        var services = serviceScope.ServiceProvider;
        var userService = services.GetRequiredService<IUserService>();
            
        if (adminOptions is null)
        {
            await userService.CreateAdmin("admin","admin");
        }
        else
        {
            await userService.CreateAdmin(adminOptions.Login, adminOptions.Password);
        }

        return app;
    }
}