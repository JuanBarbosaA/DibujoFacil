using backend.Dtos;
using backend.Repositories;
using backend.Repositories.Models;

namespace backend.Services
{
    public class UserService
    {
        private readonly UserRepository _userRepository;
        private readonly AchievementRepository _achievementRepository;

        public UserService(
            UserRepository userRepository,
            AchievementRepository achievementRepository)
        {
            _userRepository = userRepository;
            _achievementRepository = achievementRepository;
        }

        public async Task<object> GetProfile(int userId)
        {
            var user = await _userRepository.GetWithDetails(userId);

            return new
            {
                user.Id,
                user.Name,
                user.Email,
                user.AvatarUrl,
                user.Points,
                Tutorials = user.Tutorials.Select(t => new {
                    t.Id,
                    t.Title,
                    t.Description,
                    t.PublicationDate,
                    t.Status,
                    LastImage = t.TutorialContents
                        .Where(c => c.Type.StartsWith("image/"))
                        .OrderByDescending(c => c.Order)
                        .Select(c => new TutorialContentImageDto
                        {
                            Type = c.Type,
                            ContentBase64 = Convert.ToBase64String(c.Content)
                        })
                        .FirstOrDefault(),
                    CommentCount = t.Comments.Count,
                    RatingCount = t.Ratings.Count,
                    AverageRating = t.Ratings.Any() ? t.Ratings.Average(r => r.Score) : 0
                })
            };
        }

        public async Task<UserAdminDto> UpdatePoints(int userId, int points)
        {
            var user = await _userRepository.GetWithAchievements(userId);
            var crossedThreshold = user.Points < 1000 && points >= 1000;

            user.Points = points;

            if (crossedThreshold)
            {
                var achievement = await _achievementRepository.GetByPoints(1000);
                if (achievement != null && !user.UserAchievements.Any(ua => ua.AchievementId == achievement.Id))
                {
                    await _userRepository.AddAchievement(userId, achievement.Id);
                }
            }

            await _userRepository.Update(user);

            return new UserAdminDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Points = user.Points,
                Status = user.Status
            };
        }

        public async Task<List<AchievementDto>> GetAchievements(int userId)
        {
            return await _userRepository.GetAchievements(userId);
        }
    }
}