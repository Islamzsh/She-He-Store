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
    public class PaymentcardsController : Controller
    {
        private readonly ModelContext _context;

        public PaymentcardsController(ModelContext context)
        {
            _context = context;
        }

        // GET: Paymentcards
        public async Task<IActionResult> Index()
        {
              return _context.Paymentcards != null ? 
                          View(await _context.Paymentcards.ToListAsync()) :
                          Problem("Entity set 'ModelContext.Paymentcards'  is null.");
        }




		// GET: Paymentcards/Details/5
		public async Task<IActionResult> Details(decimal? id)
        {
            if (id == null || _context.Paymentcards == null)
            {
                return NotFound();
            }

            var paymentcard = await _context.Paymentcards
                .FirstOrDefaultAsync(m => m.Paymentcardid == id);
            if (paymentcard == null)
            {
                return NotFound();
            }

            return View(paymentcard);
        }

        // GET: Paymentcards/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Paymentcards/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Paymentcardid,Cardnumber,Name,Balance,ExpirationDate")] Paymentcard paymentcard)
        {
            if (ModelState.IsValid)
            {
                _context.Add(paymentcard);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(paymentcard);
        }

        // GET: Paymentcards/Edit/5
        public async Task<IActionResult> Edit(decimal? id)
        {
            if (id == null || _context.Paymentcards == null)
            {
                return NotFound();
            }

            var paymentcard = await _context.Paymentcards.FindAsync(id);
            if (paymentcard == null)
            {
                return NotFound();
            }
            return View(paymentcard);
        }

        // POST: Paymentcards/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(decimal id, [Bind("Paymentcardid,Cardnumber,Name,Balance,ExpirationDate")] Paymentcard paymentcard)
        {
            if (id != paymentcard.Paymentcardid)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(paymentcard);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PaymentcardExists(paymentcard.Paymentcardid))
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
            return View(paymentcard);
        }

        // GET: Paymentcards/Delete/5
        public async Task<IActionResult> Delete(decimal? id)
        {
            if (id == null || _context.Paymentcards == null)
            {
                return NotFound();
            }

            var paymentcard = await _context.Paymentcards
                .FirstOrDefaultAsync(m => m.Paymentcardid == id);
            if (paymentcard == null)
            {
                return NotFound();
            }

            return View(paymentcard);
        }

        // POST: Paymentcards/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(decimal id)
        {
            if (_context.Paymentcards == null)
            {
                return Problem("Entity set 'ModelContext.Paymentcards'  is null.");
            }
            var paymentcard = await _context.Paymentcards.FindAsync(id);
            if (paymentcard != null)
            {
                _context.Paymentcards.Remove(paymentcard);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PaymentcardExists(decimal id)
        {
          return (_context.Paymentcards?.Any(e => e.Paymentcardid == id)).GetValueOrDefault();
        }
    }
}
