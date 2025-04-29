namespace backend.Repositories.Models
{
    public class TutorialCategory
    {
        public int TutorialId { get; set; }
        public int CategoryId { get; set; }

        public Tutorial Tutorial { get; set; }
        public Category Category { get; set; }
    }
}
