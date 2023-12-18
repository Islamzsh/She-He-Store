using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using She_He_Store.Models;
using System.Diagnostics;
using System.Security.Claims;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace She_He_Store.Controllers
{
	public class HomeController : Controller
	{
        private readonly ModelContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;//declare variable


        public HomeController(ModelContext context , IWebHostEnvironment webHostEnvironment)
		{
			_context = context;
            _webHostEnvironment = webHostEnvironment;


        }

        public IActionResult Index()
		{
            var products = _context.Products.Take(8).ToList();
			var category = _context.Categories.ToList();
			var product = _context.Products.ToList();
            var approvedTestimonials = _context.Testimonials.Where(t => t.Status == "approved").ToList();
			var users = _context.Users.Include(u => u.UserLogins).ToList();

           

          
          

            var model = Tuple.Create<IEnumerable<Category>, IEnumerable<Product>, IEnumerable<Testimonial>, IEnumerable<User>>(category, product, approvedTestimonials, users);
            return View(model);
		}
       
        public IActionResult GetProductByCategory(int id)
        {
            var products = _context.Products.Where(p => p.Categoryid == id).ToList();
            return View(products);
        }


		
			public IActionResult Profile()
		    {
			var userId = HttpContext.Session.GetInt32("UserId");

			if (userId == null)
			{
				return RedirectToAction("Login", "LoginAndRegister");
			}

            var user = _context.Users.SingleOrDefault(u => u.Userid == userId);
            var infoOfUser = _context.Users.Where(u => u.Userid == userId).FirstOrDefault();
			var infoOfLoginUser = _context.UserLogins.Where(u => u.Userid == userId).FirstOrDefault();
			var collectInfo = Tuple.Create<User, UserLogin>(infoOfUser, infoOfLoginUser);



			return View(collectInfo);
		     }




        public IActionResult EditProfile()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            var user = _context.Users.FirstOrDefault(u => u.Userid == userId);
            var login = _context.UserLogins.FirstOrDefault(u => u.Userid == userId);
            var model = Tuple.Create<User, UserLogin>(user, login);


            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> EditProfile([Bind("Gender, FirstName,LastName ,ImageFile")] User user, [Bind("Username ,Password")] UserLogin userLogin)
        {


            var userId = HttpContext.Session.GetInt32("UserId");
            var existingUser = _context.Users.SingleOrDefault(u => u.Userid == userId);
            if (existingUser != null)
            {
                //_context.UserLogins.Update(userLogin);
                if (user.ImageFile != null)
                {
                    string wwwRootPath = _webHostEnvironment.WebRootPath;
                    string fileName = Guid.NewGuid().ToString() + "_" + user.ImageFile.FileName;//get random name and _ ,  imagefile ,file name
                    string path = Path.Combine(wwwRootPath + "/images/", fileName);
                    using (var fileStream = new FileStream(path, FileMode.Create))
                    {
                        await user.ImageFile.CopyToAsync(fileStream);
                    }
                    user.ImagePath = fileName;
                }
                existingUser.FirstName = user.FirstName;
                existingUser.LastName = user.LastName;
                existingUser.ImagePath = user.ImagePath;
                existingUser.Gender = user.Gender;
                _context.Users.Update(existingUser);
                _context.SaveChanges();



                var login = _context.UserLogins.Where(x => x.Userid == userId).SingleOrDefault();
                login.Username = userLogin.Username;
                login.Password = userLogin.Password;

                _context.UserLogins.Update(login);
                _context.SaveChanges();

                return RedirectToAction("Profile");



            }
            return View();
        }








        public IActionResult JoinTables()
        {
            var user = _context.Users.ToList();
            var orderItems = _context.Orderitems.ToList();
            var product = _context.Products.ToList();
            var category = _context.Categories.ToList();

            var multiTable = from u in user
                             join o in orderItems on u.Userid equals o.Order.Userid
                             join p in product on o.Productid equals p.Productid
                             join c in category on p.Categoryid equals c.Categoryid
                             select new JoinTables { Product = p, User = u, Orderitem = o, Category = c};

            return View(multiTable);
        }



        public IActionResult UserOrders()
		{
			
			var userId = HttpContext.Session.GetInt32("UserId");

			if (userId == null)
			{
			
				return RedirectToAction("Login", "LoginAndRegister");
			}

			// Retrieve orders for the current user
			var userOrders = _context.Orders
				.Where(o => o.Userid == userId)
				.ToList();

			return View(userOrders);
		}



      

        public async Task<IActionResult> TrackOrder(int orderId)
        {
            if (orderId == 0)
            {
                ViewData["ErrorMessage"] = "Order ID is required.";
                return View("TrackOrder");
            }

            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                ViewData["ErrorMessage"] = "Please log in to track your orders.";
                return View("TrackOrder");
            }

            var orderViewModel = await _context.Orders
                .Include(o => o.Orderitems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.IdNavigation) 
                .Where(o => o.Orderid == orderId && o.Userid == userId)
                .Select(o => new OrderViewModel
                {
                    Order = o,
                    Orderitem = o.Orderitems.ToList(),
                    Product = o.Orderitems.FirstOrDefault().Product, 
                    Shipping = o.IdNavigation 
                })
                .FirstOrDefaultAsync();

            if (orderViewModel == null)
            {
                ViewData["ErrorMessage"] = "Order not found or does not belong to the current user.";
                return View("TrackOrder");
            }

            return View(orderViewModel);
        }


        public IActionResult AddReview()
		{
			return View();

		}



		[HttpPost]
		public async Task<IActionResult> AddReview(Testimonial review )
		{

			var userId = HttpContext.Session.GetInt32("UserId");
			var user = _context.Users.FirstOrDefault(u => u.Userid == userId);

			if (userId == null)
			{

				return RedirectToAction("Login", "LoginAndRegister");
			}
			
			review.Customername = user.FirstName + " " + user.LastName; 
			review.Testimonialdate = DateTime.Now;
			review.Status = "Pending";
			review.Userid = userId;

			_context.Testimonials.Add(review);
			await _context.SaveChangesAsync();

			return Json(new { message = "Thank you, " + user?.FirstName + "!" });

		}
		
        public IActionResult Sale()
		{
			var productsOnSale = _context.Products.Where(p => p.Sale > 0).ToList();
			return View(productsOnSale);
		}

        public IActionResult Shop(string searchTerm, string selectedCategory)
        {
            var categories = _context.Categories.ToList();
            var products = _context.Products.ToList();

            // Filter by category
            if (!string.IsNullOrEmpty(selectedCategory) && selectedCategory != "All")
            {
				products = products.Where(p => p.Category.Categoryname == selectedCategory).ToList();
            }

            // Search by product name
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                products = products.Where(p => p.Productname.ToLower().Contains(searchTerm)).ToList();
            }

            var categoryProductModel = Tuple.Create<IEnumerable<Category>, IEnumerable<Product>>(categories, products);

            return View(categoryProductModel);
        }


        public IActionResult Privacy()
		{
			return View();
		}
		public IActionResult About()
		{
			return View();
		}
        public IActionResult ContactUs()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> AddContact(string Address, string Phone, string Email,string Subject, string Message)
        {

            try
            {

               var newContact = new Contactu
                {
                  Subject = Subject,	
				  Address = Address,
				  Phone = Phone,
                  Email = Email,
                  Message = Message,
                };

                _context.Contactus.Add(newContact);
                await _context.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }

            return Json(new { success = true, message = "Your Message Sent Successfully!" });
        }


        public IActionResult FAQ()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}