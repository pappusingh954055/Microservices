using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Identity.Infrastructure.Security;

public class JwtService : IJwtService
{
    private readonly IConfiguration _config;

    public JwtService(IConfiguration config)
    {
        _config = config;
    }

    public AuthResponse Generate(User user, List<string> roles)
    {
        var jwt = _config.GetSection("Jwt");

        // 1. Basic User Claims
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName!)
        };

        // 2. Roles add karna using Foreach Loop (Verified Method)
        // Isse Inventory API ko har role alag se Authorize karne mein asani hogi
        foreach (var role in roles)
        {
            if (!string.IsNullOrEmpty(role))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
               
            }
        }

        // 3. Security Key aur Credentials setup
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // 4. Expiration Logic (Exactly 1 Minute as per your setting)
        var expires = DateTime.UtcNow.AddMinutes(double.Parse(jwt["AccessTokenMinutes"]!));

        // 5. Token Object Creation
        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,            
            expires: expires,
            signingCredentials: creds
        );

        // 6. Token Serialization
        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        // Refresh Token generate karna (Ise DB mein save zaroor karein)
        var refreshToken = Guid.NewGuid().ToString("N");

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expires,
            Roles = roles,
            Email = user.Email!
        };
    }
}
