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
    public class CategoriesController : Controller
    {
        private readonly AppDbContext _context;

        public CategoriesController(AppDbContext context)
        {
            _context = context;
        }
        //Metodo que devuelve la ruta completa de una categoria
        private string GetCategoryPath(Category category)
        {
            var names = new List<string>();
            var current = category;

            while (current != null)
            {
                names.Insert(0, current.Name);
                current = current.ParentCategory;
            }
            if (names.Count > 1)
                names.RemoveAt(0); //Quita la raiz (Producto)
            return string.Join(" > ", names);
        }
        //Metodo para llenar el selectlist de categorias con su ruta completa
        private async Task PopulateParentCategoriesDropdown(int? selectedId = null)
        {
            var categories = await _context.Categories
                .Include(c => c.ParentCategory)
                .ToListAsync();

            var categoryPaths = categories.Select(c => new
            {
                Id = c.Id,
                Path = GetCategoryPath(c)
            })
            .OrderBy(c => c.Path)
            .ToList();

            ViewData["ParentCategoryId"] = new SelectList(categoryPaths, "Id", "Path", selectedId);
        }
        //Metodo para cargar la jerarquia de una categoria recursivamente
        private async Task<Category?> LoadFullCategoryHierarchyAsync(Category category)
        {
            var current = category;
            while (current.ParentCategoryId != null)
            {
                current.ParentCategory = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Id == current.ParentCategoryId);
                current = current.ParentCategory!;
            }
            return category;
        }

        // GET: Categories
        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
        .Include(c => c.ParentCategory)
        .ToListAsync();
            
              var categoryPaths = categories.ToDictionary(
                 c => c.Id,
                 c => GetCategoryPath(c)
              );

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

            // Carga toda la jerarquía antes de calcular el path
            await LoadFullCategoryHierarchyAsync(category);

            ViewBag.CategoryPath = GetCategoryPath(category);
            return View(category);
        }

        // GET: Categories/Create
        public async Task<IActionResult> Create()
        {
            await PopulateParentCategoriesDropdown();
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

            await PopulateParentCategoriesDropdown(category.ParentCategoryId);
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

            await PopulateParentCategoriesDropdown(category.ParentCategoryId);
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
            {
                return NotFound();
            }

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
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ParentCategoryId"] = new SelectList(_context.Categories, "Id", "Id", category.ParentCategoryId);
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

            // Carga jerarquía completa
            await LoadFullCategoryHierarchyAsync(category);

            ViewBag.CategoryPath = GetCategoryPath(category);
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
