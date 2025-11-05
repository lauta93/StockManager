using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StockManager.Data;
using StockManager.ViewModels;
using StockManager.Services;

namespace StockManager.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly CategoryService _categoryService;
        private readonly CategoryPathService _categoryPathService;

        public CategoriesController(AppDbContext context, CategoryService categoryService, CategoryPathService categoryPathService)
        {
            _context = context;
            _categoryService = categoryService;
            _categoryPathService = categoryPathService;
        }

        // GET: Categories
        public async Task<IActionResult> Index()
        {
            //carga solo las categorias base
            var categories = await _context.Categories
                .AsNoTracking()
                .ToListAsync();
            var categoryPaths = new Dictionary<int, string>();
            foreach (var c in categories)
            {
                //Forza que GetCategoryPathAsync cargue la jerarquia completa desde BD
                categoryPaths[c.Id] = await _categoryPathService.GetCategoryPathAsync(c);
            }
            ViewBag.CategoryPaths = categoryPaths;
            return View(categories);
        }
        // GET: Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();
            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
                return NotFound();
           ViewBag.CategoryPath = await _categoryPathService.GetCategoryPathAsync(category);
            return View(category);
        }
        // GET: Categories/Create
        public async Task<IActionResult> Create()
        {
            ViewData["ParentCategoryId"] = await _categoryService.GetCategorySelectListAsync();
            return View();
        }
        // POST: Categories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,ParentCategoryId")] Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ParentCategoryId"] = await _categoryService.GetCategorySelectListAsync(category.ParentCategoryId);
            return View(category);
        }
        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound();            
            ViewData["ParentCategoryId"] = await _categoryService.GetCategorySelectListAsync(category.ParentCategoryId);
            return View(category);
        }
        // POST: Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,ParentCategoryId")] Category category)
        {
            if (id != category.Id)
                return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ParentCategoryId"] = await _categoryService.GetCategorySelectListAsync(category.ParentCategoryId);
            return View(category);
        }
        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();
            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
                return NotFound();
            ViewBag.CategoryPath = await _categoryPathService.GetCategoryPathAsync(category);
            return View(category);
        }
        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}
