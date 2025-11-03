using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StockManager.Data;
using StockManager.Models;
using StockManager.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockManager.Controllers
{
    public class ProductsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly CategoryService _categoryService;//Inyecta el CategoryService

        public ProductsController(AppDbContext context, CategoryService categoryService)
        {
            _context = context;
            _categoryService = categoryService;
        }        

        // GET: Products
        public async Task<IActionResult> Index()
        {            
            var products = await _categoryService.GetAllProductViewModelsAsync();
            return View(products);
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
        public IActionResult Create()
        {            
            ViewData["CategoryId"] = _categoryService.GetCategorySelectListAsync();
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

            ViewData["CategoryId"] = _categoryService.GetCategorySelectListAsync();
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

            ViewData["CategoryId"] = new SelectList(
                await _categoryService.GetCategorySelectListAsync(), // reutilizamos el método
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

            ViewData["CategoryId"] = _categoryService.GetCategorySelectListAsync();
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
