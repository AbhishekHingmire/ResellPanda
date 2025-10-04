using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResellBook.Data;
using ResellBook.Models;

namespace ResellBook.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ChatController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("Send")]
        public async Task<IActionResult> Send([FromBody] ChatMessage message)
        {
            message.Id = Guid.NewGuid();
            message.SentAt = DateTime.UtcNow;

            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();

            return Ok(message);
        }

        [HttpGet("Conversation/{user1Id}/{user2Id}")]
        public async Task<IActionResult> GetConversation(Guid user1Id, Guid user2Id)
        {
            var messages = await _context.ChatMessages
                .Where(m => (m.SenderId == user1Id && m.ReceiverId == user2Id)
                         || (m.SenderId == user2Id && m.ReceiverId == user1Id))
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            return Ok(messages);
        }
    }
}
