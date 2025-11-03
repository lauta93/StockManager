using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StockManager.Data;
using StockManager.Models;

namespace StockManager.Services
{
    public class CategoryService
    {
        private readonly AppDbContext _context;

        public CategoryService(AppDbContext context)
        {
            _context = context;
        }

        // Devuelve la jerarquía completa (asincrónica)
        public async Task<string> GetCategoryPathAsync(Category? category)
        {
            if (category == null)
                return string.Empty;

            var names = new List<string>();
            var current = category;

            while (current != null)
            {
                names.Insert(0, current.Name);

                // Si el padre no está cargado, lo traemos de la BD
                if (current.ParentCategory == null && current.ParentCategoryId.HasValue)
                {
                    current = await _context.Categories
                        .FirstOrDefaultAsync(c => c.Id == current.ParentCategoryId);
                }
                else
                {
                    current = current.ParentCategory;
                }
            }

            if (names.Count > 1)
                names.RemoveAt(0); // quita la raíz (por ejemplo "Producto")

            return string.Join(" > ", names);
        }

        // Devuelve todos los productos con sus categorías jerárquicas
        public async Task<List<ProductViewModel>> GetAllProductViewModelsAsync()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.StockMovements)
                .ToListAsync();

            var result = new List<ProductViewModel>();

            foreach (var p in products)
            {
                var path = await GetCategoryPathAsync(p.Category);

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

        // Devuelve un producto individual con su jerarquía
        public async Task<ProductViewModel?> GetProductViewModelAsync(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .ThenInclude(c => c.ParentCategory)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null)
                return null;

            var path = await GetCategoryPathAsync(product.Category);

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

        // Devuelve la lista de categorías jerárquicas para dropdowns
        public async Task<List<SelectListItem>> GetCategorySelectListAsync()
        {
            var categories = await _context.Categories
                .Include(c => c.ParentCategory)
                .AsNoTracking()
                .ToListAsync();

            var list = new List<SelectListItem>();

            foreach (var c in categories)
            {
                var path = await GetCategoryPathAsync(c);

                list.Add(new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = path
                });
            }

            return list.OrderBy(c => c.Text).ToList();
        }
    }
}
