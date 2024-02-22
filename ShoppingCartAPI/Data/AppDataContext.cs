using Microsoft.EntityFrameworkCore;
using ShoppingCartLibrary;

namespace ShoppingCartAPI.Data;

public class AppDataContext : DbContext
{
    public AppDataContext(DbContextOptions<AppDataContext> options)
    : base(options) 
    { }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
}