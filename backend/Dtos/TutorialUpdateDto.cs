using System.ComponentModel.DataAnnotations;

namespace backend.Dtos
{
    public class TutorialUpdateDto
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; }

        [Required]
        [StringLength(50)]
        public string Difficulty { get; set; }

        [Required]
        public List<int> CategoryIds { get; set; }
    }
}
