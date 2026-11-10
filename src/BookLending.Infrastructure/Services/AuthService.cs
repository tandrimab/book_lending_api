using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BookLending.Application.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BookLending.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;

    public AuthService(IConfiguration configuration) => _configuration = configuration;

    public LoginResponse? Login(LoginRequest request)
    {
        var demoUsername = GetRequired("Auth:DemoUser:Username");
        var demoPassword = GetRequired("Auth:DemoUser:Password");

        bool validUser = FixedTimeEquals(request.Username, demoUsername);
        bool validPassword = FixedTimeEquals(request.Password, demoPassword);

        if (!validUser || !validPassword)
            return null;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, request.Username),
            new(ClaimTypes.Name, request.Username),
            new(ClaimTypes.Role, "Librarian"),
        };

        var expiryMinutes = _configuration.GetValue("Jwt:ExpiryMinutes", 30);
        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GetRequired("Jwt:Key")));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: GetRequired("Jwt:Issuer"),
            audience: GetRequired("Jwt:Audience"),
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        return new LoginResponse(jwt, expiresAt);
    }

    private string GetRequired(string key) =>
        _configuration[key] ?? throw new InvalidOperationException($"Missing configuration value '{key}'.");

    private static bool FixedTimeEquals(string a, string b) =>
        CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(a), Encoding.UTF8.GetBytes(b));
}
