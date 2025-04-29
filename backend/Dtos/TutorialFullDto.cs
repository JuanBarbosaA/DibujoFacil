namespace backend.Dtos
{
    public class TutorialFullDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Difficulty { get; set; } 
        public int? EstimatedDuration { get; set; } 
        public DateTime? PublicationDate { get; set; } 
        public string Status { get; set; }
        public UserDto Author { get; set; }
        public List<CategoryDto> Categories { get; set; }
        public List<TutorialContentDto> Contents { get; set; }
        public List<CommentDto> Comments { get; set; }
        public List<RatingDto> Ratings { get; set; }
        public double AverageRating { get; set; }
    }
}