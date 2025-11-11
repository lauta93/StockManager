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
                    DisplayText = $"{p.Name}"
                })
                .ToListAsync();
            var termLower = term.ToLower();
            return allProducts
                .Where(p =>
                    p.Name.ToLower().Contains(termLower))
                .Take(10)
                .ToList();
        }
    }
}