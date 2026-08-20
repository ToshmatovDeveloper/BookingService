using System.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace BookingService.Infrastructure.Connection;

public class DbConnectionFactory(IConfiguration configuration)
{
    private readonly string? _connectionString = configuration.GetConnectionString("DefaultConnection");

    public IDbConnection CreateConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }
}