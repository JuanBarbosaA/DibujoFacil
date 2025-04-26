namespace backend.Dtos
{
    public class TutorialContentDto
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string ContentBase64 { get; set; }
        public int Order { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
    }
}
