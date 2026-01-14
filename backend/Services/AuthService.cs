using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace backend.Services;

public interface IAuthService
{
    string HashPassword(string password);
    bool VerifyPassword(string plainPassword, string hashedPassword);
    string CreateAccessToken(Dictionary<string, string> data, int expirationMinutes);
}

public class AuthService : IAuthService
{
    private const int Pbkdf2Iterations = 100_000;
    private const int SaltBytes = 16;
    private readonly IConfiguration _configuration;

    public AuthService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltBytes);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            Pbkdf2Iterations,
            HashAlgorithmName.SHA256,
            32);

        string saltB64 = Convert.ToBase64String(salt).TrimEnd('=');
        string hashB64 = Convert.ToBase64String(hash).TrimEnd('=');
        
        return $"pbkdf2_sha256${Pbkdf2Iterations}${saltB64}${hashB64}";
    }

    public bool VerifyPassword(string plainPassword, string hashedPassword)
    {
        try
        {
            var parts = hashedPassword.Split('$');
            if (parts.Length != 4) return false;

            int iterations = int.Parse(parts[1]);
            byte[] salt = Convert.FromBase64String(PadBase64(parts[2]));
            byte[] expectedHash = Convert.FromBase64String(PadBase64(parts[3]));

            byte[] computedHash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(plainPassword),
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                32);

            return CryptographicOperations.FixedTimeEquals(computedHash, expectedHash);
        }
        catch
        {
            return false;
        }
    }

    public string CreateAccessToken(Dictionary<string, string> data, int expirationMinutes)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"];
        var algorithm = jwtSettings["Algorithm"];

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = data.Select(kvp => new Claim(kvp.Key, kvp.Value)).ToList();
        
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string PadBase64(string base64)
    {
        int padding = 4 - (base64.Length % 4);
        if (padding < 4)
        {
            return base64 + new string('=', padding);
        }
        return base64;
    }
}