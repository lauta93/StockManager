using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StockManager.Data;
using StockManager.Models;

namespace StockManager.Controllers
{
    public class StockMovementsController : Controller
    {
        private readonly AppDbContext _context;

        public StockMovementsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: StockMovements/Create
        //public IActionResult Create()
        //{
        //    ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name");
        //    return View();
        //}
        public IActionResult Create(int productId)
        {
            var product = _context.Products.Find(productId);
            if (product == null) return NotFound();

            ViewBag.ProductId = product.Id;
            ViewBag.ProductName = product.Name;

            var movement = new StockMovement
            {
                ProductId = product.Id
            };

            return View(movement);
        }

        // POST: StockMovements/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductId,Quantity,Note")] StockMovement movement)
        {
            if (ModelState.IsValid)
            {
                movement.Date = DateTime.Now;
                _context.Add(movement);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Products");
            }

            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name", movement.ProductId);
            return View(movement);
        }
    }
}