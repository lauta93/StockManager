using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StockManager.Data;
using StockManager.ViewModels;

namespace StockManager.Services
{
    public class CategoryService
    {
        private readonly AppDbContext _context;

        public CategoryService(AppDbContext context)
        {
            _context = context;
        }

        // Devuelve la jerarquía completa en string
        public async Task<string> GetCategoryPathAsync(Category? category)
        {
            if (category == null)
                return string.Empty;

            var names = new List<string>();
            var current = category;

            while (current != null)
            {
                names.Insert(0, current.Name);

                // Si el padre no está cargado, lo trae de la BD
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
                names.RemoveAt(0); //quita la raíz "Producto"

            return string.Join(" > ", names);
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

        //Devuelve un producto individual con su jerarquía
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
        public async Task<List<SelectListItem>> GetCategorySelectListAsync(int? selectedId = null)
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
                    Text = path,
                    Selected = (selectedId.HasValue && c.Id == selectedId.Value)
                });
            }

            return list.OrderBy(c => c.Text).ToList();
        }
        //Metodo para cargar la jerarquia de una categoria recursivamente
        public async Task<Category?> LoadFullCategoryHierarchyAsync(Category category)
        {
            var current = category;
            while (current.ParentCategoryId != null)
            {
                current.ParentCategory = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Id == current.ParentCategoryId);
                current = current.ParentCategory!;
            }
            return category;
        }
        //Devuelve un string con el numbre del producto y su jerarquia de categorias completa, menos la raiz like always
        public async Task<string> GetProductFullName(Product product)
        {
            if (product == null)
                return string.Empty;

            var path = await GetCategoryPathAsync(product.Category);
            return string.IsNullOrEmpty(path)
                ? product.Name
                : $"{product.Name} ({path})";
        }

    }
}
