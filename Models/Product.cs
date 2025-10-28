namespace StockManager.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int MinimumStock { get; set; }
        //Clave foranea de la categoria
        public int CategoryId { get; set; }
        //Crea la relacion con la categoria (N:1), muchos productos pueden pertenecer a una categoria
        public Category? Category { get; set; }
    }
}
