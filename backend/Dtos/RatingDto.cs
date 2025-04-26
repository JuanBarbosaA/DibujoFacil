namespace backend.Dtos
{
    public class RatingDto
    {
        public int Id { get; set; }
        public int? Score { get; set; }
        public DateTime? Date { get; set; }
        public UserDto User { get; set; }
    }
}
