using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StockManager.ViewModels
{
    public class Product
    {
        public int Id { get; set; }
        [Required, Display(Name = "Nombre del producto")]
        public string Name { get; set; } = string.Empty;
        [Required, DataType(DataType.Currency)]
        public decimal Price { get; set; }
        [Display(Name = "Stock mínimo")]
        public int MinimumStock { get; set; }
        //Clave foranea de la categoria
        [Display(Name = "Categoría")]
        public int CategoryId { get; set; }
        //Crea la relacion con la categoria (N:1), muchos productos pueden pertenecer a una categoria        
        public Category? Category { get; set; }
        //Crea la relacion con los movimientos de stock (1:N), un producto puede tener muchos movimientos de stock
        public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
        //Calcula el stock actual en base a los movimientos de stock
        [NotMapped]
        [Display(Name = "Stock actual")]
        public int CurrentStock => StockMovements?.Sum(m => m.Quantity) ?? 0;
    }
}
