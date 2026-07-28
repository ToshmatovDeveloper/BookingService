using Grpc.Core;
using gRPC.Contracts.Client;
using Microsoft.Extensions.Logging; 

namespace gRPC.Clients;

public class AuthGrpcClient(
    IsAuthenticated.IsAuthenticatedClient client,
    ILogger<AuthGrpcClient> logger) 
{
    public async Task<(bool IsAuthenticated, string? UserId)> CheckTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var request = new Request { Token = token };
        
        logger.LogInformation("gRPC Клиент: Отправка токена на проверку. Длина токена: {Length}", token?.Length);

        try
        {
            var response = await client.CheckAsync(request, cancellationToken: cancellationToken);
            
            logger.LogInformation("gRPC Клиент: Получен ответ. Статус: {Status}, UserId: {UserId}", 
                response.IsAuthentificated, response.UserId);
            
            return (response.IsAuthentificated, response.UserId);
        }
        catch (RpcException ex)
        {
            logger.LogError(ex, "gRPC Клиент: Ошибка вызова gRPC сервера Auth. Статус-код: {Code}", ex.StatusCode);
            return (false, null);
        }
    }
}
