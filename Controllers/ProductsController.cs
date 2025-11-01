using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StockManager.Data;
using StockManager.Models;

namespace StockManager.Controllers
{
    public class ProductsController : Controller
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        //Metodo para obtener todos los productos con su jerarquia de catergorias
        private async Task<List<ProductViewModel>> GetAllProductViewModelsAsync()
        {
            
            var products = await _context.Products
                .Include(p => p.Category)
                    .ThenInclude(c => c.ParentCategory)
                .ToListAsync();

            return products.Select(p => new ProductViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Stock = p.Stock,
                MinimumStock = p.MinimumStock,
                CategoryId = p.CategoryId,
                CategoryPath = GetCategoryPath(p.Category)
            }).ToList();
        }
        //Metodo para obtener un producto con su jerarquia de categorias
        private async Task<ProductViewModel?> GetProductViewModelAsync(int id)
        {            
            var product = await _context.Products
                .Include(p => p.Category)
                .ThenInclude(c => c.ParentCategory)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null)
                return null;

            return new ProductViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Stock = product.Stock,
                MinimumStock = product.MinimumStock,
                CategoryId = product.CategoryId,
                CategoryPath = GetCategoryPath(product.Category)
            };
        }

        //Metodo para obtener la ruta de categorias
        private string GetCategoryPath(Category category)
        {
            if (category == null) return string.Empty;
            var names = new List<string>();
            var current = category;
            while (current != null && current.ParentCategoryId != null)
            {
                names.Insert(0, current.Name);                
                current = _context.Categories
                    .AsNoTracking()
                    .FirstOrDefault(c => c.Id == current.ParentCategoryId);
            }
            return string.Join(" > ", names);
        }
        //Metodo para obtener la lista de categorias para un dropdown
        private List<SelectListItem> GetCategorySelectList()
        {
            var categories = _context.Categories
                .Include(c => c.ParentCategory)
                .AsNoTracking()
                .ToList();

            var list = categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = GetCategoryPath(c)
            })
            .OrderBy(c => c.Text)
            .ToList();
            return list;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {            
            var products = await GetAllProductViewModelsAsync();
            return View(products);
        }
        

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();
            var viewModel = await GetProductViewModelAsync(id.Value);
            if (viewModel == null)
                return NotFound();
            return View(viewModel);
        }

        // GET: Products/Create
        public IActionResult Create()
        {            
            ViewData["CategoryId"] = GetCategorySelectList();
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

            ViewData["CategoryId"] = GetCategorySelectList();
            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var viewModel = await GetProductViewModelAsync(id.Value);
            if (viewModel == null)
                return NotFound();

            ViewData["CategoryId"] = new SelectList(
                GetCategorySelectList(), // reutilizamos el método
                "Value",
                "Text",
                viewModel.CategoryId.ToString()
            );

            return View(viewModel);
        }


        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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

            ViewData["CategoryId"] = GetCategorySelectList();
            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var viewModel = await GetProductViewModelAsync(id.Value);
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
