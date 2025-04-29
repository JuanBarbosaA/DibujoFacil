using System.ComponentModel.DataAnnotations;

namespace backend.Dtos
{
    public class TutorialCreationDto
    {
        [Required]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "El título debe tener entre 5 y 200 caracteres")]
        public string Title { get; set; }

        [Required(ErrorMessage = "La descripción es requerida")]
        [StringLength(2000, ErrorMessage = "La descripción no puede exceder 2000 caracteres")]
        public string Description { get; set; }

        [Required]
        [RegularExpression("^(beginner|intermediate|advanced)$",
    ErrorMessage = "Dificultad no válida. Opciones válidas: beginner, intermediate, advanced")]
        public string Difficulty { get; set; }

        [Required(ErrorMessage = "La duración estimada es requerida")]
        [Range(10, 600, ErrorMessage = "La duración debe estar entre 10 y 600 minutos")]
        public int EstimatedDuration { get; set; }

        [Required(ErrorMessage = "Debe seleccionar al menos una categoría")]
        [MinLength(1, ErrorMessage = "Debe seleccionar al menos una categoría")]
        public List<int> CategoryIds { get; set; }

[Required]
[MinLength(1, ErrorMessage = "Mínimo 1 archivo")]
[MaxLength(10, ErrorMessage = "Máximo 10 archivos")]
public List<TutorialContentCreationDto> Contents { get; set; }
    }

}
