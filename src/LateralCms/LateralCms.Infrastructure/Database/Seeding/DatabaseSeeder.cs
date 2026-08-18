using LateralCms.Application.Authentication;
using LateralCms.Domain.Entities;

namespace LateralCms.Infrastructure.Database.Seeding;

public static class DatabaseSeeder
{
    private static readonly string AdminRole = "admin";
    private static readonly string UserRole = "user";
    private static readonly string CmsRole = "cms";

    private static readonly string AdminUsername = "administrator";
    private static readonly string AdminPassword = "fb29ce7c-b3a5-4841-94ef-650085d774a3";

    private static readonly string CmsUsername = "cms_svc_acc";
    private static readonly string CmsPassword = "ca0b9baa-b815-473c-a988-58be92e0b8d6";


    public static void Seed(LateralCmsContext context, IPasswordHashService passwordHashService)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(passwordHashService);

        CreateRolesAsync(context).Wait();
        CreateAdminUserAsync(context, passwordHashService).Wait();
    }

    public static async Task SeedAsync(
        LateralCmsContext context,
        IPasswordHashService passwordHashService,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(passwordHashService);

        await CreateRolesAsync(context, cancellationToken);
        await CreateAdminUserAsync(context, passwordHashService, cancellationToken);
        await CreateCmsUserAsync(context, passwordHashService, cancellationToken);
    }

    private static async Task CreateAdminUserAsync(
        LateralCmsContext context,
        IPasswordHashService passwordHashService,
        CancellationToken cancellationToken = default)
    {
        var username = AdminUsername;

        if (context.Users.Any(user => user.Username == username))
        {
            return;
        }

        var role = context.UserRoles.FirstOrDefault(x => x.Name!.Trim().ToLower() == AdminRole)
            ?? throw new InvalidOperationException($"Role '{AdminRole}' not found.");

        var user = new User
        {
            Username = username,
            RoleId = role.Id,
            PasswordHash = passwordHashService.HashPassword(AdminPassword)
        };

        await context.Users.AddAsync(user, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task CreateCmsUserAsync(
        LateralCmsContext context,
        IPasswordHashService passwordHashService,
        CancellationToken cancellationToken = default)
    {
        var username = CmsUsername;

        if (context.Users.Any(user => user.Username == username))
        {
            return;
        }

        var role = context.UserRoles.FirstOrDefault(x => x.Name!.Trim().ToLower() == CmsRole)
            ?? throw new InvalidOperationException($"Role '{CmsRole}' not found.");

        var user = new User
        {
            Username = username,
            RoleId = role.Id,
            PasswordHash = passwordHashService.HashPassword(CmsPassword)
        };

        await context.Users.AddAsync(user, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task CreateRolesAsync(LateralCmsContext context, CancellationToken cancellationToken = default)
    {
        if (!context.UserRoles.Any(x => x.Name!.Trim().ToLower() == AdminRole))
        {
            await context.UserRoles.AddRangeAsync(new UserRole
            {
                Id = 1,
                Name = AdminRole
            });
        }

        if (!context.UserRoles.Any(x => x.Name!.Trim().ToLower() == UserRole))
        {
            await context.UserRoles.AddRangeAsync(new UserRole
            {
                Id = 2,
                Name = UserRole
            });
        }

        if (!context.UserRoles.Any(x => x.Name!.Trim().ToLower() == CmsRole))
        {
            await context.UserRoles.AddRangeAsync(new UserRole
            {
                Id = 3,
                Name = CmsRole
            });
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
