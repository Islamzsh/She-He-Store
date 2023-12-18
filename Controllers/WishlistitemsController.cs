using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using She_He_Store.Models;

namespace She_He_Store.Controllers
{
    public class WishlistitemsController : Controller
    {
        private readonly ModelContext _context;
		private readonly ILogger<WishlistitemsController> _logger;

		public WishlistitemsController(ModelContext context, ILogger<WishlistitemsController> logger)
		{
            _context = context;
			_logger = logger;

		}

		// GET: Wishlistitems
		public async Task<IActionResult> Index()
        {
            var modelContext = _context.Wishlistitems.Include(w => w.Product).Include(w => w.User);
            return View(await modelContext.ToListAsync());
        }
		public IActionResult WishListItems()
		{

			return View();

		}
		public IActionResult MyWishList()
		{
			var userId = HttpContext.Session.GetInt32("UserId");

			var wishList = _context.Wishlistitems.Where(wishList => wishList.Userid == userId).ToList();
			var product = _context.Products.ToList();

			var productWish = from w in wishList
							  join p in product
							  on w.Productid equals p.Productid
							  select new WishListOfProduct
							  {
								  Wishlistitem = w,
								  Product = p
							  };

			if (productWish != null)
			{ return View(productWish); }
			else return View();

		}



		


		[HttpPost]
		public async Task<IActionResult> AddProductsToWishList(int id)
		{
			try
			{
				// Get user id
				var userId = HttpContext.Session.GetInt32("UserId");

				if (userId == null)
				{
					var ReturnUrl = Url.Action("GetProduct", "Products", new { id = id });
					return Json(new { redirect = Url.Action("Login", "LoginAndRegister", new { returnUrl = ReturnUrl }) });
				}

				// Check if the item is already in the user's wishlist.
				var existingWishListItem = await _context.Wishlistitems
					.SingleOrDefaultAsync(w => w.Userid == userId.Value && w.Productid == id);

				if (existingWishListItem == null)
				{
					// If the item is not in the user's wishlist, create a new wishlist item.
					var item = new Wishlistitem
					{
						Userid = userId.Value,
						Productid = id,
					};

					_context.Wishlistitems.Add(item);
					await _context.SaveChangesAsync();

				}

				

				// Return a JSON response indicating success
				return Json(new { success = true, message = "is added to wishlist successfully!" });
				//return RedirectToAction("MyWishList", "Wishlistitems");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred in the AddProductsToWishList action.");
				return Json(new { success = false, message = "Error adding product to wishlist." });
			}

		}



		public IActionResult ViewWishlist()
		{
			var userId = HttpContext.Session.GetInt32("UserId");
			var wishlistItems = _context.Wishlistitems
				.Include(w => w.Product)
				.Where(w => w.Userid == userId)
				.ToList();

			return View(wishlistItems);
		}



        public IActionResult DeleteItem(int ItemId)
        {
            // Find the item in the wishlist by ItemId
            var item = _context.Wishlistitems.FirstOrDefault(w => w.Wishlistitemid == ItemId);

            if (item == null)
            {
                return Json(new { success = false, message = "Item not found" });
            }

            // Remove the item from the wishlist
            _context.Wishlistitems.Remove(item);
            _context.SaveChanges();

            return Json(new { success = true });
        }



        // GET: Wishlistitems/Details/5
        public async Task<IActionResult> Details(decimal? id)
        {
            if (id == null || _context.Wishlistitems == null)
            {
                return NotFound();
            }

            var wishlistitem = await _context.Wishlistitems
                .Include(w => w.Product)
                .Include(w => w.User)
                .FirstOrDefaultAsync(m => m.Wishlistitemid == id);
            if (wishlistitem == null)
            {
                return NotFound();
            }

            return View(wishlistitem);
        }

       
       
    

	// GET: Wishlistitems/Create
	public IActionResult Create()
        {
            ViewData["Productid"] = new SelectList(_context.Products, "Productid", "Productid");
            ViewData["Userid"] = new SelectList(_context.Users, "Userid", "Userid");
            return View();
        }






		// POST: Wishlistitems/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Wishlistitemid,Userid,Productid")] Wishlistitem wishlistitem)
        {
            if (ModelState.IsValid)
            {
                _context.Add(wishlistitem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Productid"] = new SelectList(_context.Products, "Productid", "Productid", wishlistitem.Productid);
            ViewData["Userid"] = new SelectList(_context.Users, "Userid", "Userid", wishlistitem.Userid);
            return View(wishlistitem);
        }

        // GET: Wishlistitems/Edit/5
        public async Task<IActionResult> Edit(decimal? id)
        {
            if (id == null || _context.Wishlistitems == null)
            {
                return NotFound();
            }

            var wishlistitem = await _context.Wishlistitems.FindAsync(id);
            if (wishlistitem == null)
            {
                return NotFound();
            }
            ViewData["Productid"] = new SelectList(_context.Products, "Productid", "Productid", wishlistitem.Productid);
            ViewData["Userid"] = new SelectList(_context.Users, "Userid", "Userid", wishlistitem.Userid);
            return View(wishlistitem);
        }

        // POST: Wishlistitems/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(decimal id, [Bind("Wishlistitemid,Userid,Productid")] Wishlistitem wishlistitem)
        {
            if (id != wishlistitem.Wishlistitemid)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(wishlistitem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WishlistitemExists(wishlistitem.Wishlistitemid))
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
            ViewData["Productid"] = new SelectList(_context.Products, "Productid", "Productid", wishlistitem.Productid);
            ViewData["Userid"] = new SelectList(_context.Users, "Userid", "Userid", wishlistitem.Userid);
            return View(wishlistitem);
        }

        // GET: Wishlistitems/Delete/5
        public async Task<IActionResult> Delete(decimal? id)
        {
            if (id == null || _context.Wishlistitems == null)
            {
                return NotFound();
            }

            var wishlistitem = await _context.Wishlistitems
                .Include(w => w.Product)
                .Include(w => w.User)
                .FirstOrDefaultAsync(m => m.Wishlistitemid == id);
            if (wishlistitem == null)
            {
                return NotFound();
            }

            return View(wishlistitem);
        }

        // POST: Wishlistitems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(decimal id)
        {
            if (_context.Wishlistitems == null)
            {
                return Problem("Entity set 'ModelContext.Wishlistitems'  is null.");
            }
            var wishlistitem = await _context.Wishlistitems.FindAsync(id);
            if (wishlistitem != null)
            {
                _context.Wishlistitems.Remove(wishlistitem);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool WishlistitemExists(decimal id)
        {
          return (_context.Wishlistitems?.Any(e => e.Wishlistitemid == id)).GetValueOrDefault();
        }
    }
}
