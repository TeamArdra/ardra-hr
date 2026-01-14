using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class UserSignupDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string RegNumber { get; set; } = string.Empty;

    [Required]
    public string Mobile { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string VitEmail { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string PersonalEmail { get; set; } = string.Empty;

    [Required]
    public string TeamNumber { get; set; } = string.Empty;

    [Required]
    public string Codename { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string ResidenceType { get; set; } = string.Empty;

    public string? HostelType { get; set; }
    public string? BlockRoom { get; set; }
}

public class UserLoginDto
{
    [Required]
    public string RegNumber { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class TokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = "bearer";
    public string Token { get; set; } = string.Empty;
}