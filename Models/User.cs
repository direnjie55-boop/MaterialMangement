using System.ComponentModel.DataAnnotations;

namespace MaterialMangement.Models;

public class User
{
    public int Id { get; set; }
    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? DisplayName { get; set; }

    [MaxLength(20)]
    public string Role { get; set; } = "User";
    public DateTime CreatedAt { get; set; } = DateTime.Now;

}
