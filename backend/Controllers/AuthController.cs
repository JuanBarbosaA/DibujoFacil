using backend.Dtos;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly ILogger<AuthController> _logger;

        /// <summary>
        /// Inicializa una nueva instancia del controlador AuthController.
        /// </summary>
        /// <param name="authService">Servicio para la lógica de autenticación.</param>
        /// <param name="logger">Logger para registrar eventos y errores.</param>
        public AuthController(AuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Autentica a un usuario y genera un token JWT si las credenciales son válidas.
        /// </summary>
        /// <param name="loginDto">Datos de inicio de sesión que incluyen email y contraseña.</param>
        /// <returns>Token JWT con información del usuario o respuesta 401 si falla la autenticación.</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var result = await _authService.LoginAsync(loginDto);
            if (result == null)
            {
                return Unauthorized("Credenciales inválidas o usuario no activo");
            }
            return Ok(result);
        }

        /// <summary>
        /// Registra un nuevo usuario en el sistema.
        /// </summary>
        /// <param name="registerDto">Datos requeridos para el registro de usuario.</param>
        /// <returns>Respuesta de éxito con mensaje o conflicto si el email ya está registrado.</returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var success = await _authService.RegisterAsync(registerDto);
            if (!success)
            {
                return Conflict("El email ya está registrado");
            }
            return Ok(new { Message = "Registro exitoso. Por favor verifica tu correo electrónico." });
        }

        /// <summary>
        /// Verifica la cuenta de usuario mediante un token enviado al correo electrónico.
        /// </summary>
        /// <param name="dto">DTO que contiene el token de verificación.</param>
        /// <returns>Confirmación de verificación o error si el token es inválido o ya fue utilizado.</returns>
        [HttpPost("verify-email")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto dto)
        {
            var success = await _authService.VerifyEmailAsync(dto.Token);
            if (!success)
            {
                return BadRequest("Token inválido o correo ya verificado");
            }
            return Ok(new { Message = "¡Correo verificado exitosamente!" });
        }

        /// <summary>
        /// Solicita el envío de un enlace para recuperación de contraseña a un correo registrado.
        /// </summary>
        /// <param name="dto">DTO que contiene el correo electrónico del usuario.</param>
        /// <returns>Respuesta genérica para no revelar la existencia o no del correo.</returns>
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var success = await _authService.ForgotPasswordAsync(dto.Email);
            if (!success)
            {
                return Ok();
            }
            return Ok(new { Message = "Se ha enviado un enlace de recuperación a tu correo" });
        }

        /// <summary>
        /// Restablece la contraseña del usuario mediante token y nueva contraseña.
        /// </summary>
        /// <param name="dto">DTO con token, nueva contraseña y confirmación de contraseña.</param>
        /// <returns>Confirmación de cambio de contraseña o error si el token es inválido o expirado.</returns>
        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var success = await _authService.ResetPasswordAsync(dto);
            if (!success)
            {
                return BadRequest("Token inválido o expirado");
            }
            return Ok(new { Message = "Contraseña actualizada exitosamente" });
        }

        /// <summary>
        /// Elimina la cookie JWT para cerrar sesión del usuario.
        /// </summary>
        /// <returns>Confirmación de cierre de sesión.</returns>
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("jwt");
            return Ok(new { Message = "Logout successful" });
        }
    }
}
