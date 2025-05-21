using System.ComponentModel.DataAnnotations;

namespace backend.Dtos
{
    public class ForgotPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
