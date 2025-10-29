namespace StockManager.Models
{
    using System.ComponentModel.DataAnnotations;
    public class Category
    {
        public int Id { get; set; }
        [Display(Name = "Nombre")]
        public string Name { get; set; } = string.Empty;

        //Clave foranea de la categoria padre        
        public int? ParentCategoryId { get; set; }
        //Crea la relacion de auto referencia
        [Display(Name = "Categoria padre")]
        public Category? ParentCategory { get; set; }
        //Crea la relacion (1:N) en la BD, una categoria puede tener muchas subcategorias de tipo Category
        public ICollection<Category> SubCategories { get; set; } = new List<Category>();
        //Crea la relacion con los productos (1:N), una categoria puede tener muchos productos
        public ICollection<Product> Products { get; set; } = new List<Product>();


    }
}
