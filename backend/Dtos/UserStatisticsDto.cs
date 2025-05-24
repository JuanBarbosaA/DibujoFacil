namespace backend.Dtos
{
    public class UserStatisticsDto
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public int TutorialCount { get; set; }
        public int CommentCount { get; set; }
        public double? AverageRating { get; set; }
    }
}
