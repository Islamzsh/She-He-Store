using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using She_He_Store.Models;
using System.Data;
using System.Security.Claims;
namespace She_He_Store.Controllers
{

    public class AdminController : Controller
    {
        private readonly ModelContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;//declare variable

        public AdminController(ModelContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Dashboard()
        {

            ViewBag.AdminName = HttpContext.Session.GetString("AdminName");
            ViewBag.AdminImagePath = HttpContext.Session.GetString("AdminImagePath");

            ViewBag.numberOfUsers = _context.Users.Count();
            ViewBag.sales = _context.Orderitems.Sum(oi => oi.Quantity);

            ViewBag.minPrice = _context.Products.Min(x => x.Price);
            ViewBag.maxPrice = _context.Products.Max(x => x.Price);
            ViewBag.numberOfOredrs = _context.Orders.Count();
            ViewBag.numbaerOfProducts = _context.Products.Count();

            //number of delivered ,pindding ,  Arrived  and shipped orders 

            ViewBag.deliveredOrders = _context.Orders.Count(o => o.Orderstatus == "Delivered");
            ViewBag.pendingOrders = _context.Orders.Count(o => o.Orderstatus == "Pending");
            ViewBag.arrivedOrders = _context.Orders.Count(o => o.Orderstatus == "Arrived");
            ViewBag.shippedOrders = _context.Orders.Count(o => o.Orderstatus == "Shipped");
            ViewBag.numberOfMale = _context.Users.Count(u => u.Gender == "Male");

            ViewBag.numberOfFemale = _context.Users.Count(u => u.Gender == "Female");



            var productPrices = _context.Products.Select(p => p.Price).ToList();
            int numBins = 7;
            double minPrice = (double)productPrices.Min();
            double maxPrice = (double)productPrices.Max();
            double binWidth = (maxPrice - minPrice) / numBins;

            double[] binBoundaries = new double[numBins + 1];
            for (int i = 0; i <= numBins; i++)
            {
                binBoundaries[i] = minPrice + i * binWidth;
            }
            int[] binCounts = new int[numBins];

            // Populate the bin counts
            foreach (var price in productPrices)
            {
                decimal minPriceDecimal = (decimal)minPrice;
                decimal binWidthDecimal = (decimal)binWidth;
                int binIndex = (int)((price - minPriceDecimal) / binWidthDecimal);
                if (binIndex >= 0 && binIndex < numBins)
                {
                    binCounts[binIndex]++;
                }
            }
            ViewBag.BinBoundaries = JsonConvert.SerializeObject(binBoundaries);
            ViewBag.BinCounts = JsonConvert.SerializeObject(binCounts);





            ViewData["maxPrice"] = _context.Products.Max(x => x.Price);

            var topProducts = _context.Orderitems
        .GroupBy(oi => oi.Productid)
        .Select(g => new { ProductId = g.Key, Quantity = g.Sum(oi => oi.Quantity) })
        .OrderByDescending(x => x.Quantity)
        .Take(5) 
        .ToList();

            // Get the product names and quantities
            var productNames = topProducts.Select(tp => _context.Products.Find(tp.ProductId).Productname);
            var productQuantities = topProducts.Select(tp => tp.Quantity);

            ViewBag.TopProductNames = JsonConvert.SerializeObject(productNames);
            ViewBag.TopProductQuantities = JsonConvert.SerializeObject(productQuantities);


            //oreder vs. time
            // Get time labels for the chart
            var orders = _context.Orders.Where(o => o.Orderdate != null).OrderBy(o => o.Orderdate).ToList();
            ViewBag.TimeLabels = orders.Select(o => o.Orderdate.Value.Date.ToString("yyyy-MM-dd")).ToList();

            // Get the count of orders for each date
            ViewBag.OrderCounts = orders.GroupBy(o => o.Orderdate.Value.Date).Select(g => g.Count()).ToList();




            var client = _context.Users.ToList();
            var product = _context.Products.ToList();
            var ProductUserModel = Tuple.Create<IEnumerable<User>, IEnumerable<Product>>(client, product);

            return View(ProductUserModel);
        }



        public IActionResult Contacts()
        {
            var contact = _context.Contactus
                .ToList();

            return View(contact);
        }


        public IActionResult Reviews()
        {
            var reveiws = _context.Testimonials.ToList();
            return View(reveiws);
        }




        [HttpPost]
        public async Task<IActionResult> UpdateStatus(decimal testimonialId, string newStatus)
        {
            var testimonial = _context.Testimonials.FirstOrDefault(t => t.Testimonialid == testimonialId);
            if (testimonial == null)
            {
                return NotFound();
            }

            testimonial.Status = newStatus;

            _context.Update(testimonial);
            await _context.SaveChangesAsync();

            return Ok();
        }




        public IActionResult ManageOrders()
        {
            // Retrieve orders, order items, and shipping information
            var orders = _context.Orders
                .Include(o => o.Orderitems)
                .Include(o => o.IdNavigation)
                .ToList();

            return View(orders);
        }
        [HttpPost]
        public IActionResult UpdateOrderStatus(decimal orderId, string newStatus)
        {
            var order = _context.Orders.FirstOrDefault(o => o.Orderid == orderId);

            if (order != null)
            {
                order.Orderstatus = newStatus;
                _context.SaveChanges();
                return Json(new { success = true, newStatus = newStatus });
            }

            return Json(new { success = false, message = "Order not found" });
        }
        //report 

        [HttpGet]
        public IActionResult Report()
        {
            var userId = HttpContext.Session.GetInt32("AdminId");


            if (userId != null)
            {
                var user = _context.Users.ToList();
                var order = _context.Orders.ToList();
                var orderItem = _context.Orderitems.Include(o => o.Order).ToList();
                var product = _context.Products.ToList();
                var category = _context.Categories.ToList();

                
                //join 
                var multiTable = from u in user
                                 join o in orderItem on u.Userid equals o.Order.Userid
                                 join p in product on o.Productid equals p.Productid
                                 join c in category on p.Categoryid equals c.Categoryid
                                 select new JoinTables { Product = p, User = u, Orderitem = o, Category = c };

                var orderModel = _context.Orderitems.Include(o => o.Order).Include(p => p.Product).ToList();
                ViewBag.TotalQuantity = orderModel.Sum(x => x.Quantity);
                ViewBag.TotalPrice = orderModel.Sum(x => x.Product.Price * x.Quantity);
                var newModel = Tuple.Create<IEnumerable<JoinTables>, IEnumerable<Orderitem>>(multiTable, orderModel);
                return View(multiTable);
            }

            return RedirectToAction("Index", "Home");
        }





        [HttpPost]
        public async Task<IActionResult> Report(DateTime? startDate, DateTime? endDate)
        {

            var userId = HttpContext.Session.GetInt32("AdminId");
            if (userId != null)
            {
                var user = _context.Users.ToList();
                var order = _context.Orders.ToList();
                var orderItem = _context.Orderitems.Include(o => o.Order).ToList();
                var product = _context.Products.ToList();
                var category = _context.Categories.ToList();

                //join used in 
                //join 
                var multiTable = from u in user
                                 join o in orderItem on u.Userid equals o.Order.Userid
                                 join p in product on o.Productid equals p.Productid
                                 join c in category on p.Categoryid equals c.Categoryid
                                 select new JoinTables { Product = p, User = u, Orderitem = o, Category = c };

                var orderModel = _context.Orderitems.Include(o => o.Order).Include(p => p.Product).ToList();


                var modelContext = _context.Orderitems.Include(o => o.Order).Include(p => p.Product);


                if (startDate == null && endDate == null)
                {
                    

                    ViewBag.TotalQuantity = orderModel.Sum(x => x.Quantity);
                    ViewBag.TotalPrice = orderModel.Sum(x => x.Product.Price * x.Quantity);
                    var newModel = Tuple.Create<IEnumerable<JoinTables>, IEnumerable<Orderitem>>(multiTable,modelContext.ToList());
                    return View(newModel);
                }

                else if (startDate == null && endDate != null)
                {

                    ViewBag.TotalQuantity = orderModel.Sum(x => x.Quantity);
                    ViewBag.TotalPrice = orderModel.Sum(x => x.Product.Price * x.Quantity);
                    var r1= multiTable.Where(x => x.Orderitem.Order.Orderdate.Value.Date <= endDate);
                    var result = await modelContext.Where(x => x.Order.Orderdate.Value.Date <= endDate).ToListAsync();
                    var newModel = Tuple.Create<IEnumerable<JoinTables>, IEnumerable<Orderitem>>(r1, result);

                    return View(r1);
                }

                else if (startDate != null && endDate == null)
                {
                    ViewBag.TotalQuantity = orderModel.Sum(x => x.Quantity);
                    ViewBag.TotalPrice = orderModel.Sum(x => x.Product.Price * x.Quantity);
                    var r1 = multiTable.Where(x => x.Orderitem.Order.Orderdate.Value.Date >= startDate);
                    var result = await modelContext.Where(x => x.Order.Orderdate.Value.Date >= startDate).ToListAsync();
                    var newModel = Tuple.Create<IEnumerable<JoinTables>, IEnumerable<Orderitem>>(r1, result);

                    return View(r1);
                }
                else
                {
                    ViewBag.TotalQuantity = orderModel.Sum(x => x.Quantity);
                    ViewBag.TotalPrice = orderModel.Sum(x => x.Product.Price * x.Quantity);
                    var r1 = multiTable.Where(x => x.Orderitem.Order.Orderdate.Value.Date >= startDate && x.Orderitem.Order.Orderdate.Value.Date <= endDate);
                    var result = await modelContext.Where(x => x.Order.Orderdate.Value.Date >= startDate && x.Order.Orderdate.Value.Date <= endDate).ToListAsync();
                    var newModel = Tuple.Create<IEnumerable<JoinTables>, IEnumerable<Orderitem>>(r1, result);

                    return View(r1);
                }
            }
            return RedirectToAction("Index", "Home");
        }
        		

    



        public IActionResult Profile()
        {
            var adminId = HttpContext.Session.GetInt32("UserId");


            
            if (adminId == null)
            {
                return RedirectToAction("Login", "LoginAndRegister");
            }

            var admin = _context.Users.SingleOrDefault(u => u.Userid == adminId);


            var infoOfAdmin= _context.Users.Where(u => u.Userid == adminId).FirstOrDefault();
            var infoOfLoginAdmin = _context.UserLogins.Where(u => u.Userid == adminId).FirstOrDefault();
            var collectInfo = Tuple.Create<User, UserLogin>(infoOfAdmin, infoOfLoginAdmin);



            return View(collectInfo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Profile([Bind("Gender, FirstName,LastName ,ImageFile")] User user , [Bind("Username ,Password")] UserLogin userLogin)
        {

            
                var adminUserId = HttpContext.Session.GetInt32("UserId");
                var existingAdmin = _context.Users.SingleOrDefault(u => u.Userid == adminUserId);
                if (existingAdmin != null)
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
                existingAdmin.FirstName = user.FirstName;
                existingAdmin.LastName = user.LastName;
                existingAdmin.ImagePath = user.ImagePath;
                existingAdmin.Gender = user.Gender;
                _context.Users.Update(existingAdmin);
                _context.SaveChanges();
               


                var login = _context.UserLogins.Where(x => x.Userid == adminUserId).SingleOrDefault();
                login.Username = userLogin.Username;
                login.Password = userLogin.Password;

                _context.UserLogins.Update(login);
                _context.SaveChanges();

                return RedirectToAction("Profile");

                

            }
            return View();
        }


    }
}


