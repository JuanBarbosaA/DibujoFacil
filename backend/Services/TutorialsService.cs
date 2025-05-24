using backend.Dtos;
using backend.Repositories.Models;
using Microsoft.EntityFrameworkCore;

public class TutorialService
{
    private readonly DbDibujofacilContext _context;

    public TutorialService(DbDibujofacilContext context)
    {
        _context = context;
    }

    public List<string> GetAllowedDifficulties(int points)
    {
        if (points < 1000) return new List<string> { "beginner" };
        if (points < 3000) return new List<string> { "beginner", "intermediate" };
        return new List<string> { "beginner", "intermediate", "advanced" };
    }

    public async Task<User> GetUserByIdAsync(int userId)
    {
        return await _context.Users.FindAsync(userId);
    }

    public async Task<List<TutorialFullDto>> GetTutorialsAsync(string search, int? categoryId, string difficulty, User user)
    {
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

        return tutorialDtos;
    }

    public async Task<List<CategoryDto>> GetCategoriesAsync()
    {
        return await _context.Categories
            .AsNoTracking()
            .Select(c => new CategoryDto { Id = c.Id, Name = c.Name })
            .ToListAsync();
    }

    public async Task<TutorialFullDto> GetTutorialByIdAsync(int id)
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
            return null;

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

        return tutorialDto;
    }

    public async Task<bool> TutorialExistsAsync(int id)
    {
        return await _context.Tutorial.AnyAsync(t => t.Id == id);
    }
}
