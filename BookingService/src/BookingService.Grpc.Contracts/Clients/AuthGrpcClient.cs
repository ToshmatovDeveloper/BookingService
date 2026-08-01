using Grpc.Net.Client;
using MagicOnion;
using MagicOnion.Client;
using Response = BookingService.Auth.Grpc.Services.Response;

namespace gRPC.Clients;

public class AuthGrpcClient
{
    public async UnaryResult<Response> CheckAsync(string token)
    {
        using var channel = GrpcChannel.ForAddress("https://localhost:8139");
        
        var client = MagicOnionClient.Create<BookingService.Auth.Grpc.Services.IAuthenticationService>(channel);
        
        return await client.CheckAsync(token);
    }
}