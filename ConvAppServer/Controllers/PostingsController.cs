using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ConvAppServer.Models;
using Newtonsoft.Json;
using System.IO;
using Microsoft.Extensions.Logging;

namespace ConvAppServer.Controllers
{
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
            return await _context.Postings.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Posting>> GetPosting(long id)
        {
            _logger.LogInformation($"received GET request for {id}");
            var posting = await _context.Postings.FindAsync(id);

            if (posting == null)
            {
                return NotFound();
            }

            return posting;
        }

        #region 페이지네이션 데이터 제공
        // 페이지 총 갯수 리턴
        [HttpGet("page")]
        public async Task<IActionResult> GetPostingTotalPage()
        {
            var cntList = await _context.Postings.CountAsync();
            int itemsPerPage = 10;
            int totalPage = cntList / itemsPerPage;

            return Ok(cntList % itemsPerPage != 0 ? totalPage + 1 : totalPage);
        }

        // 원하는 페이지에 해당하는 아이템 리스트 리턴
        [HttpGet("page/{page}")]
        public async Task<IActionResult> GetPostingPage(int page)
        {
            return Ok((await _context.Postings.ToListAsync()).Skip(page * 10).Take(10).ToList());
        }
        #endregion


        #region 인피니트 스크롤 데이터 제공
        // TODO
        #endregion

        // PUT: api/UserRecipes/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPosting(long id, Posting posting)
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
        //[HttpPost]
        //public async Task<ActionResult<Posting>> PostUserRecipe(Posting posting)
        //{
        //    _context.Postings.Add(posting);
        //    await _context.SaveChangesAsync();

        //    return CreatedAtAction("GetPosting", new { posting.id }, posting);
        //}
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PostPosting(Posting posting)
        {
            _logger.LogInformation($"received POST request");
            try
            {
                posting.create_date = DateTime.Now;

                _context.Postings.Add(posting);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                BadRequest(e);
            }
            
            return new CreatedAtActionResult("GetPosting", "Postings", new { posting.id }, posting);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Posting>> DeletePosting(long id)
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

        private bool PostingExists(long id)
        {
            return _context.Postings.Any(e => e.id == id);
        }
    }
}
