using BookingService.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

namespace BookingService.IntegrationTests;

public class BookingTestWebFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:16") 
        .WithDatabase("booking_service_db")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private Respawner? _respawner;
    
    protected override void ConfigureWebHost(IWebHostBuilder builder) 
    {
        builder.ConfigureTestServices(services => 
        {
            services.RemoveAll<ApplicationDbContext>();
        
            services.AddScoped<ApplicationDbContext>(_ => 
            {
                var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
                optionsBuilder.UseNpgsql(_container.GetConnectionString());
            
                return new ApplicationDbContext(optionsBuilder.Options);
            }); 
        }); 
    }
    
    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
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
        var connection = new NpgsqlConnection(_container.GetConnectionString());
        await connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(
            connection,
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = ["public"]
        });
    }
}