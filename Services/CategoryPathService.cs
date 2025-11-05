using Microsoft.EntityFrameworkCore;
using StockManager.Data;
using StockManager.ViewModels;

namespace StockManager.Services
{
    public class CategoryPathService
    {
        private readonly AppDbContext _context;

        public CategoryPathService(AppDbContext context)
        {
            _context = context;
        }
        //Metodo principal para obtener la ruta completa de categorias
        public async Task<string> GetCategoryPathAsync(Category? category)
        {
            if (category == null)
                return string.Empty;
            // SIEMPRE cargar la categoría fresca desde la BD
            var fullCategory = await LoadFullHierarchyFromDatabaseByIdAsync(category.Id);
            return BuildCategoryPathString(fullCategory);
        }
        //Metodo que devuelve la jerarquia completa de una categoria en particular
        private async Task<Category> LoadFullHierarchyFromDatabaseByIdAsync(int categoryId)
        {
            //carga la categoría específica
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == categoryId);
            if (category == null)
                return null;
            //carga recursivamente TODOS los padres
            await LoadParentRecursivelyAsync(category);
            return category;
        }
        private async Task LoadParentRecursivelyAsync(Category category)
        {
            if (category.ParentCategoryId.HasValue && category.ParentCategory == null)
            {
                //carga el padre desde la BD
                category.ParentCategory = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Id == category.ParentCategoryId.Value);
                //si se cargo el padre carga recursivamente su padre
                if (category.ParentCategory != null)
                {
                    await LoadParentRecursivelyAsync(category.ParentCategory);
                }
            }
        }
        //Metodo que devuelve la jerarquia en string de una categoria en espeficico
        private string BuildCategoryPathString(Category category)
        {
            var names = new List<string>();
            var current = category;
            while (current != null)
            {
                names.Insert(0, current.Name);
                current = current.ParentCategory;
            }
            if (names.Count > 1)
                names.RemoveAt(0); //quita la raíz "Producto"
            return string.Join(" > ", names);
        }
    }
}