using System.ComponentModel.DataAnnotations;

namespace backend.Dtos
{
    public class CommentCreationDto
    {
        [Required(ErrorMessage = "El comentario no puede estar vacío")]
        [StringLength(1000, ErrorMessage = "El comentario no puede exceder 1000 caracteres")]
        public string Text { get; set; }
    }
}
