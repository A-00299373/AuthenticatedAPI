using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingCartAPI.Data;
using ShoppingCartLibrary;

namespace ShoppingCartAPI.Controllers;

[Authorize]
[Route("[controller]")]
[ApiController]
    public class ShoppingCartController : ControllerBase
    {
        private readonly AppDataContext _context;

        public ShoppingCartController(AppDataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProductsInUserCart()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var shoppingCart = await _context.ShoppingCarts
                .Include(sc => sc.Products)
                .FirstOrDefaultAsync(sc => sc.User == userEmail);

            if (shoppingCart == null)
            {
                return NotFound("Shopping cart not found for the current user.");
            }

            return Ok(shoppingCart.Products);
        }

        [HttpPost("RemoveItem/{productId}")]
        public async Task<IActionResult> RemoveItemFromCart(int productId)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var shoppingCart = await _context.ShoppingCarts
                .Include(sc => sc.Products)
                .FirstOrDefaultAsync(sc => sc.User == userEmail);

            if (shoppingCart == null)
            {
                return NotFound("Shopping cart not found for the current user.");
            }

            var productToRemove = shoppingCart.Products.FirstOrDefault(p => p.Id == productId);
            if (productToRemove != null)
            {
                shoppingCart.Products.Remove(productToRemove);
                await _context.SaveChangesAsync();
                return Ok("Item removed from the shopping cart.");
            }

            return NotFound("Product not found in the shopping cart.");
        }

        [HttpPost("AddItem/{productId}")]
        public async Task<IActionResult> AddItemToCart(int productId)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var shoppingCart = await _context.ShoppingCarts
                .Include(sc => sc.Products)
                .FirstOrDefaultAsync(sc => sc.User == userEmail);

            if (shoppingCart == null)
            {
                shoppingCart = new ShoppingCart
                {
                    User = userEmail,
                    Products = new List<Product>()
                };
                _context.ShoppingCarts.Add(shoppingCart);
            }

            var productToAdd = await _context.Products.FindAsync(productId);
            if (productToAdd == null)
            {
                return NotFound("Product not found.");
            }

            shoppingCart.Products.Add(productToAdd);
            await _context.SaveChangesAsync();
            return Ok("Item added to the shopping cart.");
        }
    }