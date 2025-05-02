using System.ComponentModel.DataAnnotations;

namespace backend.Dtos
{
    public class RatingCreationDto
    {
        [Required]
        [Range(1, 5, ErrorMessage = "La calificación debe ser entre 1 y 5")]
        public int Score { get; set; }
    }
}
