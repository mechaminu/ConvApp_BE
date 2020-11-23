using ConvAppServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConvAppServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbacksController : ControllerBase
    {
        private readonly MainContext _context;
        private readonly ILogger _logger;

        private readonly int pageSize = 10;

        public FeedbacksController(MainContext context, ILogger<FeedbacksController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<FeedbackDTO>> GetFeedbacks([FromQuery] byte type, [FromQuery] int id, [FromQuery] DateTime? time = null)
        {
            var query = _context.Comments
                .Where(c => c.ParentType == type)
                .Where(c => c.ParentId == id);

            if (time != null)
                query = query.Where(c => c.CreatedDate <= time);

            var comments = await query.OrderBy(c => c.CreatedDate)
                .ToListAsync();

            var likes = await _context.Likes
                .Where(c => c.ParentType == type)
                .Where(c => c.ParentId == id)
                .ToListAsync();

            return new FeedbackDTO { comments = comments, likes = likes };
        }

        public class FeedbackDTO
        {
            public List<Comment> comments { get; set; }
            public List<Like> likes { get; set; }
        }

        [HttpGet("comment")]
        public async Task<ActionResult<object>> GetComments([FromQuery] byte type, [FromQuery] int id, [FromQuery] DateTime time, [FromQuery] int? page = null)
        {
            var query = _context.Comments
                .Where(c => c.ParentType == type)
                .Where(c => c.ParentId == id)
                .Where(c => c.CreatedDate < time)
                .OrderBy(c => c.CreatedDate);

            var total = await query.CountAsync();
            var maxPage = Math.Ceiling((double)total / pageSize) - 1;

            if (page != null)
            {
                if (page > maxPage)
                    return NoContent();

                query = (IOrderedQueryable<Comment>)query.Skip((int)(pageSize * page)).Take(pageSize);
            }

            var comments = await query.ToListAsync();

            return new { page, maxPage, comments };
        }

        [HttpPost("comment")]
        public async Task<ActionResult<object>> PostComment([FromQuery] byte type, [FromQuery] int id, [FromBody] Comment comment)
        {
            _context.Comments.Add(new Comment { ParentType = type, ParentId = id, CreatorId = comment.CreatorId, Text = comment.Text });
            await _context.SaveChangesAsync();

            return await GetComments(type, id, DateTime.UtcNow);
        }

        [HttpDelete("comment")]
        public async Task<ActionResult<object>> DeleteLike([FromQuery] byte type, [FromQuery] int id, [FromBody] Comment comment)
        {
            var result = await _context.Comments.FindAsync(type, id, comment.CreatedDate);

            if (result == null)
                return NotFound();

            _context.Comments.Remove(result);
            await _context.SaveChangesAsync();

            return await GetComments(type, id, DateTime.UtcNow);
        }

        [HttpGet("like")]
        public async Task<ActionResult<List<Like>>> GetLikes([FromQuery] byte type, [FromQuery] int id)
        {
            var likes = await _context.Likes
                .Where(c => c.ParentType == type)
                .Where(c => c.ParentId == id)
                .ToListAsync();

            if (likes == null)
                return NotFound();

            return likes;
        }

        [HttpPost("like")]
        public async Task<ActionResult<List<Like>>> PostLike([FromQuery] byte type, [FromQuery] int id, [FromBody] Like like)
        {
            var result = new Like { ParentType = type, ParentId = id, CreatorId = like.CreatorId };
            _context.Likes.Add(result);
            await _context.SaveChangesAsync();

            return await GetLikes(type, id);
        }

        [HttpDelete("like")]
        public async Task<ActionResult<List<Like>>> DeleteLike([FromQuery] byte type, [FromQuery] int id, [FromBody] Like like)
        {
            var result = await _context.Likes.FindAsync(type, id, like.CreatorId);

            if (result == null)
                return NotFound();

            _context.Likes.Remove(result);
            await _context.SaveChangesAsync();

            return await GetLikes(type, id);
        }
    }
}
