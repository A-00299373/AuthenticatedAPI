using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingCartAPI.Data;
using ShoppingCartLibrary;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace ShoppingCartAPI.Controllers;

    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly AppDataContext _context;

        public ProductController(AppDataContext context)
        {
            _context = context;
        }

        // GET: /Product
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetAllProducts()
        {
            var products = await _context.Products
            .Include(p => p.Category)
            .ToListAsync();

            return Ok(products);
        }

        // GET: /Product/ByCategory/{categoryId}
        [HttpGet("ByCategory/{categoryId}")]
        public async Task<ActionResult<IEnumerable<Product>>> GetProductsByCategory(int categoryId)
        {
            var products = await _context.Products
                .Where(p => p.Category.categoryid == categoryId)
                .Include(p => p.Category)
                .ToListAsync();

            if (products == null || !products.Any())
            {
                return NotFound("No products found for this category");
            }

            return Ok(products);
        }

        // POST: /Product
        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct(Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetAllProducts), new { id = product.Id }, product);
            }
            else
            {
                return BadRequest(ModelState);
            }
        }
    }