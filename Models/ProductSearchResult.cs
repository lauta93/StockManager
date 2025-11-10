namespace StockManager.Models
{
    //Data Transfer Object para el buscador de productos
    public class ProductSearchResult
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DisplayText { get; set; } = string.Empty;
        public int? CategoryId { get; set; }
    }
}
