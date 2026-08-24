using BookingService.Notification.Application.Settings;
using BookingService.Notification.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

namespace BookingService.IntegrationTests.notificationService;

public class NotificationServiceTestWebFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:16")
        .WithDatabase("notification_service_db")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private Respawner? _respawner;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.Configure<SmtpSettings>(options =>
            {
                options.Host = "127.0.0.1"; 
                options.Port = 1025; 
                options.User = "test-system@booking.com";
            });

            services.RemoveAll<NotificationDbContext>();

            services.AddScoped<NotificationDbContext>(_ =>
            {
                var optionsBuilder = new DbContextOptionsBuilder<NotificationDbContext>();
                optionsBuilder.UseNpgsql(_container.GetConnectionString());

                return new NotificationDbContext(optionsBuilder.Options);
            });
        });
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();

        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();

        await InitializeRespawner();
    }

    public new async Task DisposeAsync()
    {
        await _container.StopAsync();
        await _container.DisposeAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        var connection = new NpgsqlConnection(_container.GetConnectionString());
        await connection.OpenAsync();

        await _respawner!.ResetAsync(connection);
    }

    private async Task InitializeRespawner()
    {
        await using var connection = new NpgsqlConnection(_container.GetConnectionString());
        await connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(
            connection,
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = ["public"],
                TablesToIgnore = ["__EFMigrationsHistory"]
            });
    }
}