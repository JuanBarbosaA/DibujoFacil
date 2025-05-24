using backend.Dtos;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend.Controllers
{
    [Route("api/admin")]
    [ApiController]
    [Authorize]
    public class AdminController : ControllerBase
    {
        private readonly AdminService _adminService;

        public AdminController(AdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return Ok(await _adminService.GetAllUsers(adminId));
        }

        [HttpPut("users/{id}")]
        public async Task<IActionResult> EditUser(int id, [FromBody] UserUpdateDto userDto)
        {
            var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return Ok(await _adminService.UpdateUser(adminId, id, userDto));
        }

        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles()
        {
            return Ok(await _adminService.GetAllRoles());
        }

        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return Ok(await _adminService.DeleteUser(adminId, id));
        }

        [HttpPost("users")]
        public async Task<IActionResult> CreateUser([FromBody] AdminUserCreationDto userDto)
        {
            var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return Ok(await _adminService.CreateUser(adminId, userDto));
        }

        [HttpGet("tutorials")]
        public async Task<IActionResult> GetAllTutorials()
        {
            var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return Ok(await _adminService.GetAllTutorials(adminId));
        }

        [HttpPut("tutorials/{id}/contents")]
        public async Task<IActionResult> UpdateTutorialContents(int id, [FromForm] TutorialContentUpdateDto contentDto)
        {
            var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return Ok(await _adminService.UpdateTutorialContents(adminId, id, contentDto));
        }

        [HttpDelete("tutorials/{id}")]
        public async Task<IActionResult> DeleteTutorial(int id)
        {
            var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return Ok(await _adminService.DeleteTutorial(adminId, id));
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            return Ok(await _adminService.GetAllCategories());
        }

        [HttpGet("tutorials/pending")]
        public async Task<IActionResult> GetPendingTutorials()
        {
            var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return Ok(await _adminService.GetPendingTutorials(adminId));
        }

        [HttpPut("tutorials/{id}/approve")]
        public async Task<IActionResult> ApproveTutorial(int id)
        {
            var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return Ok(await _adminService.ApproveTutorial(adminId, id));
        }



    }
}