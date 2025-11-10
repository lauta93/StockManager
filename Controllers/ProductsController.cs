using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StockManager.Data;
using StockManager.Extensions;
using StockManager.Services;
using StockManager.ViewModels;

namespace StockManager.Controllers
{
    public class ProductsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly CategoryService _categoryService;
        private readonly ProductSearchService _productSearchService;

        public ProductsController(AppDbContext context, CategoryService categoryService, 
               ProductSearchService productSearchService)
        {
            _context = context;
            _categoryService = categoryService;
            _productSearchService = productSearchService;
        }
        // GET: Products
        public async Task<IActionResult> Index(string searchTerm, int? categoryId, int page = 1, int pageSize = 10)
        {
            var products = await _categoryService.GetAllProductViewModelsAsync();
            //Aplica filtros, trayendo todos los prod de las subcategorias
            if (categoryId.HasValue)
            {
                var allCategoryIds = await _categoryService.GetAllSubcategoryIdsAsync(categoryId.Value);
                products = products.Where(p => allCategoryIds.Contains(p.CategoryId)).ToList();
            }
            if (!string.IsNullOrEmpty(searchTerm))
            {
                var termLower = searchTerm.ToLower();
                products = products.Where(p => p.Name.ToLower().Contains(termLower) || p.Id.ToString().Contains(searchTerm)).ToList();
            }
            ViewBag.SearchTerm = searchTerm;
            ViewBag.CategoryId = categoryId;
            ViewBag.Categories = await _categoryService.GetCategorySelectListAsync(categoryId);
            return View(products.ToPagedList(page, pageSize));
        }
        // Endpoint para autocompletado
        [HttpGet]
        public async Task<JsonResult> SearchProducts(string term)
        {
            var results = await _productSearchService.SearchProductsAsync(term);
            return Json(results.Select(r => new { id = r.Id, text = r.DisplayText }));
        }
        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();
            var viewModel = await _categoryService.GetProductViewModelAsync(id.Value);
            if (viewModel == null)
                return NotFound();
            return View(viewModel);
        }
        // GET: Products/Create
        public async Task<IActionResult> Create()
        {
            ViewData["CategoryId"] = await _categoryService.GetCategorySelectListAsync();
            return View();
        }
        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Price,Stock,MinimumStock,CategoryId")] Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = await _categoryService.GetCategorySelectListAsync();
            return View(product);
        }
        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();
            var viewModel = await _categoryService.GetProductViewModelAsync(id.Value);
            if (viewModel == null)
                return NotFound();
            ViewData["CategoryId"] = await _categoryService.GetCategorySelectListAsync(viewModel.CategoryId); 
            return View(viewModel);
        }
        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Price,Stock,MinimumStock,CategoryId")] Product product)
        {
            if (id != product.Id)
                return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = await _categoryService.GetCategorySelectListAsync();
            return View(product);
        }
        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();
            var viewModel = await _categoryService.GetProductViewModelAsync(id.Value);
            if (viewModel == null)
                return NotFound();
            return View(viewModel);
        }
        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}