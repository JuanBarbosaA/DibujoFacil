namespace backend.Dtos
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Name { get; set; } // Quitar [Required] si no es necesario
        public string Email { get; set; }
        public string? AvatarUrl { get; set; } // Hacer nullable
    }
}
