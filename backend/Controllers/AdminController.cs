using backend.Dtos;
using backend.Repositories.Models;
using backend.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace backend.Controllers
{
    [Route("api/admin")]
    [ApiController]
    [Authorize]
    public class AdminController : ControllerBase
    {
        private readonly DbDibujofacilContext _context;

        public AdminController(DbDibujofacilContext context)
        {
            _context = context;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var currentUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == currentUserId);

            // Verificar si el usuario es admin (RoleId = 1)
            if (currentUser?.RoleId != 1)
                return Forbid("No tienes permisos de administrador");

            var users = await _context.Users
                .Include(u => u.Role)
                .AsNoTracking()
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
                    LastLogin = u.RegistrationDate // Cambiar por campo real de último login si existe
                })
                .ToListAsync();

            return Ok(users);
        }



        [HttpPut("users/{id}")]
        public async Task<IActionResult> EditUser(
            int id,
            [FromBody] UserUpdateDto userDto)
        {
            try
            {
                // Verificar permisos de admin
                var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var adminUser = await _context.Users.FindAsync(adminId);
                if (adminUser?.RoleId != 1) return Forbid("No eres administrador");

                // Validar modelo
                if (!ModelState.IsValid) return BadRequest(ModelState);

                // Buscar usuario a editar
                var user = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null) return NotFound("Usuario no encontrado");

                // Validar email único (excepto para el mismo usuario)
                if (await _context.Users.AnyAsync(u => u.Email == userDto.Email && u.Id != id))
                    return Conflict("El email ya está registrado");

                // Actualizar campos permitidos
                user.Name = userDto.Name;
                user.Email = userDto.Email;
                user.Points = userDto.Points;
                user.Status = userDto.Status;

                // Cambiar rol si es necesario
                if (userDto.RoleId != user.RoleId)
                {
                    var newRole = await _context.Roles.FindAsync(userDto.RoleId);
                    if (newRole == null) return BadRequest("Rol no válido");
                    user.RoleId = newRole.Id;
                }

                // Guardar cambios
                await _context.SaveChangesAsync();

                // Devolver usuario actualizado
                return Ok(new
                {
                    user.Id,
                    user.Name,
                    user.Email,
                    user.Points,
                    user.Status,
                    Role = user.Role.Name
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "Error al actualizar usuario",
                    Error = ex.Message
                });
            }
        }

        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _context.Roles
                .Select(r => new { r.Id, r.Name })
                .ToListAsync();
            return Ok(roles);
        }


        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Verificar permisos de admin
                var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var adminUser = await _context.Users.FindAsync(adminId);
                if (adminUser?.RoleId != 1) return Forbid("No eres administrador");

                // Validar no auto-eliminación
                if (id == adminId)
                    return BadRequest("No puedes eliminarte a ti mismo");

                // Buscar usuario a eliminar con todas sus relaciones
                var user = await _context.Users
                    .Include(u => u.Tutorials)
                        .ThenInclude(t => t.Comments)
                    .Include(u => u.Tutorials)
                        .ThenInclude(t => t.Ratings)
                    .Include(u => u.Comments)
                    .Include(u => u.Ratings)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null) return NotFound("Usuario no encontrado");

                // Eliminar relaciones primero
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

                return Ok(new { Message = "Usuario eliminado exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "Error al eliminar usuario",
                    Error = ex.Message
                });
            }
        }


        [HttpPost("users")]
        public async Task<IActionResult> CreateUser([FromBody] AdminUserCreationDto userDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Verificar permisos de admin
                var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var adminUser = await _context.Users.FindAsync(adminId);
                if (adminUser?.RoleId != 1) return Forbid("No eres administrador");

                // Validar modelo
                if (!ModelState.IsValid) return BadRequest(ModelState);

                // Verificar email único
                if (await _context.Users.AnyAsync(u => u.Email == userDto.Email))
                    return Conflict("El email ya está registrado");

                // Verificar rol válido
                var role = await _context.Roles.FindAsync(userDto.RoleId);
                if (role == null) return BadRequest("Rol no válido");

                // Crear usuario
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

                return CreatedAtAction(nameof(GetAllUsers), new
                {
                    Message = "Usuario creado exitosamente",
                    UserId = newUser.Id
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "Error al crear usuario",
                    Error = ex.Message
                });
            }
        }


        [HttpGet("tutorials")]
        public async Task<IActionResult> GetAllTutorials()
        {
            try
            {
                // Verificar permisos de admin
                var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var adminUser = await _context.Users.FindAsync(adminId);
                if (adminUser?.RoleId != 1) return Forbid("No eres administrador");

                var tutorials = await _context.Tutorial
                    .Include(t => t.Author)
                    .Include(t => t.TutorialCategories)
                        .ThenInclude(tc => tc.Category)
                    .Include(t => t.TutorialContents)
                    .Include(t => t.Comments)
                    .Include(t => t.Ratings)
                    .AsNoTracking()
                    .OrderByDescending(t => t.PublicationDate)
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
                        Categories = t.TutorialCategories
                            .Select(tc => tc.Category.Name)
                            .ToList(),
                        ContentCount = t.TutorialContents.Count,
                        CommentsCount = t.Comments.Count,
                        AverageRating = t.Ratings.Any() ?
                            t.Ratings.Average(r => r.Score.Value) : 0,
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

                return Ok(tutorials);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "Error al traer tutoriales",
                    Error = ex.Message
                });
            }
        }


[HttpPut("tutorials/{id}/contents")]
public async Task<IActionResult> UpdateTutorialContents(
    int id,
    [FromForm] TutorialContentUpdateDto contentDto)
{
    using var transaction = await _context.Database.BeginTransactionAsync();
    try
    {
        var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var adminUser = await _context.Users.FindAsync(adminId);
        if (adminUser?.RoleId != 1) return Forbid("No eres administrador");

        var tutorial = await _context.Tutorial
            .Include(t => t.TutorialContents)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (tutorial == null) return NotFound("Tutorial no encontrado");

        // Eliminar contenidos marcados
        if (contentDto.ContentIdsToDelete != null && contentDto.ContentIdsToDelete.Any())
        {
            var contentsToDelete = tutorial.TutorialContents
                .Where(c => contentDto.ContentIdsToDelete.Contains(c.Id))
                .ToList();

            _context.TutorialContents.RemoveRange(contentsToDelete);
        }

        // Agregar nuevos contenidos
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
                    Order = ++maxOrder
                });
            }
        }

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return Ok(new { Message = "Contenidos actualizados exitosamente" });
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        return StatusCode(500, new
        {
            Message = "Error al actualizar contenidos",
            Error = ex.Message
        });
    }
}


        [HttpDelete("tutorials/{id}")]
        public async Task<IActionResult> DeleteTutorial(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Verificar permisos de admin
                var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var adminUser = await _context.Users.FindAsync(adminId);
                if (adminUser?.RoleId != 1) return Forbid("No eres administrador");

                // Buscar tutorial con todas sus relaciones
                var tutorial = await _context.Tutorial
                    .Include(t => t.TutorialContents)
                    .Include(t => t.Comments)
                    .Include(t => t.Ratings)
                    .Include(t => t.TutorialCategories)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (tutorial == null) return NotFound("Tutorial no encontrado");

                // Eliminar relaciones
                _context.Comments.RemoveRange(tutorial.Comments);
                _context.Ratings.RemoveRange(tutorial.Ratings);
                _context.TutorialContents.RemoveRange(tutorial.TutorialContents);
                _context.TutorialCategories.RemoveRange(tutorial.TutorialCategories);

                // Eliminar tutorial
                _context.Tutorial.Remove(tutorial);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { Message = "Tutorial eliminado exitosamente" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new
                {
                    Message = "Error al eliminar tutorial",
                    Error = ex.Message
                });
            }
        }


        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.Categories
                .Select(c => new { c.Id, c.Name })
                .ToListAsync();
            return Ok(categories);
        }

    }
}