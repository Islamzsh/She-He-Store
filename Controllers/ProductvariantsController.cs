using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using She_He_Store.Models;

namespace She_He_Store.Controllers
{
    public class ProductvariantsController : Controller
    {
        private readonly ModelContext _context;

        public ProductvariantsController(ModelContext context)
        {
            _context = context;
        }

        // GET: Productvariants
        public async Task<IActionResult> Index()
        {
            var modelContext = _context.Productvariants.Include(p => p.Product);
            return View(await modelContext.ToListAsync());
        }

        // GET: Productvariants/Details/5
        public async Task<IActionResult> Details(decimal? id)
        {
            if (id == null || _context.Productvariants == null)
            {
                return NotFound();
            }

            var productvariant = await _context.Productvariants
                .Include(p => p.Product)
                .FirstOrDefaultAsync(m => m.Variantid == id);
            if (productvariant == null)
            {
                return NotFound();
            }

            return View(productvariant);
        }

        // GET: Productvariants/Create
        public IActionResult Create()
        {
            ViewData["Productid"] = new SelectList(_context.Products, "Productid", "Productid");
            return View();
        }

        // POST: Productvariants/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Variantid,Productid,Productsize,Color")] Productvariant productvariant)
        {
            if (ModelState.IsValid)
            {
                _context.Add(productvariant);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Productid"] = new SelectList(_context.Products, "Productid", "Productid", productvariant.Productid);
            return View(productvariant);
        }

        // GET: Productvariants/Edit/5
        public async Task<IActionResult> Edit(decimal? id)
        {
            if (id == null || _context.Productvariants == null)
            {
                return NotFound();
            }

            var productvariant = await _context.Productvariants.FindAsync(id);
            if (productvariant == null)
            {
                return NotFound();
            }
            ViewData["Productid"] = new SelectList(_context.Products, "Productid", "Productid", productvariant.Productid);
            return View(productvariant);
        }

        // POST: Productvariants/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(decimal id, [Bind("Variantid,Productid,Productsize,Color")] Productvariant productvariant)
        {
            if (id != productvariant.Variantid)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(productvariant);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductvariantExists(productvariant.Variantid))
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
            ViewData["Productid"] = new SelectList(_context.Products, "Productid", "Productid", productvariant.Productid);
            return View(productvariant);
        }

        // GET: Productvariants/Delete/5
        public async Task<IActionResult> Delete(decimal? id)
        {
            if (id == null || _context.Productvariants == null)
            {
                return NotFound();
            }

            var productvariant = await _context.Productvariants
                .Include(p => p.Product)
                .FirstOrDefaultAsync(m => m.Variantid == id);
            if (productvariant == null)
            {
                return NotFound();
            }

            return View(productvariant);
        }

        // POST: Productvariants/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(decimal id)
        {
            if (_context.Productvariants == null)
            {
                return Problem("Entity set 'ModelContext.Productvariants'  is null.");
            }
            var productvariant = await _context.Productvariants.FindAsync(id);
            if (productvariant != null)
            {
                _context.Productvariants.Remove(productvariant);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductvariantExists(decimal id)
        {
          return (_context.Productvariants?.Any(e => e.Variantid == id)).GetValueOrDefault();
        }
    }
}
