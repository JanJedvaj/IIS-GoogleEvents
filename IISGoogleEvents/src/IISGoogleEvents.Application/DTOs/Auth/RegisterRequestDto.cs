using System.ComponentModel.DataAnnotations;

namespace IISGoogleEvents.Application.DTOs.Auth;

public class RegisterRequestDto
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; set; } = null!;

    [Required]
    [StringLength(100, MinimumLength = 4)]
    public string Password { get; set; } = null!;
}
