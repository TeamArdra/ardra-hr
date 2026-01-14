using Microsoft.AspNetCore.Mvc;
using backend.DTOs;
using backend.Services;

namespace backend.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IAuthService _authService;
    private readonly IConfiguration _configuration;

    public AuthController(
        IUserService userService,
        IAuthService authService,
        IConfiguration configuration)
    {
        _userService = userService;
        _authService = authService;
        _configuration = configuration;
    }

    [HttpPost("signup")]
    public async Task<ActionResult<TokenResponse>> Signup([FromBody] UserSignupDto userDto)
    {
        // Check if user already exists
        var existingUser = await _userService.GetUserByEmailOrRegNumberAsync(
            userDto.VitEmail, 
            userDto.RegNumber);

        if (existingUser != null)
        {
            return BadRequest(new { detail = "User with this email or registration number already exists" });
        }

        // Hash password
        var hashedPassword = _authService.HashPassword(userDto.Password);

        // Create user
        var user = await _userService.CreateUserAsync(userDto, hashedPassword);

        // Get expiration minutes from config
        var expirationMinutes = _configuration.GetValue<int>("JwtSettings:ExpirationMinutes");

        // Create access token
        var tokenData = new Dictionary<string, string>
        {
            { "sub", user.VitEmail },
            { "reg_number", user.RegNumber }
        };

        var accessToken = _authService.CreateAccessToken(tokenData, expirationMinutes);

        return Ok(new TokenResponse
        {
            AccessToken = accessToken,
            TokenType = "bearer",
            Token = accessToken
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<TokenResponse>> Login([FromBody] UserLoginDto credentials)
    {
        // Find user by registration number
        var user = await _userService.GetUserByRegNumberAsync(credentials.RegNumber);

        if (user == null)
        {
            return Unauthorized(new { detail = "Invalid email or password" });
        }

        // Verify password
        if (!_authService.VerifyPassword(credentials.Password, user.Password))
        {
            return Unauthorized(new { detail = "Invalid email or password" });
        }

        // Get expiration minutes from config
        var expirationMinutes = _configuration.GetValue<int>("JwtSettings:ExpirationMinutes");

        // Create access token
        var tokenData = new Dictionary<string, string>
        {
            { "sub", credentials.RegNumber },
            { "reg_number", user.RegNumber }
        };

        var accessToken = _authService.CreateAccessToken(tokenData, expirationMinutes);

        return Ok(new TokenResponse
        {
            AccessToken = accessToken,
            TokenType = "bearer",
            Token = accessToken
        });
    }
}