using backend.Repositories.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly DbDibujofacilContext _context;

        public UserController(DbDibujofacilContext context)
        {
            _context = context;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var user = await _context.Users
                .Include(u => u.Tutorials)
                    .ThenInclude(t => t.TutorialContents)
                .Include(u => u.Tutorials)
                    .ThenInclude(t => t.Comments)
                .Include(u => u.Tutorials)
                    .ThenInclude(t => t.Ratings)
                .AsNoTracking()
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Email,
                    u.AvatarUrl,
                    u.Points,
                    Tutorials = u.Tutorials
                        .Where(t => t.AuthorId == userId)
                        .OrderByDescending(t => t.PublicationDate)
                        .Select(t => new
                        {
                            t.Id,
                            t.Title,
                            t.Description,
                            t.PublicationDate,
                            t.Status,
                            LastImage = t.TutorialContents
                                .Where(c => c.Type.StartsWith("image/"))
                                .OrderByDescending(c => c.Order)
                                .Select(c => new
                                {
                                    c.Type,
                                    ContentBase64 = Convert.ToBase64String(c.Content)
                                })
                                .FirstOrDefault(),
                            CommentCount = t.Comments.Count,
                            RatingCount = t.Ratings.Count,
                            AverageRating = t.Ratings.Any() ?
                                (double?)t.Ratings.Average(r => r.Score) : null
                        })
                })
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return NotFound("Usuario no encontrado");
            return Ok(user);
        }
    }
}