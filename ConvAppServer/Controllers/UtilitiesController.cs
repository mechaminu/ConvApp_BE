using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using ConvAppServer.Models;

namespace ConvAppServer.Controllers
{
    [Route("api")]
    [ApiController]
    public class UtilitiesController : ControllerBase
    {
        private readonly MainContext _context;
        private readonly ILogger<UtilitiesController> _logger;

        public UtilitiesController(MainContext context, ILogger<UtilitiesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("ranking")]
        public async Task<ActionResult> CalcScores()
        {
            Stopwatch sw = new Stopwatch();

            sw.Start();
            var postings = await _context.Postings
                .ToListAsync();

            foreach (var posting in postings)
            {
                var likeCnt = await _context.Likes
                    .Where(l => l.ParentType == (byte)FeedbackableType.Posting && l.ParentId == posting.Id)
                    .CountAsync();

                var viewCnt = await _context.Views
                    .Where(v => v.Type == (byte)FeedbackableType.Posting && v.Id == posting.Id)
                    .CountAsync();

                var cmtList = await _context.Comments
                    .Where(c => c.ParentType == (byte)FeedbackableType.Posting && c.ParentId == posting.Id)
                    .Select(c => c.Id)
                    .ToListAsync();

                async Task<int> cmtCntCalc(int id)
                {
                    var subCmtList = await _context.Comments
                        .Where(_c => _c.ParentType == (byte)FeedbackableType.Comment && _c.ParentId == id)
                        .Select(c => c.Id)
                        .ToListAsync();

                    int result = 0;
                    foreach (var _id in subCmtList)
                        result += await cmtCntCalc(_id);

                    return result + 1;
                }

                int cmtCnt = 0;
                foreach (var id in cmtList)
                    cmtCnt += await cmtCntCalc(id);

                posting.AlltimeScore = likeCnt * 0.7 + cmtCnt * 0.2 + viewCnt * 0.1;
            }
            sw.Stop();
            _logger.LogInformation($"Postings score evaluation - {postings.Count} entries - {(double)sw.ElapsedMilliseconds / 1000} sec ({sw.ElapsedMilliseconds / postings.Count}ms per entry)");

            var products = await _context.Products
                .Include(p => p.Postings)
                .ThenInclude(p => p.PostingNodes)
                .ToListAsync();

            foreach (var product in products)
            {
                int recipes = 0;
                int recipeLikes = 0;

                int reviews = 0;
                double reviewSum = 0;

                foreach (var posting in product.Postings)
                {
                    if (posting.PostingType == 1)
                    {
                        recipes++;
                        recipeLikes += await _context.Likes
                            .Where(l => l.ParentType == (byte)FeedbackableType.Posting && l.ParentId == posting.Id)
                            .CountAsync();
                    }
                    else
                    {
                        reviews++;
                        reviewSum += double.Parse(posting.PostingNodes.Where(pn => pn.OrderIndex == 0).Single().Text);
                    }
                }

                var viewCnt = await _context.Views
                    .Where(v => v.Type == (byte)FeedbackableType.Product && v.Id == product.Id)
                    .CountAsync();

                var likeCnt = await _context.Likes
                    .Where(l => l.ParentType == (byte)FeedbackableType.Product && l.ParentId == product.Id)
                    .CountAsync();

                product.AlltimeScore = likeCnt * 0.5 + recipeLikes * 0.2 + (reviews + recipes) * 0.2 + viewCnt * 0.1;
            }

            sw.Restart();
            await _context.SaveChangesAsync();
            sw.Stop();
            _logger.LogInformation($"saving changes - {(double)sw.ElapsedMilliseconds / 1000} sec");

            return Ok();
        }
    }
}
