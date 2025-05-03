using backend.Dtos;
using backend.Repositories.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;
using System.Security.Claims;
using static System.Net.Mime.MediaTypeNames;
using System.Xml.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;

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
                        Score = r.Score.Value,
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTutorialById(int id)
        {
            try
            {
                var tutorial = await _context.Tutorial
                    .Include(t => t.Author)
                    .Include(t => t.TutorialCategories)
                        .ThenInclude(tc => tc.Category)
                    .Include(t => t.TutorialContents)
                    .Include(t => t.Comments)
                        .ThenInclude(c => c.User)
                    .Include(t => t.Ratings)
                        .ThenInclude(r => r.User)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (tutorial == null)
                {
                    return NotFound("Tutorial no encontrado");
                }

                var tutorialDto = new TutorialFullDto
                {
                    Id = tutorial.Id,
                    Title = tutorial.Title,
                    Description = tutorial.Description,
                    Difficulty = tutorial.Difficulty,
                    EstimatedDuration = tutorial.EstimatedDuration,
                    PublicationDate = tutorial.PublicationDate,
                    Status = tutorial.Status,
                    Author = new UserDto
                    {
                        Id = tutorial.Author.Id,
                        Name = tutorial.Author.Name,
                        Email = tutorial.Author.Email,
                        AvatarUrl = tutorial.Author.AvatarUrl
                    },
                    Categories = tutorial.TutorialCategories
                        .Select(tc => new CategoryDto
                        {
                            Id = tc.Category.Id,
                            Name = tc.Category.Name
                        }).ToList(),
                    Contents = tutorial.TutorialContents.Select(tc => new TutorialContentDto
                    {
                        Id = tc.Id,
                        Type = tc.Type,
                        ContentBase64 = Convert.ToBase64String(tc.Content),
                        Order = tc.Order,
                        Description = tc.Description,
                        Title = tc.Title
                    }).OrderBy(tc => tc.Order).ToList(),
                    Comments = tutorial.Comments.Select(c => new CommentDto
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
                    Ratings = tutorial.Ratings.Select(r => new RatingDto
                    {
                        Id = r.Id,
                        Score = r.Score.Value,
                        Date = r.Date,
                        User = new UserDto
                        {
                            Id = r.User.Id,
                            Name = r.User.Name
                        }
                    }).ToList(),
                    AverageRating = tutorial.Ratings.Any()
                        ? (double)tutorial.Ratings.Average(r => r.Score)
                        : 0
                };

                return Ok(tutorialDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo tutorial");
                return StatusCode(500, new
                {
                    Message = "Error al obtener el tutorial",
                    Error = ex.Message
                });
            }
        }



        [Authorize]
        [HttpPost("{id}/rate")]
        public async Task<IActionResult> RateTutorial(int id, [FromBody] RatingCreationDto ratingDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                var existingRating = await _context.Ratings
                    .FirstOrDefaultAsync(r => r.TutorialId == id && r.UserId == userId);

                if (existingRating != null)
                {
                    return Conflict("Ya has calificado este tutorial");
                }

                var rating = new Rating
                {
                    TutorialId = id,
                    UserId = userId,
                    Score = ratingDto.Score,
                    Date = DateTime.UtcNow
                };

                _context.Ratings.Add(rating);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    Message = "Calificación registrada exitosamente",
                    RatingId = rating.Id
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error registrando calificación");
                return StatusCode(500, new
                {
                    Message = "Error al calificar el tutorial",
                    Error = ex.Message
                });
            }
        }


        [Authorize]
        [HttpPost("{id}/comment")]
        public async Task<IActionResult> AddComment(int id, [FromBody] CommentCreationDto commentDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                var tutorialExists = await _context.Tutorial.AnyAsync(t => t.Id == id);
                if (!tutorialExists)
                {
                    return NotFound("Tutorial no encontrado");
                }

                var comment = new Comment
                {
                    TutorialId = id,
                    UserId = userId,
                    Comment1 = commentDto.Text,
                    Date = DateTime.UtcNow,
                    Edited = false
                };

                _context.Comments.Add(comment);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var user = await _context.Users
                    .AsNoTracking()
                    .Select(u => new UserDto
                    {
                        Id = u.Id,
                        Name = u.Name,
                        AvatarUrl = u.AvatarUrl
                    })
                    .FirstOrDefaultAsync(u => u.Id == userId);

                return CreatedAtAction(nameof(GetTutorialById), new { id }, new CommentDto
                {
                    Id = comment.Id,
                    Text = comment.Comment1,
                    Date = comment.Date,
                    Edited = comment.Edited,
                    User = user
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error agregando comentario");
                return StatusCode(500, new
                {
                    Message = "Error al agregar el comentario",
                    Error = ex.Message
                });
            }
        }

        [HttpGet("{id}/comments")]
        public async Task<IActionResult> GetPaginatedComments(
            int id,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 4)
             {
                try
                    {
                        var query = _context.Comments
                            .Where(c => c.TutorialId == id)
                            .Include(c => c.User)
                            .OrderByDescending(c => c.Date);

                        var totalComments = await query.CountAsync();
                        var comments = await query
                                .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .Select(c => new CommentDto
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
                        })
                        .ToListAsync();

                    return Ok(new
                    {
                        Comments = comments,
                        TotalComments = totalComments,
                        CurrentPage = page,
                        TotalPages = (int)Math.Ceiling(totalComments / (double)pageSize),
                        HasMore = page * pageSize < totalComments
                    });
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "Error obteniendo comentarios paginados");
                 return StatusCode(500, "Error interno del servidor");
             }
        }


        [HttpGet("download-pdf/{tutorialId}")]
        public async Task<IActionResult> DownloadTutorialPdf(int tutorialId)
        {
            try
            {
                var tutorial = await _context.Tutorial
                    .Include(t => t.TutorialContents)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == tutorialId);

                if (tutorial == null) return NotFound("Tutorial no encontrado");

                using var memoryStream = new MemoryStream();
                var document = new iTextSharp.text.Document(PageSize.A4, 25, 25, 30, 30);
                var writer = PdfWriter.GetInstance(document, memoryStream);

                document.Open();

                document.AddTitle(tutorial.Title);
                document.AddCreator("DibujoFácil");

                var orderedContents = tutorial.TutorialContents
                    .OrderBy(c => c.Order)
                    .ToList();

                foreach (var content in orderedContents)
                {
                    if (content.Type.StartsWith("image/"))
                    {
                        using var imageStream = new MemoryStream(content.Content);
                        var image = iTextSharp.text.Image.GetInstance(imageStream);
                        image.Alignment = Element.ALIGN_CENTER;
                        image.ScaleToFit(document.PageSize.Width - 50, document.PageSize.Height - 50);
                        document.Add(image);
                        document.NewPage();
                    }
                }

                document.Close();

                return File(memoryStream.ToArray(), "application/pdf",
                    $"{tutorial.Title.Replace(" ", "_")}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando PDF para tutorial {TutorialId}", tutorialId);
                return StatusCode(500, "Error generando el PDF");
            }
        }

    }
}