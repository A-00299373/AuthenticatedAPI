using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingCartAPI.Data;
using ShoppingCartLibrary;


namespace ShoppingCartAPI.Controllers;

[Authorize] // Requires authorization for all actions in this controller
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
        var products = await _context.Products.ToListAsync();
        return Ok(products);
    }

    // GET: /Product/ByCategory/{categoryId}
    [HttpGet("ByCategory/{categoryId}")]
    public async Task<ActionResult<IEnumerable<Product>>> GetProductsByCategory(int categoryId)
    {
        // Retrieve the category from the database
        // 
         var products = await _context.Products
                .Where(p => p.Category.Id == categoryId)
                .ToListAsync();

            if (products == null || products.Count == 0)
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
                // Add the product to the database
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                
                // Return the newly created product with a 201 Created status code
                // return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
                        return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);

            }
            else
            {
                // If the product data is not valid, return a 400 Bad Request response
                return BadRequest(ModelState);
            }
        }

        // Helper method to retrieve a product by its ID
        private async Task<ActionResult<Product>> GetProductById(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return product;
        }
}

