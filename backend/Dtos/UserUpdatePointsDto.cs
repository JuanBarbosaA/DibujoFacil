using System.ComponentModel.DataAnnotations;

namespace backend.Dtos
{
    public class UserUpdatePointsDto
    {
        [Range(0, int.MaxValue)]
        public int Points { get; set; }
    }
}
