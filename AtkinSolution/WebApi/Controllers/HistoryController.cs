using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Data;
using WebApi.Models;
using WebApi.Controllers.Base;
using WebApi.Models.DTOs;

namespace WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class HistoryController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public HistoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetHistory()
        {
            var userId = GetCurrentUserId();
            Console.WriteLine($"Getting history for user {userId}");

            var history = await _context.RequestHistory
                .Where(h => h.UserId == userId)
                .Include(h => h.User)
                .OrderByDescending(h => h.Timestamp)
                .Select(h => new HistoryDto
                {
                    Id = h.Id,
                    Username = h.User.Username,
                    Endpoint = h.Endpoint,
                    Method = h.Method,
                    Timestamp = h.Timestamp
                })
                .ToListAsync();
            
            // Обработка null значений после получения данных
            foreach (var record in history)
            {
                if (string.IsNullOrEmpty(record.Username))
                {
                    record.Username = "Unknown";
                }
            }
            
            Console.WriteLine($"Found {history.Count} records");
            return Ok(history);
        }

        [HttpDelete]
        public async Task<IActionResult> ClearHistory()
        {
            var userId = GetCurrentUserId();
            var history = await _context.RequestHistory
                .Where(h => h.UserId == userId)
                .ToListAsync();
            
            var count = history.Count;
            _context.RequestHistory.RemoveRange(history);
            await _context.SaveChangesAsync();
            
            return Ok(new { 
                message = "История успешно очищена",
                deletedRecords = count 
            });
        }
    }
} 