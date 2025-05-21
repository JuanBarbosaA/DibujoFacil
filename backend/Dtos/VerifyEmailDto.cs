using System.ComponentModel.DataAnnotations;

namespace backend.Dtos
{
    public class VerifyEmailDto
    {
        [Required]
        public string Token { get; set; }
    }
}
