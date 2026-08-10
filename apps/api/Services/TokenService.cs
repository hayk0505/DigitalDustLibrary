using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DigitalDustLibrary.Api.Models;
using Microsoft.IdentityModel.Tokens;

namespace DigitalDustLibrary.Api.Services;

public class JwtOptions
{
    public const string SectionName = "Jwt";
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public required string SigningKey { get; set; }
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 30;
}

/// <summary>
/// Creates access tokens (short-lived JWT, kept in memory by the client per
/// Admin_Panel_Build_Spec.md) and refresh tokens (long-lived random value, sent
/// only via httpOnly cookie — see RefreshToken.cs and AuthEndpoints).
/// </summary>
public class TokenService(JwtOptions options)
{
    public string CreateAccessToken(ApplicationUser user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()), // PascalCase, e.g. "Owner" — see Enums.cs
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(options.AccessTokenMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>Raw refresh token value — put in the httpOnly cookie, never stored as-is.</summary>
    public static string GenerateRefreshTokenValue() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    public static string HashToken(string rawToken) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));

    public DateTimeOffset RefreshTokenExpiry() => DateTimeOffset.UtcNow.AddDays(options.RefreshTokenDays);
}
