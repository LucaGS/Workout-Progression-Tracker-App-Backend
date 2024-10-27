using System.ComponentModel.DataAnnotations;

public class AppUser
{
    public int UserId { get; set; }

    [Required]
    public string Username { get; set; }

    [Required]
    public string Password { get; set; }
}
