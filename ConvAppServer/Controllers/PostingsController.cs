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
    [Route("api/[controller]")]
    [ApiController]
    public class PostingsController : ControllerBase
    {
        private readonly SqlContext _context;
        private readonly ILogger _logger;

        public PostingsController(SqlContext context, ILogger<PostingsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Posting>> GetPosting(int id)
        {
            _logger.LogInformation($"received GET request for postings");

            var posting = await _context.Postings.FindAsync(id);

            if (posting == null)
            {
                return NotFound();
            }

            return posting;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Posting>>> GetPostings(
            [FromQuery(Name = "start")] int start = 0,
            [FromQuery(Name = "end")] int end = 20,
            [FromQuery(Name = "isRecipe")] bool? isRecipe = false)
        {
            _logger.LogInformation($"received GET request for postings\n\twith Querystring - start{start} end{end} isRecipe{isRecipe}");

            var query = _context.Postings
                .Where(p => p.IsRecipe == isRecipe)
                .OrderBy(p => p.Created)
                .Skip(start).Take(end)
                .Include(p => p.PostingNodes);

            return await query.ToListAsync();
        }

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<Posting>>> GetPostingsAll(
            [FromQuery(Name = "start")] int start = 0,
            [FromQuery(Name = "end")] int end = 20)
        {
            _logger.LogInformation($"received GET request for all postings\n\twith Querystring - start{start} end{end}");

            var query = _context.Postings
                .OrderBy(p => p.Created)
                .Skip(start).Take(end)
                .Include(p => p.PostingNodes);

            return await query.ToListAsync();
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
                var posting = JsonConvert.DeserializeObject<Posting>(json);

                posting.Created = DateTime.UtcNow;

                foreach (var node in posting.PostingNodes)
                {
                    node.OrderIndex = (byte)posting.PostingNodes.IndexOf(node);
                }

                _context.Postings.Add(posting);
                
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetPosting", new { posting.Id }, posting);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest();
            }
        }
    }
}
