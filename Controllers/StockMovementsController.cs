using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using StockManager.Data;
using StockManager.Services; 
using StockManager.ViewModels;
using System.Threading.Tasks;

public class StockMovementsController : Controller
{
    private readonly AppDbContext _context;
    private readonly CategoryService _categoryService;
    private readonly StockMovementService _stockMovementService;    
    
    public StockMovementsController( AppDbContext context, CategoryService categoryService,
        StockMovementService stockMovementService)
    {
        _context = context;
        _categoryService = categoryService;
        _stockMovementService = stockMovementService;
    }
    public async Task<IActionResult> Index(int? productId, DateTime? from, DateTime? to)
    {
        ViewBag.Products = await _stockMovementService.GetProductSelectListAsync();
        ViewBag.From = from?.ToString("yyyy-MM-dd");
        ViewBag.To = to?.ToString("yyyy-MM-dd");
        var movements = await _stockMovementService.GetStockMovementsAsync(productId, from, to);
        return View(movements);
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