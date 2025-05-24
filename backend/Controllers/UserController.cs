using backend.Dtos;
using backend.Repositories;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var result = await _userService.GetProfile(userId);
            return Ok(result);
        }

        [HttpPut("update-points")]
        public async Task<IActionResult> UpdateUserPoints([FromBody] UserUpdatePointsDto pointsDto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var result = await _userService.UpdatePoints(userId, pointsDto.Points);
            return Ok(result);
        }

        [HttpGet("achievements")]
        public async Task<IActionResult> GetUserAchievements()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var achievements = await _userService.GetAchievements(userId);
            return Ok(achievements);
        }
    }
}