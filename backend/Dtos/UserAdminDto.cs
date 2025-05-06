namespace backend.Dtos
{

    public class UserAdminDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public string Status { get; set; }
        public int? Points { get; set; }

    }
}
