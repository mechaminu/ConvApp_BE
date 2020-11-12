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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Posting>>> GetPostings()
        {
            _logger.LogInformation("received GET request for all postings");
            return await _context.Postings.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Posting>> GetPosting(int id)
        {
            _logger.LogInformation($"received GET request for {id}");
            var posting = await _context.Postings.FindAsync(id);

            if (posting == null)
            {
                return NotFound();
            }

            return posting;
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

        [HttpDelete("{id}")]
        public async Task<ActionResult<Posting>> DeletePosting(int id)
        {
            var posting = await _context.Postings.FindAsync(id);
            if (posting == null)
            {
                return NotFound();
            }

            _context.Postings.Remove(posting);
            await _context.SaveChangesAsync();

            return posting;
        }

        private bool PostingExists(int id)
        {
            return _context.Postings.Any(e => e.Id == id);
        }
    }
}
