using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using StockManager.Data;
using StockManager.Extensions;
using StockManager.Services; 
using StockManager.ViewModels;
using System.Threading.Tasks;

public class StockMovementsController : Controller
{
    private readonly AppDbContext _context;
    private readonly CategoryService _categoryService;
    private readonly StockMovementService _stockMovementService;
    private readonly ProductSearchService _productSearchService;

    public StockMovementsController(AppDbContext context, CategoryService categoryService,
        StockMovementService stockMovementService, ProductSearchService productSearchService)
    {
        _context = context;
        _categoryService = categoryService;
        _stockMovementService = stockMovementService;
        _productSearchService = productSearchService;
    }
    public async Task<IActionResult> Index(int? productId, string productSearch, DateTime? from, DateTime? to, int page = 1, int pageSize = 10)
    {
        var movements = await _stockMovementService.GetStockMovementsAsync(productId, productSearch, from, to);
        var pagedMovements = movements.ToPagedList(page, pageSize);
        ViewBag.From = from?.ToString("yyyy-MM-dd");
        ViewBag.To = to?.ToString("yyyy-MM-dd");
        ViewBag.CurrentProductId = productId;
        ViewBag.ProductSearch = productSearch;
        return View(pagedMovements);
    }
    // Endpoint para autocompletado
    [HttpGet]
    public async Task<JsonResult> SearchProducts(string term)
    {
        var results = await _productSearchService.SearchProductsAsync(term);
        return Json(results.Select(r => new { id = r.Id, text = r.DisplayText }));
    }
    // GET: StockMovements/Create
    public async Task<IActionResult> Create(int productId)
    {
        var movement = await _stockMovementService.GetCreateViewModelAsync(productId);
        if(movement == null) 
            return NotFound();
        ViewBag.ProductFullName = await _categoryService.GetProductFullName(productId);
        return View(movement);
    }
    // POST: /StockMovements/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StockMovement model)
    {
        if (!ModelState.IsValid)
            return View(model);
        await _stockMovementService.AddStockMovementAsync(model);
        return RedirectToAction("Index", "Products");
    }
}