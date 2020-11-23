﻿using ConvAppServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConvAppServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly MainContext _context;
        private readonly ILogger _logger;

        public ProductsController(MainContext context, ILogger<ProductsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            _logger.LogInformation($"Get a product {id}");

            try
            {
                var product = await _context.Products
                .Where(p => p.Id == id)
                .Include(p => p.Postings)
                .AsSplitQuery()
                .SingleAsync();

                return product;
            }
            catch
            {
                return NotFound();
            }
        }

        [HttpGet]
        public async Task<ActionResult<List<Product>>> GetProducts([FromQuery] int? store = null, [FromQuery] int? category = null)
        {
            _logger.LogInformation($"Get Products store{store} cat{category}");

            var products = await _context.Products
                .Where(p => (store != null ? p.StoreId == store : true) && (category != null ? p.CategoryId == category : true))
                .OrderByDescending(p => p.CreatedDate)
                .Include(p => p.Postings)
                .AsSplitQuery()
                .ToListAsync();

            if (products == null)
            {
                return NotFound();
            }

            return products;
        }

        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProduct", new { id = product.Id }, product);
        }
    }
}