using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ConvAppServer.Models;

namespace ConvAppServer.Controllers
{
    public class PostingContext : DbContext
    {
        public PostingContext(DbContextOptions<PostingContext> options) : base(options) { }

        public DbSet<Posting> Posting { get; set; }
        public DbSet<PostingNode> PostingNode { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class PostingsController : ControllerBase
    {
        private readonly PostingContext _context;
        private readonly ILogger _logger;

        public PostingsController(PostingContext context, ILogger<PostingsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Posting>>> GetPostings()
        {
            _logger.LogInformation($"received GET request for all postings");
            return await _context.Posting.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Posting>> GetPosting(long id)
        {
            _logger.LogInformation($"received GET request for {id}");
            var posting = await _context.Posting.FindAsync(id);

            if (posting == null)
            {
                return NotFound();
            }

            return posting;
        }

        // PUT: api/UserRecipes/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPosting(long id, PostingSet posting)
        {
            if (id != posting.id)
            {
                return BadRequest();
            }

            posting.modify_date = DateTime.Now;
            _context.Entry(posting).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PostingExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/UserRecipes
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<IActionResult> PostPosting()
        {
            _logger.LogInformation($"received POST request");
            try
            {
                byte[] bytes;
                using (var ms = new MemoryStream())
                {
                    using (var reqStream = Request.Body)
                        await reqStream.CopyToAsync(ms);
                    bytes = ms.ToArray();
                }
                var json = Encoding.UTF8.GetString(bytes);
                var postingSet = JsonConvert.DeserializeObject<PostingSet>(json);

                postingSet.create_date = DateTime.UtcNow;

                byte order = 0;
                foreach (var item in postingSet.PostingNodes)
                    _context.PostingNode.Add(new PostingNode
                    {
                        parent_id = postingSet.id,
                        order_number = order++,
                        images = item.image,
                        text = item.text
                    });

                _context.Posting.Add(postingSet);
                
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetPosting", new { postingSet.id }, postingSet);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest();
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Posting>> DeletePosting(long id)
        {
            var posting = await _context.Posting.FindAsync(id);
            if (posting == null)
            {
                return NotFound();
            }

            _context.Posting.Remove(posting);
            await _context.SaveChangesAsync();

            return posting;
        }

        private bool PostingExists(long id)
        {
            return _context.Posting.Any(e => e.id == id);
        }
    }
}
