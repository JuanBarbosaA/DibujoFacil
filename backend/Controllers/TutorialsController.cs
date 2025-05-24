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
    /// <summary>
    /// Controlador para gestionar operaciones relacionadas con tutoriales.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TutorialsController : ControllerBase
    {
        private readonly DbDibujofacilContext _context;
        private readonly ILogger<TutorialsController> _logger;

        /// <summary>
        /// Inicializa una nueva instancia del controlador <see cref="TutorialsController"/>.
        /// </summary>
        /// <param name="context">Contexto de la base de datos.</param>
        /// <param name="logger">Logger para registrar información y errores.</param>
        public TutorialsController(
            DbDibujofacilContext context,
            ILogger<TutorialsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene una lista filtrada de tutoriales disponibles para el usuario autenticado.
        /// </summary>
        /// <param name="search">Texto para búsqueda en título y descripción.</param>
        /// <param name="categoryId">Identificador opcional de categoría para filtrar tutoriales.</param>
        /// <param name="difficulty">Nivel de dificultad para filtrar tutoriales.</param>
        /// <returns>Lista de tutoriales que cumplen con los criterios de búsqueda y autorización.</returns>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetTutorials(
            [FromQuery] string search = "",
            [FromQuery] int? categoryId = null,
            [FromQuery] string difficulty = "")
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var user = await _context.Users.FindAsync(userId);

                if (user == null) return NotFound("Usuario no encontrado");

                List<string> allowedDifficulties = user.RoleId == 1
                    ? new List<string> { "beginner", "intermediate", "advanced" }
                    : GetAllowedDifficulties(user.Points ?? 0);

                if (!string.IsNullOrEmpty(difficulty))
                {
                    difficulty = difficulty.ToLower();
                    allowedDifficulties = allowedDifficulties.Contains(difficulty)
                        ? new List<string> { difficulty }
                        : new List<string>();
                }

                var query = _context.Tutorial
                    .Include(t => t.Author)
                    .Include(t => t.TutorialCategories)
                        .ThenInclude(tc => tc.Category)
                    .Include(t => t.TutorialContents)
                    .Include(t => t.Comments)
                        .ThenInclude(c => c.User)
                    .Include(t => t.Ratings)
                        .ThenInclude(r => r.User)
                    .Where(t => allowedDifficulties.Contains(t.Difficulty.ToLower())
                        && t.Status == "approved")
                    .AsNoTracking();

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(t =>
                        t.Title.Contains(search) ||
                        t.Description.Contains(search));
                }

                if (categoryId.HasValue)
                {
                    query = query.Where(t =>
                        t.TutorialCategories.Any(tc => tc.CategoryId == categoryId));
                }

                var tutorials = await query.ToListAsync();

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
                    Categories = t.TutorialCategories.Select(tc => new CategoryDto
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
                    AverageRating = t.Ratings.Any() ? (double)t.Ratings.Average(r => r.Score) : 0
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

        /// <summary>
        /// Determina los niveles de dificultad permitidos según los puntos del usuario.
        /// </summary>
        /// <param name="points">Puntos acumulados del usuario.</param>
        /// <returns>Lista de niveles de dificultad permitidos.</returns>
        private List<string> GetAllowedDifficulties(int points)
        {
            if (points < 1000) return new List<string> { "beginner" };
            if (points < 3000) return new List<string> { "beginner", "intermediate" };
            return new List<string> { "beginner", "intermediate", "advanced" };
        }

        /// <summary>
        /// Obtiene todas las categorías disponibles para los tutoriales.
        /// </summary>
        /// <returns>Lista de categorías.</returns>
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _context.Categories
                    .AsNoTracking()
                    .Select(c => new CategoryDto { Id = c.Id, Name = c.Name })
                    .ToListAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo categorías");
                return StatusCode(500, new { Message = "Error al obtener categorías", Error = ex.Message });
            }
        }

        /// <summary>
        /// Crea un nuevo tutorial con sus contenidos y categorías asociadas.
        /// </summary>
        /// <param name="tutorialDto">Datos del tutorial a crear, recibidos en formulario.</param>
        /// <returns>Respuesta con la confirmación de creación y el ID del tutorial creado.</returns>
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

        /// <summary>
        /// Obtiene un tutorial completo por su identificador.
        /// </summary>
        /// <param name="id">Identificador del tutorial.</param>
        /// <returns>Tutorial completo con detalles y relaciones.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTutorial(int id)
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
                    return NotFound("Tutorial no encontrado");

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
                    Categories = tutorial.TutorialCategories.Select(tc => new CategoryDto
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
                    AverageRating = tutorial.Ratings.Any() ? (double)tutorial.Ratings.Average(r => r.Score) : 0
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
    }
}
