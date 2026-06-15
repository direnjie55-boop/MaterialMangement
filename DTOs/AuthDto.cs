using System.ComponentModel.DataAnnotations;

namespace MaterialMangement.DTOs;

public class RegisterDto
{
    [Required(ErrorMessage = "用户名不能为空")]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "密码不能为空")]
    [MinLength(6, ErrorMessage = "密码长度不能少于6位")]
    public string Password { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? DisplayName { get; set; }
}

public class LoginDto
{
    [Required(ErrorMessage = "用户名不能为空")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "密码不能为空")]
    public string Password { get; set; } = string.Empty;

}

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string Role { get; set; } = string.Empty;

}