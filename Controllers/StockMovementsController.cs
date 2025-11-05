using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StockManager.Data;
using StockManager.ViewModels;
using StockManager.Services; 
using System.Threading.Tasks;

public class StockMovementsController : Controller
{
    private readonly StockMovementService _stockMovementService;
    private readonly AppDbContext _context;
    private readonly CategoryService _categoryService;


    public StockMovementsController(StockMovementService stockMovementService, AppDbContext context, CategoryService categoryService)
    {
        _stockMovementService = stockMovementService;
        _context = context;
        _categoryService = categoryService;
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
        var product = await _context.Products
            .Include(p => p.Category)
            .ThenInclude(c => c.ParentCategory)
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (product == null)
            return NotFound();

        ViewBag.ProductFullName = await _categoryService.GetProductFullName(product);

        var movement = new StockMovement
        {
            ProductId = productId,
            Product = product
        };

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