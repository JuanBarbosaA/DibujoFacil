using System.ComponentModel.DataAnnotations;

namespace backend.Dtos
{
    public class UserUpdateDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Range(0, int.MaxValue)]
        public int Points { get; set; }

        [Required]
        [RegularExpression("active|inactive")]
        public string Status { get; set; }

        [Range(1, int.MaxValue)]
        public int RoleId { get; set; }
    }
}
