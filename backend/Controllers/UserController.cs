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



        [HttpPut("update-points")]
        public async Task<IActionResult> UpdateUserPoints([FromBody] UserUpdatePointsDto pointsDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var user = await _context.Users
                    .Include(u => u.UserAchievements)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null) return NotFound("Usuario no encontrado");

                bool crossedThreshold = user.Points < 1000 && pointsDto.Points >= 1000;
                user.Points = pointsDto.Points;

                if (crossedThreshold)
                {
                    await CheckAndAssignAchievement(user, 1000);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    user.Points,
                    Message = crossedThreshold ? "¡Logro desbloqueado!" : "Puntos actualizados"
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new
                {
                    Message = "Error al actualizar puntos",
                    Error = ex.Message
                });
            }
        }

        private async Task CheckAndAssignAchievement(User user, int requiredPoints)
        {
            var achievement = await _context.Achievements
                .FirstOrDefaultAsync(a => a.RequiredPoints == requiredPoints);

            if (achievement == null) return;

            var hasAchievement = user.UserAchievements
                .Any(ua => ua.AchievementId == achievement.Id);

            if (!hasAchievement)
            {
                user.UserAchievements.Add(new UserAchievement
                {
                    AchievementId = achievement.Id,
                    ObtainedDate = DateTime.UtcNow
                });

                _context.Notifications.Add(new Notification
                {
                    UserId = user.Id,
                    Message = $"¡Logro desbloqueado! {achievement.Name}",
                    Date = DateTime.UtcNow,
                    Read = false
                });
            }
        }

        [HttpGet("achievements")]
        public async Task<IActionResult> GetUserAchievements()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var achievements = await _context.UserAchievements
                .Where(ua => ua.UserId == userId)
                .Include(ua => ua.Achievement)
                .Select(ua => new AchievementDto
                {
                    Id = ua.Achievement.Id,
                    Name = ua.Achievement.Name,
                    Description = ua.Achievement.Description,
                    ObtainedDate = ua.ObtainedDate
                })
                .ToListAsync();

            return Ok(achievements);
        }
    }

}