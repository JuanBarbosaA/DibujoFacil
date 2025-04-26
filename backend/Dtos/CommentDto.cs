namespace backend.Dtos
{
    public class CommentDto
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime? Date { get; set; }
        public bool? Edited { get; set; }
        public UserDto User { get; set; }
    }
}
