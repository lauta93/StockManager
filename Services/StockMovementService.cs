using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StockManager.Data;
using StockManager.Services;
using StockManager.ViewModels;

public class StockMovementService
{
    private readonly AppDbContext _context;
    private readonly CategoryService _categoryService;
    private readonly CategoryPathService _categoryPathService;

    public StockMovementService(AppDbContext context,CategoryService categoryService,
        CategoryPathService categoryPathService)
    {
        _context = context;
        _categoryService = categoryService;
        _categoryPathService = categoryPathService;
    }
    //Metodo que devuelve un objeto StockMovement con un producto ya cargado (asociado) 
    public async Task<StockMovement?> GetCreateViewModelAsync(int productId)
    {
        //Carga el producto asociado por id
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == productId);
        if (product == null) 
            return null;
        //Crea el movimiento de stock asociado al producto para ser llenado en la vista
        var movement = new StockMovement
        {
            ProductId = product.Id,
            Product = product, 
            Date = DateTime.Now
        };
        return movement;
    }
    //Metodo para agregar un movimiento de stock
    public async Task AddStockMovementAsync(StockMovement movement)
    {
        movement.Date = DateTime.Now;
        _context.Add(movement);
        await _context.SaveChangesAsync();
    }
    //Metodo que devuelve una lista de todos los productos para selectlist
    public async Task<List<SelectListItem>> GetProductSelectListAsync()
    {
        return await _context.Products
            .OrderBy(p => p.Name)
            .Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = _categoryService.GetProductFullName(p).Result.ToString()//muestra el nombre con sus categorias
            })
            .ToListAsync();
    }
    //Metodo para obtener los movimientos de stock con filtros opcionales
    public async Task<List<StockMovementViewModel>> GetStockMovementsAsync(
           int? productId = null,
           DateTime? from = null,
           DateTime? to = null) {
        var query = _context.StockMovements
            .Include(m => m.Product)
            .AsQueryable();
        // Filtrado
        if (productId.HasValue)
            query = query.Where(m => m.ProductId == productId.Value);
        if (from.HasValue)
            query = query.Where(m => m.Date >= from.Value);
        if (to.HasValue)
            query = query.Where(m => m.Date <= to.Value);
        var movements = await query
            .OrderByDescending(m => m.Date)
            .ToListAsync();
        var result = new List<StockMovementViewModel>();
        foreach (var m in movements)
        {
            var path = m.Product?.CategoryId != null ?
             await _categoryPathService.GetCategoryPathAsync(m.Product.CategoryId)
    :        string.Empty;
            result.Add(new StockMovementViewModel
            {
                Id = m.Id,
                ProductName = m.Product?.Name ?? string.Empty,
                CategoryPath = path,
                Quantity = m.Quantity,
                Date = m.Date,
                Note = m.Note
            });
        }
        return result;
    }
}