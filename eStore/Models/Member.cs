using System.ComponentModel.DataAnnotations;

public class Member
{
    public int MemberId { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Company name is required")]
    [StringLength(100, ErrorMessage = "Company name cannot be longer than 100 characters")]
    public string CompanyName { get; set; }

    [Required(ErrorMessage = "City is required")]
    [StringLength(50, ErrorMessage = "City cannot be longer than 50 characters")]
    public string City { get; set; }

    [Required(ErrorMessage = "Country is required")]
    [StringLength(50, ErrorMessage = "Country cannot be longer than 50 characters")]
    public string Country { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
    public string Password { get; set; }
}