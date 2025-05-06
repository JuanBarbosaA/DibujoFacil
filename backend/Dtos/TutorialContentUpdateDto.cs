namespace backend.Dtos
{
    public class TutorialContentUpdateDto
    {
        public List<int>? ContentIdsToDelete { get; set; }
        public List<IFormFile>? NewImages { get; set; }
    }
}


