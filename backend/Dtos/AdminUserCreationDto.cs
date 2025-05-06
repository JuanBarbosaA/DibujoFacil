using System.ComponentModel.DataAnnotations;

namespace backend.Dtos
{
    public class AdminUserCreationDto
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }

        [Range(1, int.MaxValue)]
        public int RoleId { get; set; } = 2; 

        [Range(0, int.MaxValue)]
        public int Points { get; set; } = 0;

        [RegularExpression("active|inactive")]
        public string Status { get; set; } = "active";

        [Url]
        public string AvatarUrl { get; set; }
    }
}
