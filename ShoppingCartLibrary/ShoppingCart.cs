namespace ShoppingCartLibrary;

public class ShoppingCart
    {
        public int Id { get; set; }
        public string User { get; set; }
        public List<Product> Products { get; set; }
    }