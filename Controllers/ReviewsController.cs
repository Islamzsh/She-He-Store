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
    public class ReviewsController : Controller
    {
        private readonly ModelContext _context;
        private readonly ILogger<ReviewsController> _logger;

        public ReviewsController(ModelContext context, ILogger<ReviewsController> logger)
        {
            _context = context;
            _logger = logger;

        }

        // GET: Reviews
        public async Task<IActionResult> Index()
        {
            var modelContext = _context.Reviews.Include(r => r.Product).Include(r => r.User);
            return View(await modelContext.ToListAsync());
        }

		

		[HttpPost]
        public async Task<IActionResult> AddReview(int id, Review review)

		{
            try
            {
				var product = await _context.Products.FirstOrDefaultAsync(p => p.Productid == id);
				if (product == null)
				{
					return NotFound();
				}
				// Ensure the user is logged in
				var userId = HttpContext.Session.GetInt32("UserId");

                if (userId == null)
                {
                    return RedirectToAction("Login", "LoginAndRegister");
                }

                // Associate the review with the user and product
                review.Userid = userId.Value;
				review.Productid = product.Productid;
				review.Createddate = DateTime.Now;


				if (ModelState.IsValid)
                {

                    var newReview = new Review
                    {
                        Userid = userId.Value,
                        Productid = id,
                        Comments = review.Comments,
                        Rating = review.Rating,
                        Createddate = DateTime.Now
					};

					// Add the review to the database
					_context.Reviews.Add(newReview);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction("GetProduct","Products", new { id = review.Productid });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in the AddReview action.");
                return View("Error");
            }
        }


		// GET: Reviews/Details/5
		public async Task<IActionResult> Details(decimal? id)
        {
            if (id == null || _context.Reviews == null)
            {
                return NotFound();
            }

            var review = await _context.Reviews
                .Include(r => r.Product)
                .Include(r => r.User)
                .FirstOrDefaultAsync(m => m.Reviewid == id);
            if (review == null)
            {
                return NotFound();
            }

            return View(review);
        }

        // GET: Reviews/Create
        public IActionResult Create()
        {
            ViewData["Productid"] = new SelectList(_context.Products, "Productid", "Productid");
            ViewData["Userid"] = new SelectList(_context.Users, "Userid", "Userid");
            return View();
        }

        // POST: Reviews/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Reviewid,Userid,Productid,Rating,Comments,Createddate")] Review review)
        {
            if (ModelState.IsValid)
            {
                _context.Add(review);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Productid"] = new SelectList(_context.Products, "Productid", "Productid", review.Productid);
            ViewData["Userid"] = new SelectList(_context.Users, "Userid", "Userid", review.Userid);
            return View(review);
        }

        // GET: Reviews/Edit/5
        public async Task<IActionResult> Edit(decimal? id)
        {
            if (id == null || _context.Reviews == null)
            {
                return NotFound();
            }

            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                return NotFound();
            }
            ViewData["Productid"] = new SelectList(_context.Products, "Productid", "Productid", review.Productid);
            ViewData["Userid"] = new SelectList(_context.Users, "Userid", "Userid", review.Userid);
            return View(review);
        }

        // POST: Reviews/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(decimal id, [Bind("Reviewid,Userid,Productid,Rating,Comments,Createddate")] Review review)
        {
            if (id != review.Reviewid)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(review);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReviewExists(review.Reviewid))
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
            ViewData["Productid"] = new SelectList(_context.Products, "Productid", "Productid", review.Productid);
            ViewData["Userid"] = new SelectList(_context.Users, "Userid", "Userid", review.Userid);
            return View(review);
        }

        // GET: Reviews/Delete/5
        public async Task<IActionResult> Delete(decimal? id)
        {
            if (id == null || _context.Reviews == null)
            {
                return NotFound();
            }

            var review = await _context.Reviews
                .Include(r => r.Product)
                .Include(r => r.User)
                .FirstOrDefaultAsync(m => m.Reviewid == id);
            if (review == null)
            {
                return NotFound();
            }

            return View(review);
        }

        // POST: Reviews/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(decimal id)
        {
            if (_context.Reviews == null)
            {
                return Problem("Entity set 'ModelContext.Reviews'  is null.");
            }
            var review = await _context.Reviews.FindAsync(id);
            if (review != null)
            {
                _context.Reviews.Remove(review);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReviewExists(decimal id)
        {
          return (_context.Reviews?.Any(e => e.Reviewid == id)).GetValueOrDefault();
        }
    }
}
