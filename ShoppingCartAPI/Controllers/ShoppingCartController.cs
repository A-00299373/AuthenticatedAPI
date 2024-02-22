using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingCartAPI.Data;
using ShoppingCartLibrary;

namespace ShoppingCartAPI.Controllers;

[Authorize] // Requires authorization for all actions in this controller
[Route("[controller]")]
[ApiController]
public class ShoppingCartController : ControllerBase
{
    private readonly AppDataContext _context;

    public ShoppingCartController(AppDataContext context)
    {
        _context = context;
    }

    // GET: api/ShoppingCart
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetShoppingCartItems()
    {
        // Get the current user's ID from the claims
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized(); // If user ID not found in claims, return 401 Unauthorized
        }

        // Retrieve the shopping cart items for the current user
        var shoppingCartItems = await _context.ShoppingCarts
            .Where(cart => cart.User == userId)
            .Include(cart => cart.Products) // Include products in the query
            .SelectMany(cart => cart.Products)
            .ToListAsync();

        return shoppingCartItems;
    }

    // POST: api/ShoppingCart/RemoveItem/{id}
    [HttpPost("RemoveItem/{id}")]
    public async Task<IActionResult> RemoveItemFromCart(int id)
    {
        // Get the current user's ID from the claims
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized(); // If user ID not found in claims, return 401 Unauthorized
        }

        // Retrieve the shopping cart for the current user
        var shoppingCart = await _context.ShoppingCarts
            .Include(cart => cart.Products)
            .FirstOrDefaultAsync(cart => cart.User == userId);

        if (shoppingCart == null)
        {
            return NotFound(); // If shopping cart not found for the user, return 404 Not Found
        }

        // Check if the product exists in the shopping cart
        var productToRemove = shoppingCart.Products.FirstOrDefault(p => p.Id == id);
        if (productToRemove == null)
        {
            return NotFound(); // If product with given ID not found in the shopping cart, return 404 Not Found
        }

        // Remove the product from the shopping cart
        shoppingCart.Products.Remove(productToRemove);
        await _context.SaveChangesAsync();

        return NoContent(); // Return 204 No Content on successful removal
    }

    // POST: api/ShoppingCart/AddItem/{id}
    [HttpPost("AddItem/{id}")]
    public async Task<IActionResult> AddItemToCart(int id)
    {
        // Get the current user's ID from the claims
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized(); // If user ID not found in claims, return 401 Unauthorized
        }

        // Retrieve or create the shopping cart for the current user
        var shoppingCart = await _context.ShoppingCarts
            .Include(cart => cart.Products)
            .FirstOrDefaultAsync(cart => cart.User == userId);

        if (shoppingCart == null)
        {
            // If shopping cart does not exist, create a new one
            shoppingCart = new ShoppingCart
            {
                User = userId,
                Products = new List<Product>()
            };
            _context.ShoppingCarts.Add(shoppingCart);
        }

        // Retrieve the product to be added to the shopping cart
        var productToAdd = await _context.Products.FindAsync(id);
        if (productToAdd == null)
        {
            return NotFound(); // If product with given ID not found, return 404 Not Found
        }

        // Add the product to the shopping cart
        shoppingCart.Products.Add(productToAdd);

        // Save changes to the database
        await _context.SaveChangesAsync();

        return Ok(); // Return 200 OK on successful addition
    }
}
