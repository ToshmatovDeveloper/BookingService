using System.Security.Claims;

namespace BookingService.Auth.Application.Interfaces;

public interface ITokenHandlers
{
    string GenerateAccessToken(List<Claim> claim, string secretKey);
    
    string GenerateRefreshToken();
    
    string GenerateAccessTokenFromRefreshToken(string refreshToken, string secretKey);
}