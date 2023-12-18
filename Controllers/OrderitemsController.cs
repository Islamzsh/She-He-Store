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
    public class OrderitemsController : Controller
    {
        private readonly ModelContext _context;

        public OrderitemsController(ModelContext context)
        {
            _context = context;
        }

        // GET: Orderitems
        public async Task<IActionResult> Index()
        {
            var modelContext = _context.Orderitems.Include(o => o.Order).Include(o => o.Product);
            return View(await modelContext.ToListAsync());
        }

        // GET: Orderitems/Details/5
        public async Task<IActionResult> Details(decimal? id)
        {
            if (id == null || _context.Orderitems == null)
            {
                return NotFound();
            }

            var orderitem = await _context.Orderitems
                .Include(o => o.Order)
                .Include(o => o.Product)
                .FirstOrDefaultAsync(m => m.Orderitemid == id);
            if (orderitem == null)
            {
                return NotFound();
            }

            return View(orderitem);
        }

        // GET: Orderitems/Create
        public IActionResult Create()
        {
            ViewData["Orderid"] = new SelectList(_context.Orders, "Orderid", "Orderid");
            ViewData["Productid"] = new SelectList(_context.Products, "Productid", "Productid");
            return View();
        }

        // POST: Orderitems/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Orderitemid,Orderid,Productid,Quantity,Price")] Orderitem orderitem)
        {
            if (ModelState.IsValid)
            {
                _context.Add(orderitem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Orderid"] = new SelectList(_context.Orders, "Orderid", "Orderid", orderitem.Orderid);
            ViewData["Productid"] = new SelectList(_context.Products, "Productid", "Productid", orderitem.Productid);
            return View(orderitem);
        }

        // GET: Orderitems/Edit/5
        public async Task<IActionResult> Edit(decimal? id)
        {
            if (id == null || _context.Orderitems == null)
            {
                return NotFound();
            }

            var orderitem = await _context.Orderitems.FindAsync(id);
            if (orderitem == null)
            {
                return NotFound();
            }
            ViewData["Orderid"] = new SelectList(_context.Orders, "Orderid", "Orderid", orderitem.Orderid);
            ViewData["Productid"] = new SelectList(_context.Products, "Productid", "Productid", orderitem.Productid);
            return View(orderitem);
        }

        // POST: Orderitems/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(decimal id, [Bind("Orderitemid,Orderid,Productid,Quantity,Price")] Orderitem orderitem)
        {
            if (id != orderitem.Orderitemid)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(orderitem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderitemExists(orderitem.Orderitemid))
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
            ViewData["Orderid"] = new SelectList(_context.Orders, "Orderid", "Orderid", orderitem.Orderid);
            ViewData["Productid"] = new SelectList(_context.Products, "Productid", "Productid", orderitem.Productid);
            return View(orderitem);
        }

        // GET: Orderitems/Delete/5
        public async Task<IActionResult> Delete(decimal? id)
        {
            if (id == null || _context.Orderitems == null)
            {
                return NotFound();
            }

            var orderitem = await _context.Orderitems
                .Include(o => o.Order)
                .Include(o => o.Product)
                .FirstOrDefaultAsync(m => m.Orderitemid == id);
            if (orderitem == null)
            {
                return NotFound();
            }

            return View(orderitem);
        }

        // POST: Orderitems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(decimal id)
        {
            if (_context.Orderitems == null)
            {
                return Problem("Entity set 'ModelContext.Orderitems'  is null.");
            }
            var orderitem = await _context.Orderitems.FindAsync(id);
            if (orderitem != null)
            {
                _context.Orderitems.Remove(orderitem);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderitemExists(decimal id)
        {
          return (_context.Orderitems?.Any(e => e.Orderitemid == id)).GetValueOrDefault();
        }
    }
}
