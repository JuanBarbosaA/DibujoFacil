using backend.Repositories.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly DbDibujofacilContext _context;

        public UserController(DbDibujofacilContext context)
        {
            _context = context;
        }

        [HttpGet("profile")]
        [Authorize]
        public IActionResult Profile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var user = _context.Users.FirstOrDefault(u => u.Email == userId);

            if (user == null)
                return NotFound(new { message = "User not found" });
            return Ok(new
            {
                user.Id,
                user.Email,
                user.Name
            });
        }

    }
}
