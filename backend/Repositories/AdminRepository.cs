using backend.Dtos;
using backend.Repositories.Models;
using backend.Utilities;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories
{
    public class AdminRepository
    {
        private readonly DbDibujofacilContext _context;

        public AdminRepository(DbDibujofacilContext context)
        {
            _context = context;
        }

        public async Task ValidateAdmin(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user?.RoleId != 1) throw new Exception("No tienes permisos de administrador");
        }

        public async Task<List<object>> GetAllUsersWithRoles()
        {
            return await _context.Users
                .Include(u => u.Role)
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Email,
                    u.RegistrationDate,
                    u.Status,
                    u.Points,
                    RoleId = u.RoleId,
                    Role = u.Role.Name,
                    TutorialsCount = u.Tutorials.Count,
                    LastLogin = u.RegistrationDate
                })
                .Cast<object>()
                .ToListAsync();
        }

        public async Task<object> UpdateUserWithRole(int userId, UserUpdateDto userDto)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            user.Name = userDto.Name;
            user.Email = userDto.Email;
            user.Points = userDto.Points;
            user.Status = userDto.Status;

            if (userDto.RoleId != user.RoleId)
            {
                var newRole = await _context.Roles.FindAsync(userDto.RoleId);
                user.RoleId = newRole.Id;
            }

            await _context.SaveChangesAsync();

            return new
            {
                user.Id,
                user.Name,
                user.Email,
                user.Points,
                user.Status,
                Role = user.Role.Name
            };
        }

        public async Task<object> DeleteUserWithDependencies(int userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            var user = await _context.Users
                .Include(u => u.Tutorials)
                .ThenInclude(t => t.Comments)
                .Include(u => u.Tutorials)
                .ThenInclude(t => t.Ratings)
                .Include(u => u.Comments)
                .Include(u => u.Ratings)
                .FirstOrDefaultAsync(u => u.Id == userId);

            _context.Comments.RemoveRange(user.Comments);
            _context.Ratings.RemoveRange(user.Ratings);

            foreach (var tutorial in user.Tutorials)
            {
                _context.Comments.RemoveRange(tutorial.Comments);
                _context.Ratings.RemoveRange(tutorial.Ratings);
            }

            _context.Tutorial.RemoveRange(user.Tutorials);
            _context.Users.Remove(user);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new { Message = "Usuario eliminado exitosamente" };
        }

        public async Task<object> CreateUserWithRole(AdminUserCreationDto userDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            var newUser = new User
            {
                Name = userDto.Name,
                Email = userDto.Email,
                PasswordHash = EncryptUtility.HashPassword(userDto.Password),
                RoleId = userDto.RoleId,
                Points = userDto.Points,
                Status = userDto.Status ?? "active",
                RegistrationDate = DateTime.UtcNow,
                AvatarUrl = userDto.AvatarUrl
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new
            {
                Message = "Usuario creado exitosamente",
                UserId = newUser.Id
            };
        }

        public async Task<List<AdminTutorialDto>> GetAllTutorialsWithDetails()
        {
            return await _context.Tutorial
                .Include(t => t.Author)
                .Include(t => t.TutorialCategories)
                .ThenInclude(tc => tc.Category)
                .Include(t => t.TutorialContents)
                .Include(t => t.Comments)
                .Include(t => t.Ratings)
                .Select(t => new AdminTutorialDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Status = t.Status,
                    PublicationDate = (DateTime)t.PublicationDate,
                    Difficulty = t.Difficulty,
                    Author = new AdminTutorialAuthorDto
                    {
                        Id = t.Author.Id,
                        Name = t.Author.Name,
                        Email = t.Author.Email
                    },
                    Categories = t.TutorialCategories.Select(tc => tc.Category.Name).ToList(),
                    ContentCount = t.TutorialContents.Count,
                    CommentsCount = t.Comments.Count,
                    AverageRating = t.Ratings.Any() ? t.Ratings.Average(r => r.Score.Value) : 0,
                    LastImage = t.TutorialContents
                        .Where(c => c.Type.StartsWith("image/"))
                        .OrderByDescending(c => c.Order)
                        .Select(c => new TutorialContentImageDto
                        {
                            Type = c.Type,
                            ContentBase64 = Convert.ToBase64String(c.Content)
                        })
                        .FirstOrDefault()
                })
                .ToListAsync();
        }

        public async Task<object> UpdateTutorialContents(int tutorialId, TutorialContentUpdateDto contentDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            var tutorial = await _context.Tutorial
                .Include(t => t.TutorialContents)
                .FirstOrDefaultAsync(t => t.Id == tutorialId);

            if (contentDto.ContentIdsToDelete != null && contentDto.ContentIdsToDelete.Any())
            {
                var contentsToDelete = tutorial.TutorialContents
                    .Where(c => contentDto.ContentIdsToDelete.Contains(c.Id))
                    .ToList();

                _context.TutorialContents.RemoveRange(contentsToDelete);
            }

            if (contentDto.NewImages != null && contentDto.NewImages.Count > 0)
            {
                var maxOrder = tutorial.TutorialContents.Any() ?
                    tutorial.TutorialContents.Max(c => c.Order) : 0;

                foreach (var image in contentDto.NewImages)
                {
                    using var ms = new MemoryStream();
                    await image.CopyToAsync(ms);

                    tutorial.TutorialContents.Add(new TutorialContent
                    {
                        Type = image.ContentType,
                        Content = ms.ToArray(),
                        Order = ++maxOrder,
                        Title = image.Name

                    });
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new { Message = "Contenidos actualizados exitosamente" };
        }

        public async Task<object> DeleteTutorialWithContents(int tutorialId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            var tutorial = await _context.Tutorial
                .Include(t => t.TutorialContents)
                .Include(t => t.Comments)
                .Include(t => t.Ratings)
                .Include(t => t.TutorialCategories)
                .FirstOrDefaultAsync(t => t.Id == tutorialId);

            _context.Comments.RemoveRange(tutorial.Comments);
            _context.Ratings.RemoveRange(tutorial.Ratings);
            _context.TutorialContents.RemoveRange(tutorial.TutorialContents);
            _context.TutorialCategories.RemoveRange(tutorial.TutorialCategories);
            _context.Tutorial.Remove(tutorial);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new { Message = "Tutorial eliminado exitosamente" };
        }

        public async Task<List<object>> GetAllRoles()
        {
            return await _context.Roles
                .Select(r => new { r.Id, r.Name })
                .Cast<object>()
                .ToListAsync();
        }

        public async Task<List<object>> GetAllCategories()
        {
            return await _context.Categories
                .Select(c => new
                {
                    c.Id,
                    c.Name
                })
                .ToListAsync<object>();
        }

        public async Task<List<AdminTutorialDto>> GetPendingTutorialsWithDetails()
        {
            return await _context.Tutorial
                .Include(t => t.Author)
                .Include(t => t.TutorialCategories)
                .ThenInclude(tc => tc.Category)
                .Where(t => t.Status == "pending")
                .Select(t => new AdminTutorialDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Status = t.Status,
                    PublicationDate = (DateTime)t.PublicationDate,
                    Difficulty = t.Difficulty,
                    Author = new AdminTutorialAuthorDto
                    {
                        Id = t.Author.Id,
                        Name = t.Author.Name,
                        Email = t.Author.Email
                    },
                    Categories = t.TutorialCategories.Select(tc => tc.Category.Name).ToList(),
                    ContentCount = t.TutorialContents.Count,
                    CommentsCount = t.Comments.Count,
                    AverageRating = t.Ratings.Any() ? t.Ratings.Average(r => r.Score.Value) : 0,
                    LastImage = t.TutorialContents
                        .Where(c => c.Type.StartsWith("image/"))
                        .OrderByDescending(c => c.Order)
                        .Select(c => new TutorialContentImageDto
                        {
                            Type = c.Type,
                            ContentBase64 = Convert.ToBase64String(c.Content)
                        })
                        .FirstOrDefault()
                })
                .ToListAsync();
        }
        // En AdminRepository.cs, MODIFICAR el método ApproveTutorial:

        public async Task<object> ApproveTutorial(int tutorialId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var tutorial = await _context.Tutorial
                    .Include(t => t.Author)
                    .FirstOrDefaultAsync(t => t.Id == tutorialId); // <- Quitar ThenInclude de UserAchievements

                if (tutorial == null)
                    throw new Exception("Tutorial no encontrado");

                if (tutorial.Status == "pending")
                {
                    // Paso 1: Actualizar estado usando SQL directo (esto activará el trigger)
                    await _context.Database.ExecuteSqlInterpolatedAsync(
                        $"UPDATE Tutorial SET Status = 'approved' WHERE Id = {tutorialId}");

                    // Paso 2: Recargar el tutorial para obtener cambios
                    await _context.Entry(tutorial).ReloadAsync();

                    // Paso 3: Confirmar transacción
                    await transaction.CommitAsync();

                    return new { Message = "Tutorial aprobado exitosamente" }; // <- Eliminar puntos y logros
                }

                return new { Message = "Tutorial ya estaba aprobado" };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<UserStatisticsDto>> GetUserStatistics()
        {
            return await _context.UserStatistics
                .FromSqlRaw("EXEC GetUserStatisticsV2")
                .AsNoTracking()
                .ToListAsync();
        }
    }
}