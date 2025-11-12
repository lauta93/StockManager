using Microsoft.EntityFrameworkCore;
using StockManager.Data;
using StockManager.ViewModels;

namespace StockManager.Services
{
    public class DashboardService
    {
        private readonly AppDbContext _context;
        private readonly CategoryPathService _categoryPathService;
        public DashboardService(AppDbContext context, CategoryPathService categoryPathService)
        {
            _context = context;
            _categoryPathService = categoryPathService;
        }
        //trae los productos mas vendidos (egresos)
        public async Task<List<ProductSalesData>> GetTopSellingProductsAsync(int topCount = 10)
        {
            var topProducts = await _context.StockMovements
                .Where(m => m.Quantity < 0)
                .GroupBy(m => new { m.ProductId, m.Product.Name, m.Product.CategoryId })
                .Select(g => new
                {
                    ProductName = g.Key.Name,
                    CategoryId = g.Key.CategoryId,
                    TotalSold = Math.Abs(g.Sum(m => m.Quantity))
                })
                .OrderByDescending(p => p.TotalSold)
                .Take(topCount)
                .ToListAsync();
            var result = new List<ProductSalesData>();
            foreach (var product in topProducts)
            {
                var categoryPath = await _categoryPathService.GetShortCategoryPathAsync(product.CategoryId, 2);
                result.Add(new ProductSalesData
                {
                    ProductName = product.ProductName,
                    TotalSold = product.TotalSold,
                    CategoryPath = categoryPath
                });
            }
            return result;
        }
        //trae los productos con stock bajo
        public async Task<List<LowStockProduct>> GetLowStockProductsAsync(int threshold = 5)
        {
            var lowStockProducts = await _context.Products
                .Include(p => p.StockMovements)
                .Where(p => p.StockMovements.Sum(m => m.Quantity) < p.MinimumStock)
                .Select(p => new
                {
                    ProductName = p.Name,
                    CurrentStock = p.StockMovements.Sum(m => m.Quantity),
                    MinimumStock = p.MinimumStock,
                    CategoryId = p.CategoryId,
                    Status = p.StockMovements.Sum(m => m.Quantity) < 0 ? "Crítico" : "Bajo"
                })
                .OrderBy(p => p.CurrentStock)
                .Take(10)
                .ToListAsync();
            var result = new List<LowStockProduct>();
            foreach (var product in lowStockProducts)
            {
                var categoryPath = await _categoryPathService.GetShortCategoryPathAsync(product.CategoryId, 2);
                result.Add(new LowStockProduct
                {
                    ProductName = product.ProductName,
                    CurrentStock = product.CurrentStock,
                    MinimumStock = product.MinimumStock,
                    Status = product.Status,
                    CategoryPath = categoryPath
                });
            }
            return result;
        }
        //Resumen general
        public async Task<DashboardSummary> GetDashboardSummaryAsync()
        {
            var totalProducts = await _context.Products.CountAsync();
            var totalCategories = await _context.Categories.CountAsync();
            var totalMovements = await _context.StockMovements.CountAsync();
            var criticalStock = await _context.Products
                .Include(p => p.StockMovements)
                .CountAsync(p => p.StockMovements.Sum(m => m.Quantity) < 0);
            var lowStock = await GetLowStockProductsCountAsync();
            return new DashboardSummary
            {
                TotalProducts = totalProducts,
                TotalCategories = totalCategories,
                TotalMovements = totalMovements,
                CriticalStockCount = criticalStock,
                LowStockCount = lowStock
            };
        }
        //metodo para contar los productos con stock actual menor al stock minimo
        public async Task<int> GetLowStockProductsCountAsync()
        {
            return await _context.Products
        .Include(p => p.StockMovements)
        .CountAsync(p => p.StockMovements.Sum(m => m.Quantity) < p.MinimumStock &&
                   p.StockMovements.Sum(m => m.Quantity) >= 0);//trae los productos con stock bajo pero no negativo (pedido)
        }
    }
}