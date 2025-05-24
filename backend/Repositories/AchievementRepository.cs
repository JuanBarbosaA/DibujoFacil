using backend.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories
{
    public class AchievementRepository
    {
        private readonly DbDibujofacilContext _context;

        public AchievementRepository(DbDibujofacilContext context)
        {
            _context = context;
        }

        public async Task<Achievement> GetByPoints(int points)
        {
            return await _context.Achievements
                .FirstOrDefaultAsync(a => a.RequiredPoints == points);
        }
    }
}