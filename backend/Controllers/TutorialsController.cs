using backend.Dtos;
using backend.Repositories.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TutorialsController : ControllerBase
    {
        private readonly DbDibujofacilContext _context;
        private readonly ILogger<TutorialsController> _logger;

        public TutorialsController(
            DbDibujofacilContext context,
            ILogger<TutorialsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetTutorials()
        {
            try
            {
                var tutorials = await _context.Tutorial
                    .Include(t => t.Author)
                    .Include(t => t.TutorialCategories)
                        .ThenInclude(tc => tc.Category)  
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
                    Categories = t.TutorialCategories  
                        .Select(tc => new CategoryDto
                        {
                            Id = tc.Category.Id,
                            Name = tc.Category.Name
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
                    AverageRating = t.Ratings.Any()
                        ? (double)t.Ratings.Average(r => r.Score)
                        : 0
                }).ToList();

                return Ok(tutorialDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo tutoriales");
                return StatusCode(500, new
                {
                    Message = "Error al obtener los tutoriales",
                    Error = ex.Message
                });
            }
        }

        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> CreateTutorial(
            [FromForm] TutorialCreationDto tutorialDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                var existingCategories = await _context.Categories
                    .Where(c => tutorialDto.CategoryIds.Contains(c.Id))
                    .Select(c => c.Id)
                    .ToListAsync();

                if (existingCategories.Count != tutorialDto.CategoryIds?.Count)
                    return BadRequest("Una o más categorías no existen");

                var tutorial = new Tutorial
                {
                    Title = tutorialDto.Title,
                    Description = tutorialDto.Description,
                    Difficulty = tutorialDto.Difficulty,
                    EstimatedDuration = tutorialDto.EstimatedDuration,
                    AuthorId = userId,
                    PublicationDate = DateTime.UtcNow,
                    Status = "pending",
                    TutorialCategories = new List<TutorialCategory>()  
                };

                await _context.Tutorial.AddAsync(tutorial);
                await _context.SaveChangesAsync();

                foreach (var contentDto in tutorialDto.Contents)
                {
                    using var memoryStream = new MemoryStream();
                    await contentDto.File.CopyToAsync(memoryStream);

                    _context.TutorialContents.Add(new TutorialContent
                    {
                        TutorialId = tutorial.Id,
                        Type = contentDto.File.ContentType, 
                        Content = memoryStream.ToArray(),
                        Order = contentDto.Order,
                        Title = contentDto.Title,
                        Description = contentDto.Description
                    });
                }

                var tutorialCategories = tutorialDto.CategoryIds.Select(cId =>
                    new TutorialCategory
                    {
                        TutorialId = tutorial.Id,
                        CategoryId = cId
                    });

                await _context.TutorialCategories.AddRangeAsync(tutorialCategories);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return CreatedAtAction(nameof(GetTutorials), new
                {
                    Message = "Tutorial creado exitosamente",
                    TutorialId = tutorial.Id
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creando tutorial");
                return StatusCode(500, new
                {
                    Message = "Error interno del servidor",
                    Error = ex.Message
                });
            }
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _context.Categories
                    .AsNoTracking()
                    .Select(c => new CategoryDto
                    {
                        Id = c.Id,
                        Name = c.Name
                    })
                    .ToListAsync();

                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo categorías");
                return StatusCode(500, new
                {
                    Message = "Error al obtener categorías",
                    Error = ex.Message
                });
            }
        }
    }
}