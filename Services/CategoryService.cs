using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StockManager.Data;
using StockManager.ViewModels;

namespace StockManager.Services
{
    public class CategoryService
    {
        private readonly AppDbContext _context;
        private readonly CategoryPathService _categoryPathService;

        public CategoryService(AppDbContext context, CategoryPathService categoryPathService)
        {
            _context = context;
            _categoryPathService = categoryPathService;
        }       
        //Metodo que devuelve una lista de productos con su jerarquia de categorias para vistas de listado
        public async Task<List<ProductViewModel>> GetAllProductViewModelsAsync()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.StockMovements)
                .ToListAsync();
            var result = new List<ProductViewModel>();
            foreach (var p in products)
            {
                var path = await _categoryPathService.GetCategoryPathAsync(p.Category);
                result.Add(new ProductViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    CurrentStock = p.StockMovements.Sum(m => m.Quantity),
                    MinimumStock = p.MinimumStock,
                    CategoryId = p.CategoryId,
                    CategoryPath = path
                });
            }
            return result;
        }
        //Retorna un solo prducto con su jerarquia de categorias para vistas de detalle
        public async Task<ProductViewModel?> GetProductViewModelAsync(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category) //Solo incluye la categoría directa
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
                return null;
            // GetCategoryPathAsync se encarga de cargar recursivamente los padres
            var path = await _categoryPathService.GetCategoryPathAsync(product.Category);
            return new ProductViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                CurrentStock = product.CurrentStock,
                MinimumStock = product.MinimumStock,
                CategoryId = product.CategoryId,
                CategoryPath = path
            };
        }
        //Metodo para devolver la lista de categorías jerarquicas para dropdowns
        public async Task<List<SelectListItem>> GetCategorySelectListAsync(int? selectedId = null)
        {
            //Solo cargar las categorias basicas
            var categories = await _context.Categories
                .AsNoTracking()
                .ToListAsync();
            var list = new List<SelectListItem>();
            foreach (var c in categories)
            {
                // GetCategoryPathAsync se encarga de cargar la jerarquia completa recursivamente
                var path = await _categoryPathService.GetCategoryPathAsync(c);
                list.Add(new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = path,
                    Selected = (selectedId.HasValue && c.Id == selectedId.Value)
                });
            }
            return list.OrderBy(c => c.Text).ToList();
        }
        //Retorna un string con el nombre del producto y su jerarquia de categorias completa
        public async Task<string> GetProductFullName(Product product)
        {
            if (product == null)
                return string.Empty;
            var path = await _categoryPathService.GetCategoryPathAsync(product.Category);
            return string.IsNullOrEmpty(path)
                ? product.Name
                : $"{product.Name} ({path})";
        }
        //Sobrecarga para obtener el nombre completo del producto por su Id
        public async Task<string> GetProductFullName(int productId)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null)
                return string.Empty;
            var path = await _categoryPathService.GetCategoryPathAsync(new Category { Id = product.CategoryId });
            return string.IsNullOrEmpty(path)
                ? product.Name
                : $"{product.Name} ({path})";
        }
    }
}
