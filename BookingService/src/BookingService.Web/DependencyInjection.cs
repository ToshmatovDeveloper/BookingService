using gRPC.Clients;
using Microsoft.Extensions.DependencyInjection;

namespace BookingService.Web;

public static class DependencyInjection
{
    public static IServiceCollection AddAuthGrpcClient(this IServiceCollection services, string serverUrl)
    {
        services.AddGrpcClient<gRPC.Contracts.Client.IsAuthenticated.IsAuthenticatedClient>(options => 
        { 
            options.Address = new Uri(serverUrl); 
        });

        services.AddScoped<AuthGrpcClient>();

        return services;
    }
}