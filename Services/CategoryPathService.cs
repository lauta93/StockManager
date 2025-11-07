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
        public async Task<string> GetCategoryPathAsync(int categoryId)
        {
            if (categoryId <= 0) return string.Empty;
            var fullCategory = await LoadFullHierarchyFromDatabaseByIdAsync(categoryId);
            return BuildCategoryPathString(fullCategory);
        }
        //Sobrecarga del metodo principal para recibir como parametro un obj tipo Category
        public async Task<string> GetCategoryPathAsync(Category? category)
        {
            if (category == null) return string.Empty;
            return await GetCategoryPathAsync(category.Id);
        }
        //Metodo que devuelve una categoria con toda su jerarquia cargada en memoria
        private async Task<Category> LoadFullHierarchyFromDatabaseByIdAsync(int categoryId)
        {
            //carga la categoría específica
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == categoryId);
            if (category == null)
                return null;
            await LoadParentRecursivelyAsync(category);
            return category;
        }
        //Tarea recursiva que carga las categorias dentro de un obj Category
        private async Task LoadParentRecursivelyAsync(Category category)
        {
            if (category.ParentCategoryId.HasValue && category.ParentCategory == null)
            {
                //carga el padre desde la BD
                category.ParentCategory = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Id == category.ParentCategoryId.Value);
                //carga recursivamente sus padres
                if (category.ParentCategory != null)
                {
                    await LoadParentRecursivelyAsync(category.ParentCategory);
                }
            }
        }
        //Metodo que "desenvuelve" las jerarquias de un Category ya cargado y lo devuelve en un string formateado
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