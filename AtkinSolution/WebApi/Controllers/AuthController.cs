using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using BCrypt.Net;
using WebApi.Data;
using WebApi.Models;
using WebApi.Models.DTOs;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApi.Controllers.Base;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Auth controller is working!");
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            if (!IsValidEmail(model.Email))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Некорректный формат email адреса"
                });
            }

            if (await _context.Users.AnyAsync(u => u.Username == model.Username || u.Email == model.Email))
                return BadRequest(new { 
                    success = false,
                    message = "Пользователь с таким именем или email уже существует" 
                });

            var user = new User
            {
                Username = model.Username,
                Email = model.Email.ToLower(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            user.Token = GenerateJwtToken(user);
            await _context.SaveChangesAsync();

            return Ok(new { 
                success = true,
                message = "Регистрация успешно завершена",
                token = user.Token 
            });
        }

        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                if (email.Length > 254)
                    return false;

                email = email.Trim().ToLower();

                var regex = new System.Text.RegularExpressions.Regex(
                    @"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$");

                return regex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == model.Username);
            
            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                return Unauthorized(new { 
                    success = false,
                    message = "Неверное имя пользователя или пароль" 
                });

            user.Token = GenerateJwtToken(user);
            await _context.SaveChangesAsync();

            return Ok(new { 
                success = true,
                message = "Вход выполнен успешно",
                token = user.Token 
            });
        }

        private string GenerateJwtToken(User user)
        {
            Console.WriteLine($"Generating token for user: Id={user.Id}, Username={user.Username}");
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"] 
                ?? throw new InvalidOperationException("JWT secret not configured"));
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            Console.WriteLine($"Generated token: {tokenString}");
            return tokenString;
        }

        [Authorize]
        [HttpPatch("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto model)
        {
            try 
            {
                var userId = GetCurrentUserId();
                var user = await _context.Users.FindAsync(userId) 
                    ?? throw new UnauthorizedAccessException("User not found");

                if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.PasswordHash))
                    return BadRequest(new { 
                        success = false,
                        message = "Неверный текущий пароль" 
                    });

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                user.Token = GenerateJwtToken(user);
                
                await _context.SaveChangesAsync();
                return Ok(new { 
                    success = true,
                    message = "Пароль успешно изменен",
                    token = user.Token 
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ChangePassword: {ex.Message}");
                return StatusCode(500, new { 
                    success = false,
                    message = "Произошла ошибка при смене пароля" 
                });
            }
        }

        [HttpGet("test-token")]
        [Authorize]
        public IActionResult TestToken()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value });
            return Ok(new 
            { 
                Message = "Token is valid",
                Claims = claims
            });
        }
    }
} 