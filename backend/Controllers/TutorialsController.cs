using backend.Dtos;
using backend.Repositories.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TutorialsController : ControllerBase
    {
        private readonly DbDibujofacilContext _context;

        public TutorialsController(DbDibujofacilContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetTutorials()
        {
            try
            {
                var tutorials = await _context.Tutorial
                    .Include(t => t.Author)
                    .Include(t => t.Categories)
                    .Include(t => t.TutorialContents)
                    .Include(t => t.Comments)
                        .ThenInclude(c => c.User)
                    .Include(t => t.Ratings)
                        .ThenInclude(r => r.User)
                    .AsNoTracking()
                    .ToListAsync();

                var tutorialDtos = tutorials.Select(t => new TutorialFullDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    Difficulty = t.Difficulty,
                    EstimatedDuration = t.EstimatedDuration,
                    PublicationDate = t.PublicationDate,
                    Status = t.Status,
                    Author = new UserDto
                    {
                        Id = t.Author.Id,
                        Name = t.Author.Name,
                        Email = t.Author.Email,
                        AvatarUrl = t.Author.AvatarUrl
                    },
                    Categories = t.Categories.Select(c => new CategoryDto
                    {
                        Id = c.Id,
                        Name = c.Name
                    }).ToList(),
                    Contents = t.TutorialContents.Select(tc => new TutorialContentDto
                    {
                        Id = tc.Id,
                        Type = tc.Type,
                        ContentBase64 = Convert.ToBase64String(tc.Content),
                        Order = tc.Order,
                        Description = tc.Description,
                        Title = tc.Title
                    }).OrderBy(tc => tc.Order).ToList(),
                    Comments = t.Comments.Select(c => new CommentDto
                    {
                        Id = c.Id,
                        Text = c.Comment1,
                        Date = c.Date,
                        Edited = c.Edited,
                        User = new UserDto
                        {
                            Id = c.User.Id,
                            Name = c.User.Name,
                            AvatarUrl = c.User.AvatarUrl
                        }
                    }).ToList(),
                    Ratings = t.Ratings.Select(r => new RatingDto
                    {
                        Id = r.Id,
                        Score = r.Score,
                        Date = r.Date,
                        User = new UserDto
                        {
                            Id = r.User.Id,
                            Name = r.User.Name
                        }
                    }).ToList(),
                    AverageRating = t.Ratings.Any() ? (double)t.Ratings.Average(r => r.Score) : 0
                }).ToList();

                return Ok(tutorialDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "Error al obtener los tutoriales",
                    Error = ex.Message
                });
            }
        }
    }
}