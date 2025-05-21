using backend.Dtos;
using backend.Repositories.Models;
using backend.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DbDibujofacilContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;
        private readonly EmailManager _emailManager;

        public AuthController(
            DbDibujofacilContext context,
            IConfiguration configuration,
            ILogger<AuthController> logger,
            EmailManager emailManager) 
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _emailManager = emailManager;
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null) return Ok(); 

            var token = JwtUtility.GeneratePasswordResetToken(
                email: user.Email,
                jwtKey: _configuration["JwtSettings:Key"],
                expirationMinutes: 60
            );

            _emailManager.SendPasswordResetEmail(user.Email, token);

            return Ok(new { Message = "Se ha enviado un enlace de recuperación a tu correo" });
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            try
            {
                var principal = JwtUtility.ValidatePasswordResetToken(
                    token: dto.Token,
                    jwtKey: _configuration["JwtSettings:Key"]
                );

                var email = principal.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(email)) return BadRequest("Token inválido");

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null) return BadRequest("Usuario no encontrado");

                user.PasswordHash = EncryptUtility.HashPassword(dto.NewPassword);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Contraseña actualizada exitosamente" });
            }
            catch (SecurityTokenExpiredException)
            {
                return BadRequest("El enlace ha expirado");
            }
            catch (Exception)
            {
                return BadRequest("Token inválido");
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Modelo inválido: {Errors}", ModelState.Values);
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Intento de login para: {Email}", loginDto.Email);

                var user = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

                if (user == null)
                {
                    _logger.LogWarning("Usuario no encontrado para el email: {Email}", loginDto.Email);
                    return Unauthorized("Credenciales inválidas");
                }

                if (user.Status != "active") 
                {
                    return Unauthorized("Por favor verifica tu correo electrónico");
                }

                var isPasswordValid = EncryptUtility.VerifyPassword(loginDto.Password, user.PasswordHash);

                if (!isPasswordValid)
                {
                    _logger.LogWarning("Credenciales inválidas para el usuario: {Email}", loginDto.Email);
                    return Unauthorized("Credenciales inválidas");
                }

                if (!EncryptUtility.IsBCryptHash(user.PasswordHash))
                {
                    try
                    {
                        var newHash = EncryptUtility.MigrateFromSHA256(loginDto.Password);
                        user.PasswordHash = newHash;
                        _context.Update(user);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("Contraseña migrada a BCrypt para el usuario: {Email}", loginDto.Email);
                    }
                    catch (Exception migrationEx)
                    {
                        _logger.LogError(migrationEx, "Error migrando contraseña para el usuario: {Email}", loginDto.Email);
                    }
                }

                var token = JwtUtility.GenerateToken(
                    email: user.Email,
                    userId: user.Id,
                    jwtKey: _configuration["JwtSettings:Key"],
                    jwtIssuer: _configuration["JwtSettings:Issuer"],
                    jwtAudience: _configuration["JwtSettings:Audience"]);
     

                _logger.LogInformation("Login exitoso para el usuario: {Email}", loginDto.Email);

                return Ok(new AuthResponseDto
                {
                    Token = token,
                    Expiration = DateTime.Now.AddMinutes(int.Parse(_configuration["JwtSettings:ExpirationInMinutes"])),
                    Email = user.Email,
                    UserId = user.Id.ToString()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el proceso de login para el email: {Email}", loginDto.Email);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(registerDto.Email))
                    return BadRequest("Email es requerido");

                if (string.IsNullOrWhiteSpace(registerDto.Password))
                    return BadRequest("Contraseña es requerida");

                if (string.IsNullOrWhiteSpace(registerDto.Name))
                    return BadRequest("Nombre es requerido");

                if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
                    return Conflict("El email ya está registrado");

                var user = new User
                {
                    Email = registerDto.Email,
                    PasswordHash = EncryptUtility.HashPassword(registerDto.Password),
                    Name = registerDto.Name,
                    RoleId = 2,
                    RegistrationDate = DateTime.UtcNow,
                    Status = "pending", 
                    Points = 0
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var verificationToken = JwtUtility.GenerateStateToken(
                    email: user.Email,
                    jwtKey: _configuration["JwtSettings:Key"],
                    claimType: "email_verification",
                    expirationMinutes: 1440 
                );

                _emailManager.SendVerificationEmail(user.Email, verificationToken);

                return Ok(new
                {
                    Message = "Registro exitoso. Por favor verifica tu correo electrónico.",
                    UserId = user.Id,
                    Email = user.Email
                });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Error de base de datos al registrar usuario");
                return StatusCode(500, new
                {
                    Message = "Error al registrar el usuario",
                    Detail = dbEx.InnerException?.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al registrar usuario");
                return StatusCode(500, new
                {
                    Message = "Error interno del servidor",
                    Detail = ex.Message
                });
            }
        }


        [HttpPost("verify-email")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto dto)
        {
            try
            {
                var principal = JwtUtility.ValidateToken(
                    dto.Token,
                    _configuration["JwtSettings:Key"],
                    expectedClaim: "email_verification"
                );

                var email = principal.FindFirst(ClaimTypes.Email)?.Value;
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

                if (user == null) return NotFound("Usuario no encontrado");
                if (user.Status == "active") return BadRequest("El correo ya fue verificado");

                user.Status = "active"; 
                await _context.SaveChangesAsync();

                return Ok(new { Message = "¡Correo verificado exitosamente!" });
            }
            catch (SecurityTokenExpiredException)
            {
                return BadRequest("El enlace de verificación ha expirado");
            }
            catch (Exception)
            {
                return BadRequest("Token inválido");
            }
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("jwt");
            return Ok(new { Message = "Logout successful" });
       
        }



        [HttpGet("test-connection")]
        public IActionResult TestConnection()
        {
            try
            {
                using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    connection.Open();
                    var cmd = new SqlCommand(
                        "SELECT SUSER_SNAME() AS [current_user], DB_NAME() AS [current_db], " +
                        "CASE WHEN IS_MEMBER('db_owner') = 1 THEN 'db_owner' " +
                        "WHEN IS_MEMBER('db_datareader') = 1 THEN 'db_datareader' " +
                        "ELSE 'sin_permisos' END AS [database_role]",
                        connection);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return Ok(new
                            {
                                Usuario = reader["current_user"],
                                BaseDatos = reader["current_db"],
                                Rol = reader["database_role"],
                                ConexionExitosa = true,
                                HoraServidor = DateTime.Now
                            });
                        }
                    }
                }
                return BadRequest("No se pudo leer información de conexión");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en test-connection");
                return StatusCode(500, new
                {
                    Error = ex.Message,
                    Detalles = ex.InnerException?.Message
                });
            }
        }
    }
}


[Authorize]
[Route("api/[controller]")]
[ApiController]
public class ProtectedController : ControllerBase
{
    private readonly DbDibujofacilContext _context;

    public ProtectedController(DbDibujofacilContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var user = await _context.Users.FindAsync(userId);

        return Ok(new
        {
            Message = "Este es un endpoint protegido",
            Usuario = user?.Name, 
            Email = User.Identity.Name, 
            Fecha = DateTime.UtcNow
        });
    }
}