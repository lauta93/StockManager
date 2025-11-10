using Microsoft.EntityFrameworkCore;
using StockManager.Data;
using StockManager.Models;

namespace StockManager.Services
{
    public class ProductSearchService
    {
        private readonly AppDbContext _context;
        public ProductSearchService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<List<ProductSearchResult>> SearchProductsAsync(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return new List<ProductSearchResult>();
            //Trae todos y filtra en memoria
            var allProducts = await _context.Products
                .Select(p => new ProductSearchResult
                {
                    Id = p.Id,
                    Name = p.Name,
                    DisplayText = $"{p.Name} (ID: {p.Id})"
                })
                .ToListAsync();
            var termLower = term.ToLower();
            return allProducts
                .Where(p =>
                    p.Name.ToLower().Contains(termLower) ||
                    p.Id.ToString().Contains(term))
                .Take(10)
                .ToList();
        }
        public async Task<List<ProductSearchResult>> SearchProductsAdvancedAsync(string term, int? categoryId = null)
        {
            var query = _context.Products.AsQueryable();
            if (!string.IsNullOrWhiteSpace(term))
            {
                query = query.Where(p => p.Name.Contains(term) || p.Id.ToString() == term);
            }
            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }
            return await query
                .Select(p => new ProductSearchResult
                {
                    Id = p.Id,
                    Name = p.Name,
                    DisplayText = $"{p.Name} (ID: {p.Id})",
                    CategoryId = p.CategoryId
                })
                .Take(20)
                .ToListAsync();
        }
    }
}