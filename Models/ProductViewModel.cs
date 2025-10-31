namespace StockManager.Models
{
    public class ProductViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int MinimumStock { get; set; }
        public int CategoryId { get; set; }
        public string CategoryPath { get; set; }
    }
}
