using backend.Dtos;
using backend.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories
{
    public class UserRepository
    {
        private readonly DbDibujofacilContext _context;

        public UserRepository(DbDibujofacilContext context)
        {
            _context = context;
        }

        public async Task<User> GetWithDetails(int userId)
        {
            return await _context.Users
                .Include(u => u.Tutorials)
                    .ThenInclude(t => t.TutorialContents)
                .Include(u => u.Tutorials)
                    .ThenInclude(t => t.Comments)
                .Include(u => u.Tutorials)
                    .ThenInclude(t => t.Ratings)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<User> GetWithAchievements(int userId)
        {
            return await _context.Users
                .Include(u => u.UserAchievements)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task Update(User user)
        {
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task AddAchievement(int userId, int achievementId)
        {
            _context.UserAchievements.Add(new UserAchievement
            {
                UserId = userId,
                AchievementId = achievementId,
                ObtainedDate = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
        }

        public async Task<List<AchievementDto>> GetAchievements(int userId)
        {
            return await _context.UserAchievements
                .Where(ua => ua.UserId == userId)
                .Include(ua => ua.Achievement)
                .Select(ua => new AchievementDto
                {
                    Id = ua.Achievement.Id,
                    Name = ua.Achievement.Name,
                    Description = ua.Achievement.Description,
                    ObtainedDate = (DateTime)ua.ObtainedDate
                })
                .ToListAsync();
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User> GetByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task AddAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }
    }
}