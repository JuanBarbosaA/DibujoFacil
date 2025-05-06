namespace backend.Dtos
{
    public class AdminTutorialDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public DateTime PublicationDate { get; set; }
        public string Difficulty { get; set; }
        public AdminTutorialAuthorDto Author { get; set; }
        public List<string> Categories { get; set; }
        public int ContentCount { get; set; }
        public int CommentsCount { get; set; }
        public double AverageRating { get; set; }
        public TutorialContentImageDto LastImage { get; set; }
    }
}
