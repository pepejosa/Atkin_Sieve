using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApi.Services;
using WebApi.Data;
using WebApi.Controllers.Base;
using WebApi.Models;

namespace WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SieveController : BaseController
    {
        private readonly SieveService _sieveService;
        private readonly ApplicationDbContext _context;

        public SieveController(SieveService sieveService, ApplicationDbContext context)
        {
            _sieveService = sieveService;
            _context = context;
        }

        [HttpPost("primes/count")]
        public async Task<IActionResult> GetNPrimes([FromBody] int n)
        {
            await LogRequest();
            var primes = _sieveService.GetNPrimes(n);
            return Ok(primes);
        }

        [HttpPost("primes/range")]
        public async Task<IActionResult> GetPrimesUpToN([FromBody] int n)
        {
            await LogRequest();
            var primes = _sieveService.GetPrimesUpToN(n);
            return Ok(primes);
        }

        [HttpPost("visualization")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetSieveVisualization(
            [FromBody] int n,
            [FromQuery(Name = "format")] string format = "text")
        {
            if (string.IsNullOrEmpty(format))
            {
                return BadRequest(new { error = "Параметр format обязателен. Допустимые значения: 'text', 'base64', 'binary', 'url'" });
            }
            
            if (!new[] { "text", "base64", "binary", "url" }.Contains(format.ToLower()))
            {
                return BadRequest(new { error = "Недопустимый формат. Используйте 'text', 'base64', 'binary' или 'url'" });
            }
            
            await LogRequest();

            switch (format.ToLower())
            {
                case "text":
                    var textVisualization = _sieveService.GenerateVisualization(n, "text");
                    return Ok(new { Result = textVisualization });

                case "base64":
                    var base64Visualization = _sieveService.GenerateVisualization(n, "base64");
                    return Ok(new { Result = base64Visualization });

                case "binary":
                    var binaryData = _sieveService.GenerateImageAsBinary(n);
                    return File(binaryData, "image/png", $"sieve_{n}.png");

                case "url":
                    var fileName = await _sieveService.SaveImageAndGetUrl(n);
                    var url = $"{Request.Scheme}://{Request.Host}/images/{fileName}";
                    return Ok(new { Url = url });

                default:
                    return BadRequest(new { error = "Неподдерживаемый формат" });
            }
        }

        private async Task LogRequest()
        {
            var userId = GetCurrentUserId();
            var user = await _context.Users.FindAsync(userId) 
                ?? throw new UnauthorizedAccessException("User not found");
            
            var request = new RequestHistory
            {
                UserId = userId,
                Endpoint = HttpContext.Request.Path,
                Method = HttpContext.Request.Method,
                Timestamp = DateTime.UtcNow,
                User = user
            };

            _context.RequestHistory.Add(request);
            await _context.SaveChangesAsync();
        }
    }
} 