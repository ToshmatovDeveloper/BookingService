using Grpc.Net.Client;
using MagicOnion.Client;
using BookingService.Auth.Grpc.Services;
using Response = BookingService.Auth.Grpc.Services.Response;

namespace gRPC.Clients;

public class AuthGrpcClient(GrpcChannel channel)
{
    private readonly IAuthenticationService _client = MagicOnionClient.Create<IAuthenticationService>(channel);

    public async Task<Response> CheckAsync(string token)
    {
        return await _client.CheckAsync(token);
    }
}