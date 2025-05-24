using backend.Dtos;
using backend.Repositories;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend.Controllers
{
    /// <summary>
    /// Controlador para operaciones relacionadas con el usuario autenticado.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        /// <summary>
        /// Inicializa una nueva instancia del controlador <see cref="UserController"/>.
        /// </summary>
        /// <param name="userService">Servicio para manejar la lógica de usuario.</param>
        public UserController(UserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Obtiene el perfil del usuario actualmente autenticado.
        /// </summary>
        /// <returns>Información detallada del perfil del usuario.</returns>
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var result = await _userService.GetProfile(userId);
            return Ok(result);
        }

        /// <summary>
        /// Actualiza los puntos del usuario autenticado.
        /// </summary>
        /// <param name="pointsDto">Objeto que contiene los puntos a actualizar.</param>
        /// <returns>Resultado de la actualización de puntos.</returns>
        [HttpPut("update-points")]
        public async Task<IActionResult> UpdateUserPoints([FromBody] UserUpdatePointsDto pointsDto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var result = await _userService.UpdatePoints(userId, pointsDto.Points);
            return Ok(result);
        }

        /// <summary>
        /// Obtiene los logros alcanzados por el usuario autenticado.
        /// </summary>
        /// <returns>Lista de logros del usuario.</returns>
        [HttpGet("achievements")]
        public async Task<IActionResult> GetUserAchievements()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var achievements = await _userService.GetAchievements(userId);
            return Ok(achievements);
        }
    }
}
