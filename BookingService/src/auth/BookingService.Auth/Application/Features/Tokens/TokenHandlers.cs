using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BookingService.Auth.Application.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BookingService.Auth.Application.Features.Tokens;

public class TokenHandlers(IOptionsMonitor<JwtSettings> options)
{
    public string GenerateAccessToken(List<Claim> claim, string secretKey)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(secretKey);
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claim),
            Expires = DateTime.UtcNow.AddMinutes(options.CurrentValue.ExpirationInMinutes),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
    
    public string GenerateAccessTokenFromRefreshToken(string refreshToken, string secretKey)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(secretKey);

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,   
            ValidateAudience = true, 
            ValidateLifetime = true,  
            ClockSkew = TimeSpan.Zero
        };

        try
        {
            //Returns all information about user state(is signed in, id, role)
            var principal = tokenHandler.ValidateToken(refreshToken, validationParameters, out SecurityToken validatedToken);
        
            //Returns users passport(id, email, role)
            var userClaims = principal.Identity as ClaimsIdentity;

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = userClaims, 
                Expires = DateTime.UtcNow.AddMinutes(options.CurrentValue.ExpirationInMinutes),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        catch (Exception ex)
        
        {
            throw new SecurityTokenException("Неверный или просроченный Refresh Token", ex);
        }
    }
}