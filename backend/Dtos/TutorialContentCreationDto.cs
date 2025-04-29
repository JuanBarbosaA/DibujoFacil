
using System.ComponentModel.DataAnnotations;

public class TutorialContentCreationDto
{
    [Required(ErrorMessage = "El archivo es requerido")]
    public IFormFile File { get; set; }

    [Required(ErrorMessage = "El orden es requerido")]
    [Range(1, 100, ErrorMessage = "El orden debe estar entre 1 y 100")]
    public int Order { get; set; }

    public string Title { get; set; }
    public string Description { get; set; }

}