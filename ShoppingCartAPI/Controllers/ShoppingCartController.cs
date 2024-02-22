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
        public async Task<ActionResult<IEnumerable<ShoppingCart>>> GetShoppingCarts()
        {
            // Retrieve all shopping carts from the database
            var shoppingCarts = await _context.ShoppingCarts.ToListAsync();
            return Ok(shoppingCarts);
        }

        [HttpGet("{user}")]
        public async Task<ActionResult<ShoppingCart>> GetUserShoppingCart(string user)
        {
            // Retrieve the shopping cart for the specified user from the database
            var shoppingCart = await _context.ShoppingCarts
                .Include(sc => sc.Products)
                .FirstOrDefaultAsync(sc => sc.User == user);

            if (shoppingCart == null)
            {
                return NotFound();
            }

            return shoppingCart;
        }

        [HttpPost("RemoveItem/{cartId}/{productId}")]
        public async Task<IActionResult> RemoveItem(int cartId, int productId)
        {
            // Retrieve the shopping cart from the database
            var shoppingCart = await _context.ShoppingCarts
                .Include(sc => sc.Products)
                .FirstOrDefaultAsync(sc => sc.Id == cartId);

            if (shoppingCart == null)
            {
                return NotFound();
            }

            // Remove the product from the shopping cart
            var productToRemove = shoppingCart.Products.FirstOrDefault(p => p.Id == productId);
            if (productToRemove != null)
            {
                shoppingCart.Products.Remove(productToRemove);
                await _context.SaveChangesAsync();
            }

            return Ok();
        }

        [HttpPost("AddItem/{cartId}/{productId}")]
        public async Task<IActionResult> AddItem(int cartId, int productId)
        {
            // Retrieve the shopping cart from the database
            var shoppingCart = await _context.ShoppingCarts
                .Include(sc => sc.Products)
                .FirstOrDefaultAsync(sc => sc.Id == cartId);

            if (shoppingCart == null)
            {
                return NotFound();
            }

            // Retrieve the product from the database
            var productToAdd = await _context.Products.FindAsync(productId);
            if (productToAdd == null)
            {
                return NotFound("Product not found");
            }

            // Add the product to the shopping cart
            shoppingCart.Products.Add(productToAdd);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }