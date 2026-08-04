using Grpc.Net.Client;
using MagicOnion.Client;
using Microsoft.Extensions.Configuration;
using Response = BookingService.Auth.Grpc.Services.Response;

namespace gRPC.Clients;

public class AuthGrpcClient(IConfiguration configuration)
{
    private readonly Lazy<GrpcChannel> _channel = new(() => 
    {
        var address = configuration["GrpcSettings:AuthServiceUrl"] ?? "https://localhost:8139";
        return GrpcChannel.ForAddress(address);
    });

    public async Task<Response> CheckAsync(string token, CancellationToken cancellationToken = default)
    {
        var client = MagicOnionClient.Create<BookingService.Auth.Grpc.Services.IAuthenticationService>(_channel.Value);
        
        return await client.CheckAsync(token);
    }
}