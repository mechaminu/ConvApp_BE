using ConvAppServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
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
            var sw = new Stopwatch();
            sw.Start();

            var fiveminutebefore = (DateTime.UtcNow).Subtract(TimeSpan.FromMinutes(5));

            bool recent = await _context.Views
                .Where(v => v.ParentType == view.ParentType)
                .Where(v => v.ParentId == view.ParentId)
                .Where(v => v.UserId == view.UserId)
                .Where(v => v.CreatedDate > fiveminutebefore)
                .AnyAsync();

            if (recent)
            {
                sw.Stop();
                _logger.LogInformation($"posting view for {view.ParentType}/{view.ParentId} rejected (recent view exist) - {sw.ElapsedMilliseconds}ms");
                return StatusCode(406);
            }

            // 굳이 필요할까? 랭킹낼때 View 다 찾는 로직으로 운영되는데...
            switch (view.ParentType)
            {
                case (byte)FeedbackableType.Posting:
                    (await _context.Postings.FindAsync(view.ParentId)).ViewCount++;
                    break;
                case (byte)FeedbackableType.Product:
                    (await _context.Products.FindAsync(view.ParentId)).ViewCount++;
                    break;
                default:
                    sw.Stop();
                    _logger.LogError($"posting view for {view.ParentType}/{view.ParentId} failed (invaild ParentType) - {sw.ElapsedMilliseconds}ms");
                    return StatusCode(406);
            }

            _context.Views.Add(new View
            {
                ParentType = view.ParentType,
                ParentId = view.ParentId,
                UserId = view.UserId
            });
            await _context.SaveChangesAsync();

            sw.Stop();
            _logger.LogInformation($"posting view for {view.ParentType}/{view.ParentId} successful - {sw.ElapsedMilliseconds}ms");
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
            var comments = await _context.Comments
                .Where(c => c.ParentType == type)
                .Where(c => c.ParentId == id)
                .OrderBy(c => c.CreatedDate)
                .ToListAsync();

            return comments;
        }

        [HttpPost("comment")]
        public async Task<ActionResult> PostComment([FromBody] Comment comment)
        {
            _context.Comments.Add(comment);
            (await _context.GetFeedbackable((FeedbackableType)comment.ParentType, comment.ParentId)).CommentCount++;
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("comment")]
        public async Task<ActionResult> DeleteComment([FromQuery] int id)
        {
            var result = await _context.Comments.FindAsync(id);

            if (result == null)
                return NotFound();

            var likes = await _context.Likes
                .Where(l => l.ParentType == (byte)FeedbackableType.Comment)
                .Where(l => l.ParentId == id)
                .ToListAsync();

            likes.ForEach(l => _context.Likes.Remove(l));

            var cmts = await _context.Comments
                .Where(l => l.ParentType == (byte)FeedbackableType.Comment)
                .Where(l => l.ParentId == id)
                .ToListAsync();

            cmts.ForEach(c => _context.Comments.Remove(c));

            (await _context.GetFeedbackable((FeedbackableType)result.ParentType, result.ParentId)).CommentCount--;
            _context.Comments.Remove(result);

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("like")]
        public async Task<ActionResult<List<Like>>> GetLikes([FromQuery] byte type, [FromQuery] int id)
        {
            var likes = await _context.Likes
                .Where(c => c.ParentType == type)
                .Where(c => c.ParentId == id)
                .ToListAsync();

            return likes;
        }

        [HttpPost("like")]
        public async Task<ActionResult> PostLike([FromBody] Like like)
        {
            _context.Likes.Add(like);
            (await _context.GetFeedbackable((FeedbackableType)like.ParentType, like.ParentId)).LikeCount++;
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("like")]
        public async Task<ActionResult> DeleteLike([FromBody] Like like)
        {
            var result = await _context.Likes
                .Where(l => l.ParentType == like.ParentType && l.ParentId == like.ParentId && l.UserId == like.UserId)
                .SingleAsync();

            if (result == null)
                return NotFound();

            _context.Likes.Remove(result);
            (await _context.GetFeedbackable((FeedbackableType)like.ParentType, like.ParentId)).LikeCount--;
            await _context.SaveChangesAsync();

            return Ok();
        }


    }
}
