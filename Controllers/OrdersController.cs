using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using She_He_Store.Models;
using MailKit.Net.Smtp;
using MimeKit;


namespace She_He_Store.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ModelContext _context;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(ModelContext context, ILogger<OrdersController> logger)
        {
            _context = context;
            _logger = logger;

        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            var modelContext = _context.Orders.Include(o => o.IdNavigation).Include(o => o.User);
            return View(await modelContext.ToListAsync());
        }



        private async Task SendOrderDeliveredEmail(Order order)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var user = _context.Users.FirstOrDefault(u => u.Userid == userId);
            var userlogins = _context.UserLogins.Where(x => x.Userid == order.Userid).Include(c => c.User).SingleOrDefault();

            try
            {

                MimeMessage message = new MimeMessage();
                MailboxAddress from = new MailboxAddress("She_He_Shop", "eslamalshqeirat1@gmail.com");
                message.From.Add(from);
                MailboxAddress to = new MailboxAddress(user.FirstName, userlogins.Username);
                message.To.Add(to);
                message.Subject = "Order Delivered";
                BodyBuilder builder = new BodyBuilder();
                builder.HtmlBody = $"<h3> Hello, {user.FirstName} <h3>" +
                                  "<p>Your order has been delivered!<p>" +
                                  "<p>Thank you for shopping with us.We hope you enjoy your products!<p>" +
                                  "<h5>Best Regards<h5>"; message.Body = builder.ToMessageBody();
                using (var clinte = new SmtpClient())
                {
                    clinte.Connect("smtp.gmail.com", 465, true);
                    clinte.Authenticate("eslamalshqeirat1@gmail.com", "gijfkvsqruewheoi");
                    clinte.Send(message);
                    clinte.Disconnect(true);

                }

            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Error");


            }
        }

            // GET: Orders/Details/5
            public async Task<IActionResult> Details(decimal? id)
        {
            if (id == null || _context.Orders == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.IdNavigation)
                .Include(o => o.User)
                .FirstOrDefaultAsync(m => m.Orderid == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Orders/Create
        public IActionResult Create()
        {
            ViewData["Id"] = new SelectList(_context.Shippings, "Id", "Id");
            ViewData["Userid"] = new SelectList(_context.Users, "Userid", "Userid");
            return View();
        }

        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Orderid,Orderdate,Totalamount,Orderstatus,Userid,Id")] Order order)
        {
            if (ModelState.IsValid)
            {
                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Id"] = new SelectList(_context.Shippings, "Id", "Id", order.Id);
            ViewData["Userid"] = new SelectList(_context.Users, "Userid", "Userid", order.Userid);
            return View(order);
        }




        [HttpGet]
        public async Task<IActionResult> Edit(decimal? id)
        {
            if (id == null || _context.Orders == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.IdNavigation)
                .Include(o => o.User)
                .FirstOrDefaultAsync(m => m.Orderid == id);

            if (order == null)
            {
                return NotFound();
            }

            var orderList = new OrderList
            {
                Orderid = order.Orderid,
                Orderdate = (DateTime)order.Orderdate,
                Totalamount = order.Totalamount,
                Orderstatus = order.Orderstatus,
                Userid = (decimal)order.Userid,
                Id = (decimal)order.Id
            };

            return View(orderList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(decimal id, [Bind("Orderid,Orderdate,Totalamount,Orderstatus,Userid,Id")] OrderList orderList)
        {
            if (id != orderList.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var order = await _context.Orders.FindAsync(id);
                    if (order != null)
                    {
                        order.Orderstatus = orderList.Orderstatus;
                        _context.Update(order);
                        await _context.SaveChangesAsync();

                        if (order.Orderstatus == "Delivered")
                        {
                            await SendOrderDeliveredEmail(order);
                        }
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(orderList.Orderid))
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

            return View(orderList);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(decimal? id)
        {
            if (id == null || _context.Orders == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.IdNavigation)
                .Include(o => o.User)
                .FirstOrDefaultAsync(m => m.Orderid == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(decimal id)
        {
            if (_context.Orders == null)
            {
                return Problem("Entity set 'ModelContext.Orders'  is null.");
            }
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(decimal id)
        {
          return (_context.Orders?.Any(e => e.Orderid == id)).GetValueOrDefault();
        }
    }
}
