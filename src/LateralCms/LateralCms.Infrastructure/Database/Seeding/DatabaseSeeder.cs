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

    private static readonly string UserUsername = "john_doe";
    private static readonly string UserPassword = "54588b8f-8756-4916-a914-38db61760b89";

    private static readonly string CmsUsername = "cms_svc_acc";
    private static readonly string CmsPassword = "ca0b9baa-b815-473c-a988-58be92e0b8d6";

    public static void Seed(LateralCmsContext context, IPasswordHashService passwordHashService)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(passwordHashService);

        CreateRoles(context);

        CreateUser(context, passwordHashService, AdminUsername, AdminPassword, AdminRole);
        CreateUser(context, passwordHashService, UserUsername, UserPassword, UserRole);
        CreateUser(context, passwordHashService, CmsUsername, CmsPassword, CmsRole);
    }

    public static async Task SeedAsync(
        LateralCmsContext context,
        IPasswordHashService passwordHashService,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(passwordHashService);

        await CreateRolesAsync(context, cancellationToken);

        await CreateUserAsync(context, passwordHashService, AdminUsername, AdminPassword, AdminRole, cancellationToken);
        await CreateUserAsync(context, passwordHashService, UserUsername, UserPassword, UserRole, cancellationToken);
        await CreateUserAsync(context, passwordHashService, CmsUsername, CmsPassword, CmsRole, cancellationToken);
    }

    private static void CreateRoles(LateralCmsContext context)
    {
        var missingRoles = GetMissingRoles(context);

        if (missingRoles.Count <= 0)
        {
            return;
        }

        context.UserRoles.AddRange(missingRoles);
        context.SaveChanges();
    }

    private static async Task CreateRolesAsync(LateralCmsContext context, CancellationToken cancellationToken = default)
    {
        var missingRoles = GetMissingRoles(context);

        if (missingRoles.Count <= 0)
        {
            return;
        }

        await context.UserRoles.AddRangeAsync(missingRoles, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static List<UserRole> GetMissingRoles(LateralCmsContext context)
    {
        var missingRoles = new List<UserRole>();

        if (!context.UserRoles.Any(x => x.Name!.Trim().ToLower() == AdminRole))
        {
            missingRoles.Add(new UserRole
            {
                Id = 1,
                Name = AdminRole
            });
        }

        if (!context.UserRoles.Any(x => x.Name!.Trim().ToLower() == UserRole))
        {
            missingRoles.Add(new UserRole
            {
                Id = 2,
                Name = UserRole
            });
        }

        if (!context.UserRoles.Any(x => x.Name!.Trim().ToLower() == CmsRole))
        {
            missingRoles.Add(new UserRole
            {
                Id = 3,
                Name = CmsRole
            });
        }

        return missingRoles;
    }

    private static void CreateUser(LateralCmsContext context, IPasswordHashService passwordHashService,
        string username, string password, string role, CancellationToken cancellationToken = default)
    {
        if (context.Users.Any(user => user.Username == username))
        {
            return;
        }

        var dbRole = context.UserRoles.FirstOrDefault(x => x.Name!.Trim().ToLower() == role)
            ?? throw new InvalidOperationException($"Role '{role}' not found.");

        var user = new User
        {
            Username = username,
            RoleId = dbRole.Id,
            PasswordHash = passwordHashService.HashPassword(password)
        };

        context.Users.Add(user);
        context.SaveChanges();
    }

    private static async Task CreateUserAsync(LateralCmsContext context, IPasswordHashService passwordHashService,
        string username, string password, string role, CancellationToken cancellationToken = default)
    {
        if (context.Users.Any(user => user.Username == username))
        {
            return;
        }

        var dbRole = context.UserRoles.FirstOrDefault(x => x.Name!.Trim().ToLower() == role)
            ?? throw new InvalidOperationException($"Role '{role}' not found.");

        var user = new User
        {
            Username = username,
            RoleId = dbRole.Id,
            PasswordHash = passwordHashService.HashPassword(password)
        };

        await context.Users.AddAsync(user, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
