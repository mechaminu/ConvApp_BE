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
        public async Task<ActionResult<Posting>> GetPostingDetail(int id)
        {
            _logger.LogInformation($"received GET request for a posting");

            var result = await _context.Postings
                .Where(p => p.Id == id)
                .SingleAsync();

            if (result == null)
            {
                return NotFound();
            }

            return result;
        }


        [HttpGet("latest")]
        public async Task<ActionResult<int>> GetLatestPostingId()
        {
            _logger.LogInformation($"received GET request for id of the latest posting");
            int result;

            try
            {
                result = await _context.Postings
                .OrderByDescending(p => p.CreatedDate)
                .Select(p => p.Id)
                .FirstAsync();
            }
            catch
            {
                result = 0;
            }
            

            return result;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Posting>>> GetPostings([FromQuery(Name = "type")] byte? type = null)
        {
            _logger.LogInformation($"received GET request for some postings\n\ttype{type}");

            if (type != null)
            {
                return await _context.Postings
                .Where(p => p.PostingType == type)
                .OrderByDescending(p => p.CreatedDate)
                .Take(20)
                .Include(p => p.PostingNodes
                    .OrderBy(pn => pn.OrderIndex))
                .ToListAsync();
            }
            else
            {
                return await _context.Postings
                .OrderByDescending(p => p.CreatedDate)
                .Take(20)
                .Include(p => p.PostingNodes
                    .OrderBy(pn => pn.OrderIndex))
                .ToListAsync();
            }
        }

        /// <summary>
        /// 무한스크롤 구현을 위한 페이징 쿼리
        /// </summary>
        /// <param name="offsetId">페이지 시작 포스팅 id</param>
        /// <param name="type">포스팅 종류</param>
        /// <param name="page">페이지 번호</param>
        /// <returns></returns>
        [HttpGet("{offsetId}")]
        public async Task<ActionResult<IEnumerable<Posting>>> GetPostingsOffset(
            int offsetId,
            [FromQuery(Name = "type")] byte? type = null,
            [FromQuery(Name = "page")] int page = 0)
        {
            _logger.LogInformation($"received GET request from scrolling for postings\n\toffsetId{offsetId} type{type} page{page}");

            var result = await _context.Postings
                .Where(p => p.Id < offsetId)
                .Where(p => type == null || p.PostingType == type)
                .OrderByDescending(p => p.CreatedDate)
                .Skip(10 * (page - 1))
                .Take(10 * page)
                .Include(p => p.PostingNodes
                    .OrderBy(pn => pn.OrderIndex))
                .ToListAsync();

            return result;
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

                foreach (var node in posting.PostingNodes)
                {
                    node.OrderIndex = (byte)posting.PostingNodes.IndexOf(node);
                }

                _context.Postings.Add(posting);
                await _context.SaveChangesAsync();

                return Ok();
                //return CreatedAtAction("GetPostingDetail", new { posting.Id }, posting);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest();
            }
        }
    }
}
