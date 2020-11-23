﻿using ConvAppServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        public async Task<ActionResult<Posting>> GetPostingDetail(int id)
        {
            _logger.LogInformation($"received GET request for a posting");

            var posting = await _context.Postings
                .Where(p => p.Id == id)
                .SingleAsync();

            if (posting == null)
            {
                return NotFound();
            }

            return posting;
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
        public async Task<ActionResult<List<Posting>>> GetPostings([FromQuery(Name = "type")] byte? type = null)
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
                .Include(p => p.Products)
                .AsSplitQuery()
                .ToListAsync();
            }
            else
            {
                return await _context.Postings
                .OrderByDescending(p => p.CreatedDate)
                .Take(20)
                .Include(p => p.PostingNodes
                    .OrderBy(pn => pn.OrderIndex))
                .Include(p => p.Products)
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

                return Ok();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest();
            }
        }
    }
}
