using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MaterialMangement.Data;
using MaterialMangement.DTOs;
using MaterialMangement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace MaterialMangement.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly MaterialDbContext _context;
    private readonly IConfiguration _configuration;
    public AuthController(MaterialDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    ///<summary>
    /// 用户注册
    ///</summary>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
    {
        if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
        {
            return BadRequest(new { message = "用户名已存在" });
        }
        var user = new User
        {
            Username = dto.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            DisplayName = dto.DisplayName,
            Role = "User",
            CreatedAt = DateTime.Now
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = GenerateJwtToken(user);

        return Ok(new AuthResponseDto
        {
            Token = token.Token,
            Expiration = token.Expiration,
            Username = user.Username,
            DisplayName = user.DisplayName,
            Role = user.Role
        });
    }

    /// <summary>
    /// 用户登录
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "用户名或密码错误" });
        }

        var token = GenerateJwtToken(user);

        return Ok(new AuthResponseDto
        {
            Token = token.Token,
            Expiration = token.Expiration,
            Username = user.Username,
            DisplayName = user.DisplayName,
            Role = user.Role
        });
    }

    private (string Token, DateTime Expiration) GenerateJwtToken(User user)
    {
        var jwtSection = _configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));
        var expiration = DateTime.Now.AddMinutes(double.Parse(jwtSection["ExpirationMinutes"]!));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("DisplayName", user.DisplayName ?? user.Username)
        };

        var token = new JwtSecurityToken(
            issuer: jwtSection["Issuer"],
            audience: jwtSection["Audience"],
            claims: claims,
            expires: expiration,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expiration);
    }
}