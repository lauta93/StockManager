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
        //Helper para llenar un viewmodel con su jerarquia de categorias
        private async Task<ProductViewModel> CreateProductViewModelAsync(Product product)
        {
            var path = await _categoryPathService.GetCategoryPathAsync(product.CategoryId);
            return new ProductViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                CurrentStock = product.StockMovements?.Sum(m => m.Quantity) ?? 0,
                MinimumStock = product.MinimumStock,
                CategoryId = product.CategoryId,
                CategoryPath = path
            };
        }
        //Metodo que devuelve una lista de viewmodels con su jerarquia de categorias
        public async Task<List<ProductViewModel>> GetAllProductViewModelsAsync()
        {
            var products = await _context.Products
                .Include(p => p.StockMovements)
                .ToListAsync();
            var result = new List<ProductViewModel>();
            foreach (var product in products)
            {
                result.Add(await CreateProductViewModelAsync(product));
            }
            return result;
        }
        //Metodo que devuelve un viewmodel de un producto en particular con su jerarquia
        public async Task<ProductViewModel?> GetProductViewModelAsync(int id)
        {
            var product = await _context.Products
                .Include(p => p.StockMovements)
                .FirstOrDefaultAsync(p => p.Id == id);
            return product != null ? await CreateProductViewModelAsync(product) : null;
        }
        //Metodo para devolver la lista de categorias jerarquicas para dropdowns
        public async Task<List<SelectListItem>> GetCategorySelectListAsync(int? selectedId = null)
        {
            var categories = await _context.Categories
                .AsNoTracking()
                .ToListAsync();
            var list = new List<SelectListItem>();
            foreach (var category in categories)
            {
                var path = await _categoryPathService.GetCategoryPathAsync(category.Id);
                list.Add(new SelectListItem
                {
                    Value = category.Id.ToString(),
                    Text = path,
                    Selected = (selectedId.HasValue && category.Id == selectedId.Value)
                });
            }
            return list.OrderBy(c => c.Text).ToList();
        }
        //Metodo para obtener un string con el nombre del producto + jerarquia
        public async Task<string> GetProductFullName(Product product)
        {
            if (product == null) return string.Empty;
            var path = await _categoryPathService.GetCategoryPathAsync(product.CategoryId);
            return string.IsNullOrEmpty(path) ? product.Name : $"{product.Name} ({path})";
        }
        //Sobrecarga del metodo anterior pero recibiendo el id
        public async Task<string> GetProductFullName(int productId)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null) return string.Empty;
            return await GetProductFullName(product);
        }
        //Metodo para obtener un diccionario de ids y rutas de categorias para el index del controlador
        public async Task<Dictionary<int, string>> GetCategoryPathsDictionaryAsync()
        {
            var selectList = await GetCategorySelectListAsync();
            return selectList.ToDictionary(
                item => int.Parse(item.Value),
                item => item.Text
            );
        }
    }
}
