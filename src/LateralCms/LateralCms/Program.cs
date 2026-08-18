using LateralCms.Api.Authentication;
using LateralCms.Api.BackgroundServices;
using LateralCms.Api.Middleware;
using LateralCms.Application.Authentication;
using LateralCms.Application.Interfaces.Persistence;
using LateralCms.Application.Interfaces.Queue;
using LateralCms.Application.Services;
using LateralCms.Infrastructure.Authentication;
using LateralCms.Infrastructure.Database;
using LateralCms.Infrastructure.Database.Seeding;
using LateralCms.Infrastructure.Persistence;
using LateralCms.Infrastructure.Queue;
using Mapster;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddMapster();
builder.Services.AddOpenApi();
builder.Services.AddAuthentication(o =>
    {
        o.DefaultAuthenticateScheme = BasicAuthenticationHandler.DefaultScheme;
        o.DefaultChallengeScheme = BasicAuthenticationHandler.DefaultScheme;
    })
    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>(BasicAuthenticationHandler.DefaultScheme, _ => { });

builder.Services.AddAuthorization();
builder.Services.AddScoped<IUserCredentialValidator, DatabaseUserCredentialValidator>();
builder.Services.AddScoped<IPasswordHashService, AspNetCorePasswordHashService>();
builder.Services.AddSingleton<ICmsEventQueue, CmsEventQueue>();
builder.Services.AddScoped<ICmsEventService, CmsEventService>();
builder.Services.AddScoped<ICmsEntityService, CmsEntityService>();
builder.Services.AddScoped<ICmsEventProcessor, CmsEventProcessor>();
builder.Services.AddHostedService<CmsEventBatchWorker>();

InitializeDatabase(builder);

WebApplication app = builder.Build();

await ApplyDatabaseMigrationAsync(app);

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "LateralCms API v1");
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();


static void InitializeDatabase(WebApplicationBuilder builder)
{
    var connectionString = builder.Configuration.GetConnectionString("LateralCmsDatabase")
    ?? throw new InvalidOperationException("Connection string 'LateralCmsDatabase' was not found.");

    var connectionStringBuilder = new SqliteConnectionStringBuilder(connectionString);

    if (!Path.IsPathRooted(connectionStringBuilder.DataSource))
    {
        connectionStringBuilder.DataSource = Path.GetFullPath(
            connectionStringBuilder.DataSource,
            builder.Environment.ContentRootPath);
    }

    var databaseDirectory = Path.GetDirectoryName(connectionStringBuilder.DataSource);

    if (!string.IsNullOrWhiteSpace(databaseDirectory))
    {
        Directory.CreateDirectory(databaseDirectory);
    }

    var passwordHashService = new AspNetCorePasswordHashService();

    builder.Services.AddDbContext<LateralCmsContext>(options =>
    {
        options.UseSqlite(connectionStringBuilder.ToString())
            .UseSeeding((context, _) => DatabaseSeeder.Seed(
                (LateralCmsContext)context,
                passwordHashService))
            .UseAsyncSeeding((context, _, cancellationToken) => DatabaseSeeder.SeedAsync(
                (LateralCmsContext)context,
                passwordHashService,
                cancellationToken));
    });

    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
}

static async Task ApplyDatabaseMigrationAsync(WebApplication app)
{
    await using AsyncServiceScope scope = app.Services.CreateAsyncScope();
    LateralCmsContext context = scope.ServiceProvider.GetRequiredService<LateralCmsContext>();
    ILogger logger = scope.ServiceProvider
        .GetRequiredService<ILoggerFactory>()
        .CreateLogger("DatabaseMigration");

    try
    {
        await context.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied successfully.");
    }
    catch (Exception exception)
    {
        logger.LogCritical(exception, "Database migration failed during application startup.");
        throw;
    }
}
