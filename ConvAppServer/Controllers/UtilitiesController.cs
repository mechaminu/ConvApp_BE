﻿using ConvAppServer.Models;
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
            _logger.LogDebug("UtilitesController created");
        }

        [HttpGet("ranking")]
        public async Task<ActionResult> CreateRankingScores()
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
                    .Where(v => v.ParentType == (byte)FeedbackableType.Posting && v.ParentId == posting.Id)
                    .CountAsync();

                var cmtList = await _context.Comments
                    .Where(c => c.ParentType == (byte)FeedbackableType.Posting && c.ParentId == posting.Id)
                    .Select(c => c.Id)
                    .ToListAsync();

                async Task<int> cmtCntCalc(long id)
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
            _logger.LogInformation($"postings ranking score evaluation finished - {sw.ElapsedMilliseconds}ms {sw.ElapsedMilliseconds / postings.Count}msPerEntry");

            sw.Restart();
            var products = await _context.Products
                .Include(p => p.Postings)
                .ThenInclude(p => p.PostingNodes.OrderBy(pn => pn.OrderIndex))
                .ToListAsync();

            foreach (var product in products)
            {
                int recipes = 0;
                int recipeLikes = 0;
                int reviews = 0;
                double reviewSum = 0;

                foreach (var posting in product.Postings)
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

                var viewCnt = await _context.Views
                    .Where(v => v.ParentType == (byte)FeedbackableType.Product && v.ParentId == product.Id)
                    .CountAsync();

                var likeCnt = await _context.Likes
                    .Where(l => l.ParentType == (byte)FeedbackableType.Product && l.ParentId == product.Id)
                    .CountAsync();

                product.AlltimeScore = likeCnt * 0.5 + recipeLikes * 0.2 + (reviews + recipes) * 0.2 + viewCnt * 0.1;
            }
            sw.Stop();
            _logger.LogInformation($"products ranking score evaluation finished - {sw.ElapsedMilliseconds}ms {sw.ElapsedMilliseconds / products.Count}msPerEntry");

            sw.Restart();
            await _context.SaveChangesAsync();
            sw.Stop();
            _logger.LogInformation($"saving changes - {(double)sw.ElapsedMilliseconds / 1000} sec");

            return Ok();
        }

        [HttpGet("search")]
        public async Task<ActionResult<object>> SearchContents([FromQuery] string search)
        {
            var ftsQueryStr = search;
            // 상품 검색 - 편의점명
            var result1 = await _context.Stores
                .Where(s => EF.Functions.Contains(s.Name, ftsQueryStr))
                .Select(s => s.Id)
                .ToArrayAsync();
            // 상품 검색 - 카테고리
            var result2 = await _context.Categories
                .Where(c => EF.Functions.Contains(c.Name, ftsQueryStr))
                .Select(c => c.Id)
                .ToArrayAsync();
            // 상품 검색 - 상품명
            var resultProducts = await _context.Products
                .Where(p => result1.Contains(p.StoreId) || result2.Contains(p.CategoryId) || EF.Functions.Contains(p.Name, ftsQueryStr) || EF.Functions.FreeText(p.Description, ftsQueryStr))
                .ToListAsync();
            // 포스팅 검색
            var result = await _context.PostingNodes
                .Where(pn => EF.Functions.FreeText(pn.Text, ftsQueryStr))
                .Select(pn => pn.PostingId)
                .ToArrayAsync();

            var resultList = new List<long>();
            foreach (var id in result)
                if (!resultList.Contains(id))
                    resultList.Add(id);

            var resultPostings = new List<Posting>();
            foreach (var id in resultList)
                resultPostings.Add(await _context.Postings
                    .Include(p => p.PostingNodes.OrderBy(pn => pn.OrderIndex))
                    .Include(p => p.Products)
                    .SingleAsync(p => p.Id == id));

            if (!resultProducts.Any() && !resultPostings.Any())
                return NotFound();

            return new { count = resultProducts.Count + resultPostings.Count, products = resultProducts, postings = resultPostings };
        }
    }
}
