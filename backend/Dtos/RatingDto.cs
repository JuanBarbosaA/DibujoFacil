using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace backend.Dtos
{
    public class RatingDto
    {
        [JsonIgnore]
        public int Id { get; set; }

        [Required(ErrorMessage = "El puntaje es requerido")]
        [Range(1, 5, ErrorMessage = "La calificación debe ser entre 1 y 5 estrellas")]
        public int Score { get; set; }

        [JsonIgnore]
        public DateTime? Date { get; set; }

        [JsonIgnore]
        public UserDto? User { get; set; } 
    }
}
