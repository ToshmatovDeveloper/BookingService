using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BookingService.Auth.Application.Interfaces;
using BookingService.Auth.Application.Settings;
using BookingService.Auth.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames;

namespace BookingService.Auth.Application.Features.Tokens;

public class TokenProvider(IOptionsMonitor<JwtSettings> options)
{
    public string GenerateAccessToken(Account account)
    {
        string secretKey = options.CurrentValue.Secret;
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Sub, account.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, account.Email!),
            ]),
            Expires = DateTime.UtcNow.AddMinutes(options.CurrentValue.ExpirationInMinutes),
            SigningCredentials = credentials,
            Issuer = options.CurrentValue.Issuer,
            Audience = options.CurrentValue.Audience
        };

        var handler = new JsonWebTokenHandler();

        string token = handler.CreateToken(tokenDescriptor);

        return token;
    }

    public string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }

    public string GenerateAccessTokenFromRefreshToken(string refreshToken, string refreshSecretKey)
    {
        var tokenHandler = new JsonWebTokenHandler();
        var refreshKey = Encoding.UTF8.GetBytes(refreshSecretKey);

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(refreshKey), 
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        try
        {
            var validationResult = tokenHandler.ValidateToken(refreshToken, validationParameters);

            if (!validationResult.IsValid)
            {
                throw new SecurityTokenException("Invalid or expired refresh token");
            }

            var userId = validationResult.ClaimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? validationResult.ClaimsIdentity.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                throw new SecurityTokenException("The token is missing the user identifier.");
            }

            var accessClaims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
            };

            var accessKey = Encoding.UTF8.GetBytes(options.CurrentValue.Secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(accessClaims),
                Expires = DateTime.UtcNow.AddMinutes(options.CurrentValue.ExpirationInMinutes),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(accessKey),
                    SecurityAlgorithms.HmacSha256Signature) 
            };

            return tokenHandler.CreateToken(tokenDescriptor);
        }
        catch (Exception ex)
        {
            throw new SecurityTokenException("Error validating or generating token", ex);
        }
    }
}