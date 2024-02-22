namespace ShoppingCartLibrary;

public class Product
    {
        public int Id { get; set; }
        public decimal Price { get; set; }
        public string Name { get; set; } = String.Empty;
        public string Description { get; set; } 

        public Category Category { get; set; } 
    }