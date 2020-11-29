using ConvAppServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ConvAppServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbacksController : ControllerBase
    {
        private readonly MainContext _context;
        private readonly ILogger<FeedbacksController> _logger;

        public FeedbacksController(MainContext context, ILogger<FeedbacksController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("view")]
        public async Task<ActionResult> AddView([FromBody] View view)
        {
            _logger.LogInformation("Post request for view creation");

            var fiveminutebefore = (DateTime.UtcNow).Subtract(TimeSpan.FromMinutes(5));

            bool recent = await _context.Views
                .Where(v => v.Type == view.Type)
                .Where(v => v.Id == view.Id)
                .Where(v => v.UserId == view.UserId)
                .Where(v => v.Date > fiveminutebefore)
                .AnyAsync();
            if (recent)
                return StatusCode(406);

            switch (view.Type)
            {
                case (byte)FeedbackableType.Posting:
                    (await _context.Postings.FindAsync(view.Id)).ViewCount++;
                    break;
                case (byte)FeedbackableType.Product:
                    (await _context.Products.FindAsync(view.Id)).ViewCount++;
                    break;
                default:
                    return StatusCode(406);
            }

            _context.Views.Add(new View
            {
                Type = view.Type,
                Id = view.Id,
                Date = DateTime.UtcNow,
                UserId = view.UserId
            });
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet]
        public async Task<ActionResult<FeedbackDTO>> GetFeedbacks([FromQuery] byte type, [FromQuery] int id)
        {
            var comments = await _context.Comments
                .Where(c => c.ParentType == type)
                .Where(c => c.ParentId == id)
                .OrderBy(c => c.CreatedDate)
                .ToListAsync();

            var likes = await _context.Likes
                .Where(c => c.ParentType == type)
                .Where(c => c.ParentId == id)
                .ToListAsync();

            return new FeedbackDTO { Comments = comments, Likes = likes };
        }

        public class FeedbackDTO
        {
            public List<Comment> Comments { get; set; }
            public List<Like> Likes { get; set; }
        }

        [HttpGet("comment")]
        public async Task<ActionResult<List<Comment>>> GetComments([FromQuery] byte type, [FromQuery] int id)
        {
            var query = _context.Comments
                .Where(c => c.ParentType == type)
                .Where(c => c.ParentId == id)
                .OrderBy(c => c.CreatedDate);

            //var total = await query.CountAsync();
            //var maxPage = Math.Ceiling((double)total / pageSize) - 1;

            //if (page != null)
            //{
            //    if (page > maxPage)
            //        return NoContent();

            //    query = (IOrderedQueryable<Comment>)query.Skip((int)(pageSize * page)).Take(pageSize);
            //}

            var comments = await query.ToListAsync();

            return comments;
        }

        [HttpPost("comment")]
        public async Task<ActionResult<List<Comment>>> PostComment([FromQuery] byte type, [FromQuery] int id, [FromBody] Comment comment)
        {
            var cmt = new Comment { ParentType = type, ParentId = id, CreatorId = comment.CreatorId, Text = comment.Text };
            _context.Comments.Add(cmt);
            await _context.SaveChangesAsync();

            // 문제가 없었다면 코멘트수 +1로 이어져야
            // 여기서 Concurrency issue 발생할 여지가 있어 보이는데...

            //switch (type)
            //{
            //    case (byte)FeedbackableType.Posting:
            //        (await _context.Postings.FindAsync(id)).CommentCount
                        
            //    (byte)FeedbackableType.Product => _context.Products,
            //    (byte)FeedbackableType.Comment => _context.Comments,
            //    (byte)FeedbackableType.User => _context.Users,
            //    _ => throw new Exception()
            //};

            return CreatedAtAction(nameof(GetComments), new { type, id }, cmt);
        }

        [HttpDelete("comment")]
        public async Task<ActionResult<object>> DeleteComment([FromQuery] byte type, [FromQuery] int id, [FromBody] Comment comment)
        {
            var result = await _context.Comments.FindAsync(type, id, comment.CreatedDate);

            if (result == null)
                return NotFound();

            _context.Comments.Remove(result);
            await _context.SaveChangesAsync();

            return await GetComments(type, id);
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
