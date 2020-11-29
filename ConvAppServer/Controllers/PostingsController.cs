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
    public class PostingsController : ControllerBase
    {
        private readonly MainContext _context;
        private readonly ILogger _logger;

        public PostingsController(MainContext context, ILogger<PostingsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Posting>> GetPosting(int id)
        {
            _logger.LogInformation($"received GET request for a posting");

            var posting = await _context.Postings
                .Where(p => p.Id == id)
                .Include(p => p.PostingNodes)
                .Include(p => p.Products)
                .AsSplitQuery()
                .SingleAsync();

            if (posting == null)
            {
                return NotFound();
            }

            return posting;
        }

        [HttpGet]
        public async Task<ActionResult<List<Posting>>> GetPostings([FromQuery] DateTime time, [FromQuery] int page, [FromQuery] byte? type = null)
        {
            _logger.LogInformation($"received GET request for some postings\n\ttype{type} time{time} page{page}");

            IQueryable<Posting> query = type == null ? _context.Postings : _context.Postings.Where(p => p.PostingType == type);

            query = query.Where(p => p.CreatedDate <= time);

            int maxPage = (int)Math.Floor((double)(await query.CountAsync()) / 20);

            if (page > maxPage)
                return NoContent();

            // Comments, Likes 포함하지 않는다
            var postings = await query
                .OrderByDescending(p => p.CreatedDate)
                .Skip(20 * page)
                .Take(20)
                .Include(p => p.PostingNodes
                    .OrderBy(pn => pn.OrderIndex))
                .Include(p => p.Products)
                .AsSplitQuery()
                .ToListAsync();

            //var result = new PostingQueryResult { page = option.page, maxPage = maxPage, postings = postings };

            return postings;
        }

        [HttpGet("hot")]
        public async Task<ActionResult<List<Posting>>> GetHotPostings([FromQuery] DateTime time, [FromQuery] int page, [FromQuery] byte? type = null)
        {
            _logger.LogInformation($"GET - hot postings - args type{type} time{time} page{page}");

            IQueryable<Posting> query = type == null ? _context.Postings : _context.Postings.Where(p => p.PostingType == type);

            query = query
                .Where(p => p.CreatedDate <= time)
                .OrderByDescending(p => p.AlltimeScore);

            int maxPage = (int)Math.Floor((double)(await query.CountAsync()) / 20);

            if (page > maxPage)
                return NoContent();

            // Comments, Likes 포함하지 않는다
            var postings = await query
                .Skip(20 * page)
                .Take(20)
                .Include(p => p.PostingNodes
                    .OrderBy(pn => pn.OrderIndex))
                .Include(p => p.Products)
                .AsSplitQuery()
                .ToListAsync();

            //var result = new PostingQueryResult { page = option.page, maxPage = maxPage, postings = postings };

            return postings;
        }

        // POST: api/UserRecipes
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<IActionResult> PostPosting([FromBody] Posting posting)
        {
            _logger.LogInformation($"received POST request");
            try
            {
                //byte[] bytes;
                //using (var ms = new MemoryStream())
                //{
                //    using (var reqStream = Request.Body)
                //        await reqStream.CopyToAsync(ms);
                //    bytes = ms.ToArray();
                //}
                //var json = Encoding.UTF8.GetString(bytes);
                //var posting = JsonConvert.DeserializeObject<Posting>(json);

                byte cnt = 0;
                foreach (var node in posting.PostingNodes)
                {
                    node.OrderIndex = cnt++;
                }

                var prod = new List<Product>();
                foreach (var prodDTO in posting.Products)
                {
                    var product = await _context.Products.FindAsync(prodDTO.Id);
                    prod.Add(product);
                }
                posting.Products = prod;

                _context.Postings.Add(posting);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetPosting), new { id = posting.Id }, posting);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest();
            }
        }
    }

    public class PostingQueryOption
    {
        public DateTime baseTime;
        public int page;
    }

    public class PostingQueryResult
    {
        public int page;
        public int maxPage;
        public List<Posting> postings;
    }
}
