using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using She_He_Store.Models;
using MailKit.Net.Smtp;
using System.Transactions;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using System.IO;
using MimeKit;
using MailKit.Net.Smtp;
using System.Text;
using iText.Kernel.Geom;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf.draw;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;



namespace She_He_Store.Controllers
{
	public class ShoppingcartsController : Controller
	{
		private readonly ModelContext _context;
		private readonly IWebHostEnvironment _webHostEnvironment;//declare variable

		private readonly ILogger<ShoppingcartsController> _logger;
		public ShoppingcartsController(ModelContext context, IWebHostEnvironment webHostEnvironment, ILogger<ShoppingcartsController> logger)
		{
			_context = context;
			_webHostEnvironment = webHostEnvironment;
			_logger = logger;
		}
		// GET: Shoppingcarts
		public async Task<IActionResult> Index()
		{
			var modelContext = _context.Shoppingcarts.Include(s => s.Coupon).Include(s => s.Product).Include(s => s.User);
			return View(await modelContext.ToListAsync());
		}






		public IActionResult MyCart()
		{
			var userId = HttpContext.Session.GetInt32("UserId");

			var cart = _context.Shoppingcarts.Where(cart => cart.Userid == userId).ToList();
			var product = _context.Products.ToList();

			var productCart = from c in cart
							  join p in product
							  on c.Productid equals p.Productid
							  select new ProductCart
							  {
								  Shoppingcarts = c,
								  Product = p
							  };
		    decimal? subTotal = 0;



			foreach (var item in productCart)
			{
				var regularPrice = item.Product.Price;
				var salePrice = regularPrice;

				if (item.Product.Sale.HasValue && item.Product.Sale > 0)
				{
					var discount = item.Product.Sale.Value / 100M;
					salePrice = regularPrice - (regularPrice * discount);
				}

				subTotal += salePrice * item.Shoppingcarts.Quantity;
			}
			ViewBag.subTotal = subTotal;

			if (productCart != null)
			{
				return View(productCart);
			}
			else return View();

		}
		[HttpPost]
		public IActionResult ApplyCoupon(string couponCode)
		{
			Coupon coupon = _context.Coupons.FirstOrDefault(c => c.Couponcode == couponCode);

			if (coupon == null)
			{
				return BadRequest(new { message = "Coupon is not valid." });
			}


			decimal discount = coupon.Discountamount;

			var userId = HttpContext.Session.GetInt32("UserId");
			var cartItems = _context.Shoppingcarts
				.Where(item => item.Userid == userId)
				.Include(item => item.Product)
				.ToList();

			var subtotal = cartItems.Sum(item => item.Product.Price * item.Quantity);
			var newSubtotal = subtotal - discount;

			// Update the ViewBag with the new subtotal.
			ViewBag.Subtotal = newSubtotal;

			// Return coupon details and discount.
			return Ok(new { details = "Coupon Applied: " + coupon.Couponcode, discount = discount, newSubtotal = newSubtotal });
		}


		[HttpPost]
		public async Task<IActionResult> AddProductsToCart(int id, int quantity)
		{
			
				// Get user id
				
			try
			{

				var userId = HttpContext.Session.GetInt32("UserId");
				//if (userId == null)
				//{
				//	return Json(new { success = false, message = "User is not authenticated" });
				//}
				if (userId == null)
				{
					var ReturnUrl = Url.Action("GetProduct", "Products", new { id = id });
					return Json(new { redirect = Url.Action("Login", "LoginAndRegister", new { returnUrl = ReturnUrl }) });
				}

				// Check if the item is already in the cart.
				var existingCartItem = await _context.Shoppingcarts
					.SingleOrDefaultAsync(c => c.Userid == userId.Value && c.Productid == id);

				if (existingCartItem == null)
				{
					// If the item is not in the cart, create a new cart item.
					var cartItem = new Shoppingcart
					{
						Userid = userId.Value,
						Productid = id,
						Quantity = quantity
					};

					_context.Shoppingcarts.Add(cartItem);
				}
				else
				{
					// If the item is already in the cart, update the quantity.
					existingCartItem.Quantity += quantity;
				}

				await _context.SaveChangesAsync();

				// Return a JSON response indicating success
				return Json(new { success = true , message= "is added to cart successfully!" });
			}
			catch (Exception ex)
			{
				// Log the exception message and stack trace.
				_logger.LogError(ex, "Error adding product to cart");
				return Json(new { success = false, message = "Error adding product to cart." });
			}
		}



		[HttpPost]
		public IActionResult UpdateQuantity(int cartItemId, int newQuantity)
		{
			var cartItem = _context.Shoppingcarts.FirstOrDefault(c => c.Cartid == cartItemId);

			if (cartItem == null)
			{
				return NotFound();
			}

			cartItem.Quantity = newQuantity;
			_context.SaveChanges();

			// Calculate new subtotal
			var userId = HttpContext.Session.GetInt32("UserId");
			var cartItems = _context.Shoppingcarts
				.Where(item => item.Userid == userId)
				.Include(item => item.Product)
				.ToList();

			var subtotal = cartItems.Sum(item =>
			{
				var regularPrice = item.Product.Price;
				var salePrice = regularPrice;

				if (item.Product.Sale.HasValue && item.Product.Sale > 0)
				{
					var discount = item.Product.Sale.Value / 100M;
					salePrice = regularPrice - (regularPrice * discount);
				}

				return salePrice * item.Quantity;
			});

			return Json(new { success = true, newSubtotal = subtotal });
		}

		[HttpPost]
		public IActionResult DeleteItem(int cartItemId)
		{

			var cartItem = _context.Shoppingcarts.FirstOrDefault(c => c.Cartid == cartItemId);

			if (cartItem == null)
			{
				return NotFound();
			}

			_context.Shoppingcarts.Remove(cartItem);
			_context.SaveChanges();

			// Calculate new subtotal
			var userId = HttpContext.Session.GetInt32("UserId");
			var cartItems = _context.Shoppingcarts
				.Where(item => item.Userid == userId)
				.Include(item => item.Product)
				.ToList();

			var subtotal = cartItems.Sum(item =>
			{
				var regularPrice = item.Product.Price;
				var salePrice = regularPrice;

				if (item.Product.Sale.HasValue && item.Product.Sale > 0)
				{
					var discount = item.Product.Sale.Value / 100M;
					salePrice = regularPrice - (regularPrice * discount);
				}

				return salePrice * item.Quantity;
			});

			return Json(new { success = true, newSubtotal = subtotal });
		}



		public IActionResult Checkout()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Checkout([Bind("Id,Shippingcountry,Shippingcity,Shippingpostalcode,Phonenumber,Email")] Shipping shippingInfo , string namex, int cardNumber,  int cvv , string expirationDate) 
		{
            var userId = HttpContext.Session.GetInt32("UserId");

            //unauthenticated users then redirect to login page
            if (userId == null)
            {
                return RedirectToAction("Login", "LoginAndRegister");
            }

            var user = _context.Users.FirstOrDefault(u => u.Userid == userId);

			
            var paymentCard = _context.Paymentcards.FirstOrDefault(card => card.Cardnumber == cardNumber
                                                      && card.Name == namex
                                                      && card.Cvv == cvv
                                                      && card.ExpirationDate == expirationDate);


            if (paymentCard == null)
            {
                TempData["ErrorMessage"] = "Card information is incorrect.";
                return View();
            }
            var cart= _context.Shoppingcarts
                    .Where(item => item.Userid == userId)
                    .Include(item => item.Product)
                    .ToList();

            var subtotal = cart.Sum(item =>
            {
                var regularPrice = item.Product.Price;
                var salePrice = regularPrice;

                if (item.Product.Sale.HasValue && item.Product.Sale > 0)
                {
                    var discount = item.Product.Sale.Value / 100M;
                    salePrice = regularPrice - (regularPrice * discount);
                }

                return salePrice * item.Quantity;
            });


            if (paymentCard.Balance < subtotal)
            {
                TempData["ErrorMessage"] = "Insufficient balance to complete the purchase.";
				return View();
            }


            Shipping addressInfo = new Shipping();
            addressInfo.Email = shippingInfo.Email;
            addressInfo.Shippingcountry = shippingInfo.Shippingcountry;
            addressInfo.Shippingcity = shippingInfo.Shippingcity;
            addressInfo.Shippingpostalcode = shippingInfo.Shippingpostalcode;
            addressInfo.Phonenumber = shippingInfo.Phonenumber;

            _context.Shippings.Add(addressInfo);
             _context.SaveChanges();


			Order order = new Order();
			order.Userid = userId;
			order.Orderstatus = "Pending";
			order.Orderdate = DateTime.Now;
            order.Totalamount = (decimal)subtotal;
			order.IdNavigation = addressInfo;
           
			_context.Orders.Add(order);
			_context.SaveChanges();

          


            var userLogins = _context.UserLogins.FirstOrDefault(u => u.Userid == userId);
            MimeMessage message = new MimeMessage();
            MailboxAddress from = new MailboxAddress("She_He_Shop", "eslamalshqeirat1@gmail.com");
            message.From.Add(from);
            MailboxAddress to = new MailboxAddress(user.FirstName, userLogins.Username);
            message.To.Add(to);
            message.Subject = "Order Confirmation";
            BodyBuilder builder = new BodyBuilder();
			builder.HtmlBody = "Thank you!" +
							  $"Your order (Order ID: {order.Orderid}) has been successfully received. Thank you." +
							  "Best Regards"; message.Body = builder.ToMessageBody();

            using (var clinte = new SmtpClient())
            {
                clinte.Connect("smtp.gmail.com", 465, true);
                clinte.Authenticate("eslamalshqeirat1@gmail.com", "gijfkvsqruewheoi");
                clinte.Send(message);
                clinte.Disconnect(true);
            }

            paymentCard.Balance -= subtotal;


            foreach (var cartItem in cart)
            {
                var product = _context.Products.Where(c => c.Productid == cartItem.Productid).SingleOrDefault();

				Orderitem orderItem = new Orderitem();

				orderItem.Orderid = order.Orderid;
				orderItem.Productid = cartItem.Productid;
				orderItem.Quantity = (decimal)cartItem.Quantity;
				orderItem.Price = (decimal)subtotal;
                product.Stockquantity -= (decimal)cartItem.Quantity;


                if (product.Stockquantity == 0)
                {
                    product.Status = "Out of Stock";
                    var carts = _context.Shoppingcarts.ToList();
                    foreach (var cartitems in carts)
                    {
                        if (cartitems.Productid == product.Productid && cartitems.Userid != userId)
                        {
                            _context.Shoppingcarts.Remove(cartitems);
                            await _context.SaveChangesAsync();
                        }
                    }
                }
                else
                {
                    var carts = _context.Shoppingcarts.ToList();
                    foreach (var cartitems in carts)
                    {
                        if (cartitems.Productid == product.Productid && cartitems.Userid != userId)
                        {
                            if (cartitems.Quantity > product.Stockquantity)
                            {
                                cartitems.Quantity = product.Stockquantity;
                                _context.Shoppingcarts.Update(cartitems);
                                await _context.SaveChangesAsync();
                            }

                        }
                    }
                }
                _context.Products.Update(product);
                _context.Orderitems.Add(orderItem);
            }
            _context.SaveChanges();


			
			// Remove items from  cart
			_context.Shoppingcarts.RemoveRange(cart);
            _context.SaveChanges();
		
			return Json(new { success = true, message = "Order is placed successfully." });
            //  return View("CheckOut");
        }

        private async Task SendEmailWithPdf(User user, Order order, byte[] pdfBytes)
        {

            var userId = HttpContext.Session.GetInt32("UserId");
            var userLogins = _context.UserLogins.FirstOrDefault(u => u.Userid == userId);

            // Create and configure the email message
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("She_He_Shop", "eslamalshqeirat1@gmail.com"));
            message.To.Add(new MailboxAddress(user.FirstName, userLogins.Username));
            message.Subject = "Order Confirmation";

            // Create the email body
            var builder = new BodyBuilder
            {
                HtmlBody = $"<h3>Thank you!</h3><p>Your order (Order ID: {order.Orderid}) has been successfully received. Thank you.</p><h5>Best Regards</h5>"
            };

            // Attach the PDF
            var attachment = new MimePart("application", "pdf")
            {
                Content = new MimeContent(new MemoryStream(pdfBytes)),
                ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                ContentTransferEncoding = ContentEncoding.Base64,
                FileName = $"Order_{order.Orderid}.pdf"
            };
            builder.Attachments.Add(attachment);
            message.Body = builder.ToMessageBody();

            // Send the email
            using (var client = new SmtpClient())
            {
                client.Connect("smtp.gmail.com", 465, true);
                client.Authenticate("eslamalshqeirat1@gmail.com", "yourpassword");
                await client.SendAsync(message);
                client.Disconnect(true);
            }
        }


        private string GeneratePdf(Order order, List<Shoppingcart> cartItems)
        {
            string tempFilePath = System.IO.Path.GetTempFileName();

            using (var writer = new iText.Kernel.Pdf.PdfWriter(tempFilePath))
            {
                using (var pdf = new iText.Kernel.Pdf.PdfDocument(writer))
                {
                    var document = new iText.Layout.Document(pdf);

                    // Add a title for the document
                    document.Add(new iText.Layout.Element.Paragraph("Order Details")
                                 .SetFontSize(16)
                                 .SetBold());

                    // Add order ID
                    document.Add(new iText.Layout.Element.Paragraph($"Order ID: {order.Orderid}")
                                 .SetFontSize(12));

                    // Add a line break
                    document.Add(new iText.Layout.Element.Paragraph("\n"));

                    // Add headings for the cart items
                    document.Add(new iText.Layout.Element.Paragraph("Item Name | Quantity | Price")
                                 .SetBold());

                    // Loop through each cart item and add details
                    foreach (var item in cartItems)
                    {
                        string itemName = item.Product.Productname; // Replace with actual property
                        int quantity = (int)item.Quantity;
                        decimal price = (decimal)item.Product.Price; // Replace with actual property

                        document.Add(new iText.Layout.Element.Paragraph($"{itemName} | {quantity} | ${price}"));
                    }

                    // Add a line break
                    document.Add(new iText.Layout.Element.Paragraph("\n"));

                    // Add total amount
                    decimal totalAmount = order.Totalamount; // Replace with actual property
                    document.Add(new iText.Layout.Element.Paragraph($"Total Amount: ${totalAmount}")
                                 .SetBold());

                    document.Close();
                }
            }

            return tempFilePath;
        }
        public IActionResult ViewCart()
		{
			var userId = HttpContext.Session.GetInt32("UserId");
			var cartItems = _context.Shoppingcarts
				.Where(item => item.Userid == userId)
				.Include(item => item.Product)
				.ToList();

			return View(cartItems);
		}


		private string GenerateInvoiceContent(Order order, List<Shoppingcart> cartItems)
		{
			var invoiceBuilder = new StringBuilder();

			// Add a title for the document
			invoiceBuilder.AppendLine("Order Details");
			invoiceBuilder.AppendLine($"Order ID: {order.Orderid}");

			// Loop through each cart item and add details
			foreach (var item in cartItems)
			{
				string itemName = item.Product.Productname; // Replace with actual property
				int quantity = (int)item.Quantity;
				decimal price = (decimal)item.Product.Price; // Replace with actual property
				invoiceBuilder.AppendLine($"Item Name: {itemName}, Quantity: {quantity}, Price: ${price}");
			}

			// Add total amount
			decimal totalAmount = order.Totalamount; // Replace with actual property
			invoiceBuilder.AppendLine($"Total Amount: ${totalAmount}");

			return invoiceBuilder.ToString();

		}




        public byte[] GeneratePdfInvoice(Order order, List<Shoppingcart> cartItems)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                // Initialize PDF writer
                iText.Kernel.Pdf.PdfWriter writer = new iText.Kernel.Pdf.PdfWriter(stream);
                // Initialize PDF document
                iText.Kernel.Pdf.PdfDocument pdf = new iText.Kernel.Pdf.PdfDocument(writer);
                // Initialize document
                iText.Layout.Document document = new iText.Layout.Document(pdf);

                // Use the GenerateInvoiceContent method to get the invoice text
                string invoiceContent = GenerateInvoiceContent(order, cartItems);

                // Add the content to the PDF
                document.Add(new iText.Layout.Element.Paragraph(invoiceContent));

                // Close document
                document.Close();

                // Return the bytes of the PDF document
                return stream.ToArray();
            }
        }
        // GET: Shoppingcarts/Details/5
        public async Task<IActionResult> Details(decimal? id)
		{
			if (id == null || _context.Shoppingcarts == null)
			{
				return NotFound();
			}

			var shoppingcart = await _context.Shoppingcarts
				.Include(s => s.Coupon)
				.Include(s => s.Product)
				.Include(s => s.User)
				.FirstOrDefaultAsync(m => m.Cartid == id);
			if (shoppingcart == null)
			{
				return NotFound();
			}

			return View(shoppingcart);
		}

		// GET: Shoppingcarts/Create
		public IActionResult Create()
		{
			ViewData["Couponid"] = new SelectList(_context.Coupons, "Couponid", "Couponid");
			ViewData["Productid"] = new SelectList(_context.Products, "Productid", "Productid");
			ViewData["Userid"] = new SelectList(_context.Users, "Userid", "Userid");
			return View();
		}

		// POST: Shoppingcarts/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("Cartid,Userid,Productid,Couponid,Quantity")] Shoppingcart shoppingcart)
		{
			if (ModelState.IsValid)
			{
				_context.Add(shoppingcart);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			ViewData["Couponid"] = new SelectList(_context.Coupons, "Couponid", "Couponid", shoppingcart.Couponid);
			ViewData["Productid"] = new SelectList(_context.Products, "Productid", "Productid", shoppingcart.Productid);
			ViewData["Userid"] = new SelectList(_context.Users, "Userid", "Userid", shoppingcart.Userid);
			return View(shoppingcart);
		}

		// GET: Shoppingcarts/Edit/5
		public async Task<IActionResult> Edit(decimal? id)
		{
			if (id == null || _context.Shoppingcarts == null)
			{
				return NotFound();
			}

			var shoppingcart = await _context.Shoppingcarts.FindAsync(id);
			if (shoppingcart == null)
			{
				return NotFound();
			}
			ViewData["Couponid"] = new SelectList(_context.Coupons, "Couponid", "Couponid", shoppingcart.Couponid);
			ViewData["Productid"] = new SelectList(_context.Products, "Productid", "Productid", shoppingcart.Productid);
			ViewData["Userid"] = new SelectList(_context.Users, "Userid", "Userid", shoppingcart.Userid);
			return View(shoppingcart);
		}

		// POST: Shoppingcarts/Edit/5
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(decimal id, [Bind("Cartid,Userid,Productid,Couponid,Quantity")] Shoppingcart shoppingcart)
		{
			if (id != shoppingcart.Cartid)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					_context.Update(shoppingcart);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!ShoppingcartExists(shoppingcart.Cartid))
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
			ViewData["Couponid"] = new SelectList(_context.Coupons, "Couponid", "Couponid", shoppingcart.Couponid);
			ViewData["Productid"] = new SelectList(_context.Products, "Productid", "Productid", shoppingcart.Productid);
			ViewData["Userid"] = new SelectList(_context.Users, "Userid", "Userid", shoppingcart.Userid);
			return View(shoppingcart);
		}

		// GET: Shoppingcarts/Delete/5
		public async Task<IActionResult> Delete(decimal? id)
		{
			if (id == null || _context.Shoppingcarts == null)
			{
				return NotFound();
			}

			var shoppingcart = await _context.Shoppingcarts
				.Include(s => s.Coupon)
				.Include(s => s.Product)
				.Include(s => s.User)
				.FirstOrDefaultAsync(m => m.Cartid == id);
			if (shoppingcart == null)
			{
				return NotFound();
			}

			return View(shoppingcart);
		}

		// POST: Shoppingcarts/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(decimal id)
		{
			if (_context.Shoppingcarts == null)
			{
				return Problem("Entity set 'ModelContext.Shoppingcarts'  is null.");
			}
			var shoppingcart = await _context.Shoppingcarts.FindAsync(id);
			if (shoppingcart != null)
			{
				_context.Shoppingcarts.Remove(shoppingcart);
			}

			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		private bool ShoppingcartExists(decimal id)
		{
			return (_context.Shoppingcarts?.Any(e => e.Cartid == id)).GetValueOrDefault();
		}
	}
}
