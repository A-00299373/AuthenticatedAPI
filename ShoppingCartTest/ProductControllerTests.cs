using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ShoppingCartAPI.Controllers;
using ShoppingCartAPI.Data;
using ShoppingCartLibrary;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ShoppingCartTest;

[TestClass]
public class ProductControllerTests
{
    public class MockAppDataContext : AppDataContext
    {
        public MockAppDataContext(DbContextOptions<AppDataContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
    }

    [TestMethod]
    public async Task GetAllProducts_ReturnsAllProducts()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDataContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;
        using (var dbContext = new MockAppDataContext(options))
        {

            var products = new List<Product>
                {
                    new Product { Id = 1, Name = "Product 1", Price = 1, Description = "hello1"},
                    new Product { Id = 2, Name = "Product 2", Price = 2, Description = "hello2" },
                    new Product { Id = 3, Name = "Product 3", Price = 3, Description = "hello3" }
                };

            dbContext.Products.AddRange(products);
            await dbContext.SaveChangesAsync();

            var controller = new ProductController(dbContext);
            // Act
            var result = await controller.GetAllProducts();

            // Assert
            var okResult = result.Result as OkObjectResult;
            var returnedProducts = okResult?.Value as List<Product>; // Use null-conditional operator
            Assert.IsNotNull(returnedProducts);
            Assert.AreEqual(products.Count, returnedProducts.Count);
            CollectionAssert.AreEqual(products, returnedProducts);
        }
    }

    [TestMethod]
    public async Task GetProductsByCategory_ReturnsProductsForGivenCategory()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDataContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;
        using (var dbContext = new MockAppDataContext(options))
        {
            var categoryId = 1; // Specify a category ID
            var products = new List<Product>
        {
            new Product { Id = 1, Name = "Product 1", Price = 1, Description = "hello1", Category = new Category { Id = categoryId } },
            new Product { Id = 2, Name = "Product 2", Price = 2, Description = "hello2", Category = new Category { Id = categoryId } },
            new Product { Id = 3, Name = "Product 3", Price = 3, Description = "hello3", Category = new Category { Id = categoryId } }
        };

            dbContext.Products.AddRange(products);
            await dbContext.SaveChangesAsync();

            var controller = new ProductController(dbContext);

            // Act
            var result = await controller.GetProductsByCategory(categoryId);

            // Assert
            var okResult = result.Result as OkObjectResult;
            var returnedProducts = okResult?.Value as List<Product>;
            Assert.IsNotNull(returnedProducts);
            Assert.AreEqual(products.Count, returnedProducts.Count);
            CollectionAssert.AreEqual(products, returnedProducts);
        }
    }

    [TestMethod]
    public async Task GetProductsByCategory_ReturnsNotFoundForNonExistentCategory()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDataContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;
        using (var dbContext = new MockAppDataContext(options))
        {
            var categoryId = 100; // Specify a category ID that doesn't exist
            var controller = new ProductController(dbContext);

            // Act
            var result = await controller.GetProductsByCategory(categoryId);

            // Assert
            var notFoundResult = result.Result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual("No products found for this category", notFoundResult.Value);
        }
    }

    [TestMethod]
    public async Task CreateProduct_ReturnsCreatedAtAction()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDataContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;
        using (var dbContext = new MockAppDataContext(options))
        {
            var productToCreate = new Product { Id = 4, Name = "New Product", Price = 10, Description = "New Description" };
            var controller = new ProductController(dbContext);

            // Act
            var result = await controller.CreateProduct(productToCreate);

            // Assert
            var createdAtActionResult = result.Result as CreatedAtActionResult;
            Assert.IsNotNull(createdAtActionResult);
            Assert.AreEqual("GetAllProducts", createdAtActionResult.ActionName);
            Assert.AreEqual(productToCreate.Id, createdAtActionResult.RouteValues["id"]);

            // You might want to further verify that the product is actually created in the database
        }
    }
}

[TestClass]
    public class ShoppingCartControllerTests
    {
        private ShoppingCartController _controller1;
        private AppDataContext _context1;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDataContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;
            _context1 = new AppDataContext(options);

            _controller1 = new ShoppingCartController(_context1);

            // Mock the User property of the controller's HttpContext
            var userClaims = new List<Claim> { new Claim(ClaimTypes.Email, "test@example.com") };
            var identity = new ClaimsIdentity(userClaims, "TestAuthType");
            var userPrincipal = new ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext { User = userPrincipal };
            _controller1.ControllerContext = new ControllerContext { HttpContext = httpContext };
        }

        [TestCleanup]
        public void TearDown()
        {
            _context1.Dispose();
        }

        [TestMethod]
        public async Task GetProductsInUserCart_WhenShoppingCartExists_ReturnsOkWithProducts()
        {
            // Arrange
            var shoppingCart = new ShoppingCart
            {
                User = "test@example.com",
                Products = new List<Product> { new Product { Id = 1, Name = "Product 1" } }
            };
            _context1.ShoppingCarts.Add(shoppingCart);
            _context1.SaveChanges();

            // Act
            var result = await _controller1.GetProductsInUserCart();

            // Assert
            var okResult = result.Result as OkObjectResult;
            var products = okResult?.Value as List<Product>;
            Assert.IsNotNull(products);
            Assert.AreEqual(1, products.Count);
        }

        [TestMethod]
        public async Task RemoveItemFromCart_WhenProductExistsInCart_ReturnsOk()
        {
            // Arrange
            var product = new Product { Id = 1, Name = "Product 1" };
            var shoppingCart = new ShoppingCart
            {
                User = "test@example.com",
                Products = new List<Product> { product }
            };
            _context1.ShoppingCarts.Add(shoppingCart);
            _context1.SaveChanges();

            // Act
            var result = await _controller1.RemoveItemFromCart(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        [TestMethod]
        public async Task AddItemToCart_WhenProductExists_ReturnsOk()
        {
            // Arrange
            var product = new Product { Id = 1, Name = "Product 1" };
            _context1.Products.Add(product);
            _context1.SaveChanges();

            // Act
            var result = await _controller1.AddItemToCart(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        [TestMethod]
        public async Task AddItemToCart_WhenProductNotFound_ReturnsNotFound()
        {
            // Arrange - no need to add the product since it should not exist

            // Act
            var result = await _controller1.AddItemToCart(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
        }
    }