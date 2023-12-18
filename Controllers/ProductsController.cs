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
    public class ProductsController : Controller
    {
        private readonly ModelContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductsController(ModelContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {

		var modelContext = _context.Products.Include(p => p.Category);
            return View(await modelContext.ToListAsync());
        }


		//public IActionResult GetProduct(int id)
		//{
		//	var products = _context.Products.FirstOrDefault(p => p.Productid == id);
		//	return View(products);

		//}
		public IActionResult GetProduct(int id)
        {

            var reviews = _context.Reviews.Include(u => u.User).Where(r => r.Productid == id).ToList();
            var products = _context.Products.Where(p => p.Productid == id).SingleOrDefault();
			var ProductReviewModel = Tuple.Create <  Product  , IEnumerable <Review>>(products,reviews);
			ViewBag.numberOfReviews = reviews.Count();


			// Calculate the average rating
			decimal? averageRating = reviews.Count > 0 ? reviews.Average(r => (decimal?)r.Rating) : null;

			// Convert the average rating to a double (if not null)
			double averageRatingDouble = averageRating.HasValue ? (double)averageRating : 0;

			// Convert the average rating to a scale out of 5
			int averageRatingOutOf5 = (int)Math.Round(averageRatingDouble / 5 * 5);

			ViewBag.averageRatingOutOf5 = averageRatingOutOf5;

			var product = _context.Products.Include(p => p.Category).Single(p => p.Productid == id);
			return View(ProductReviewModel);
		}


        public IActionResult UpdateProductStatus(int productId)
        {
            // Get the product from the database
            var product = _context.Products.FirstOrDefault(p => p.Productid == productId);

            if (product != null && product.Stockquantity == 0)
            {
                product.Status = "Out of Stock";
                _context.SaveChanges(); // Save changes to the database
            }

            return RedirectToAction("GetProduct", new { id = productId });
        }



        //[HttpPost]
        //public IActionResult AddToCart(int productId, int chosenQuantity)
        //{
        //	var product = _context.Products.Find(productId);

        //	if (product != null)
        //	{
        //		if (chosenQuantity > 0 && chosenQuantity <= product.Stockquantity)
        //		{
        //			// Quantity is valid, proceed with adding the product to the cart
        //			// You can implement your cart logic here

        //			TempData["SuccessMessage"] = "Product added to cart successfully.";
        //		}
        //		else
        //		{
        //			// Handle the case where the chosen quantity is invalid
        //			TempData["ErrorMessage"] = "Invalid quantity. Please choose a quantity between 1 and " + product.Stockquantity;
        //		}
        //	}
        //	else
        //	{
        //		// Handle the case where the product is not found
        //		TempData["ErrorMessage"] = "Product not found.";
        //	}

        //	// Redirect to the product details page with an appropriate message
        //	return RedirectToAction("ProductDetails", new { id = productId });
        //}



        // GET: Products/Details/5
        public async Task<IActionResult> Details(decimal? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Productid == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            ViewData["Categoryid"] = new SelectList(_context.Categories, "Categoryid", "Categoryid");
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Productid,Productname,Description,Price,ImageFile,Stockquantity,Sale,Status,Categoryid")] Product product)
        {
            if (ModelState.IsValid)
            {
                if (product.ImageFile != null)
                {
                    string wwwRootPath = _webHostEnvironment.WebRootPath;
                    string fileName = Guid.NewGuid().ToString() + "_" + product.ImageFile.FileName;//get random name and _ ,  imagefile ,file name
                    string path = Path.Combine(wwwRootPath + "/images/", fileName);
                    using (var fileStream = new FileStream(path, FileMode.Create))
                    {
                        await product.ImageFile.CopyToAsync(fileStream);
                    }
                    product.ImagePath = fileName;
                }
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Categoryid"] = new SelectList(_context.Categories, "Categoryid", "Categoryid", product.Categoryid);
            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(decimal? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["Categoryid"] = new SelectList(_context.Categories, "Categoryid", "Categoryid", product.Categoryid);
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(decimal id, [Bind("Productid,Productname,Description,Price,ImageFile,Stockquantity,Sale,Status,Categoryid")] Product product)
        {
            if (id != product.Productid)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (product.ImageFile != null)
                    {
                        string wwwRootPath = _webHostEnvironment.WebRootPath;
                        string fileName = Guid.NewGuid().ToString() + "_" + product.ImageFile.FileName;//get random name and _ ,  imagefile ,file name
                        string path = Path.Combine(wwwRootPath + "/images/", fileName);
                        using (var fileStream = new FileStream(path, FileMode.Create))
                        {
                            await product.ImageFile.CopyToAsync(fileStream);
                        }
                        product.ImagePath = fileName;
                    }
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Productid))
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
            ViewData["Categoryid"] = new SelectList(_context.Categories, "Categoryid", "Categoryid", product.Categoryid);
            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(decimal? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Productid == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(decimal id)
        {
            if (_context.Products == null)
            {
                return Problem("Entity set 'ModelContext.Products'  is null.");
            }
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(decimal id)
        {
          return (_context.Products?.Any(e => e.Productid == id)).GetValueOrDefault();
        }
    }
}
