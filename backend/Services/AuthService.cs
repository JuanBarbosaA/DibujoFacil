using backend.Dtos;
using backend.Repositories;
using backend.Repositories.Models;
using backend.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace backend.Services
{
    public class AuthService
    {
        private readonly UserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;
        private readonly EmailManager _emailManager;

        public AuthService(
            UserRepository userRepository,
            IConfiguration configuration,
            ILogger<AuthService> logger,
            EmailManager emailManager)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _logger = logger;
            _emailManager = emailManager;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _userRepository.GetByEmailAsync(loginDto.Email);
            if (user == null || user.Status != "active")
            {
                return null;
            }

            var isPasswordValid = EncryptUtility.VerifyPassword(loginDto.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                return null;
            }

            if (!EncryptUtility.IsBCryptHash(user.PasswordHash))
            {
                try
                {
                    var newHash = EncryptUtility.MigrateFromSHA256(loginDto.Password);
                    user.PasswordHash = newHash;
                    await _userRepository.UpdateAsync(user);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error migrating password for user: {Email}", loginDto.Email);
                }
            }

            var token = JwtUtility.GenerateToken(
                email: user.Email,
                userId: user.Id,
                jwtKey: _configuration["JwtSettings:Key"],
                jwtIssuer: _configuration["JwtSettings:Issuer"],
                jwtAudience: _configuration["JwtSettings:Audience"]);

            return new AuthResponseDto
            {
                Token = token,
                Expiration = DateTime.Now.AddMinutes(int.Parse(_configuration["JwtSettings:ExpirationInMinutes"])),
                Email = user.Email,
                UserId = user.Id.ToString()
            };
        }

        public async Task<bool> RegisterAsync(RegisterDto registerDto)
        {
            if (await _userRepository.ExistsByEmailAsync(registerDto.Email))
            {
                return false;
            }

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

            await _userRepository.AddAsync(user);

            var verificationToken = JwtUtility.GenerateStateToken(
                email: user.Email,
                jwtKey: _configuration["JwtSettings:Key"],
                claimType: "email_verification",
                expirationMinutes: 1440);

            _emailManager.SendVerificationEmail(user.Email, verificationToken);

            return true;
        }

        public async Task<bool> VerifyEmailAsync(string token)
        {
            try
            {
                var principal = JwtUtility.ValidateToken(
                    token,
                    _configuration["JwtSettings:Key"],
                    expectedClaim: "email_verification");

                var email = principal.FindFirst(ClaimTypes.Email)?.Value;
                var user = await _userRepository.GetByEmailAsync(email);

                if (user == null || user.Status == "active")
                {
                    return false;
                }

                user.Status = "active";
                await _userRepository.UpdateAsync(user);

                return true;
            }
            catch (SecurityTokenExpiredException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ForgotPasswordAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                return false;
            }

            var token = JwtUtility.GeneratePasswordResetToken(
                email: user.Email,
                jwtKey: _configuration["JwtSettings:Key"],
                expirationMinutes: 60);

            _emailManager.SendPasswordResetEmail(user.Email, token);

            return true;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDto dto)
        {
            try
            {
                var principal = JwtUtility.ValidatePasswordResetToken(
                    token: dto.Token,
                    jwtKey: _configuration["JwtSettings:Key"]);

                var email = principal.FindFirst(ClaimTypes.Email)?.Value;
                var user = await _userRepository.GetByEmailAsync(email);

                if (user == null)
                {
                    return false;
                }

                user.PasswordHash = EncryptUtility.HashPassword(dto.NewPassword);
                await _userRepository.UpdateAsync(user);

                return true;
            }
            catch (SecurityTokenExpiredException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
