using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using She_He_Store.Models;
using System.Security.Claims;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace She_He_Store.Controllers
{
    public class LoginAndRegisterController : Controller
    {

		private readonly ModelContext _context;
		private readonly IWebHostEnvironment _webHostEnvironment;//declare variable
		private readonly ILogger<LoginAndRegisterController> _logger;

		public LoginAndRegisterController(ILogger<LoginAndRegisterController> logger , ModelContext context, IWebHostEnvironment webHostEnvironment)
		{
			_logger = logger;

			_context = context;
			_webHostEnvironment = webHostEnvironment;//dependancy injection
		}

		public async Task<IActionResult>  Logout()
		{
			HttpContext.Session.Clear();


			return RedirectToAction("Index", "Home");
		}


		public IActionResult Register()
        {
            return View();
        }

        public IActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            
            return View();
        }

		
		[HttpPost]
		public async Task<IActionResult> Login([Bind("Username,Password")] UserLogin userlogin , string returnUrl)
		{
			//to ensure that no duplicate records are created
			var auth = _context.UserLogins.Where(
				x => x.Username == userlogin.Username && x.Password == userlogin.Password).SingleOrDefault();

			//values exist 
			if (auth != null)
			{


                var claims = new List<Claim>
               {
            new Claim(ClaimTypes.Name, auth.Username),
            new Claim(ClaimTypes.Role, auth.Roleid.ToString()),
             };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true, 
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(60)
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);


                HttpContext.Session.SetInt32("UserId", (int)auth.Userid);

				 if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
				{
					//Debug.WriteLine($"Redirecting to returnUrl: {returnUrl}");
					// Redirect to the returnUrl if it's a local URL
					_logger.LogInformation($"Redirecting to {returnUrl}");
					return Redirect(returnUrl);
				}
				Response.Cookies.Delete("returnUrl");

                switch (auth.Roleid)
				{
					//save info
					//1. Admin 
					case 1:
						

						HttpContext.Session.SetString("AdminName", (auth.Username));
						HttpContext.Session.SetInt32("UserId", (int)auth.Userid);
                        HttpContext.Session.SetInt32("AdminId", (int)auth.Userid);


                        //HttpContext.Session.SetString("AdminName", auth.User.FirstName + " " + auth.User.LastName);


                        var user = _context.Users.Where(x => x.Userid == auth.Userid).SingleOrDefault();
                        //HttpContext.Session.SetString("AdminImagePath", user.ImagePath);
                        ViewBag.AdminName = user.FirstName + " " + user.LastName;
                        ViewBag.AdminImagePath = user.ImagePath;
						TempData["ImagePath"] = user.ImagePath;
						TempData["Name"] = user.FirstName  +" " + user.LastName;
                        return RedirectToAction("Dashboard", "Admin");


					//2. user 
					case 2:

					
						HttpContext.Session.SetInt32("UserId", (int)auth.Userid);
						return RedirectToAction("Index", "Home" );

						//3. employee..


				}

			}
			ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(userlogin);
        }




		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Register([Bind("Userid,FirstName,LastName,Gender,ImageFile")] User user, string userName, string password)
		{
			if (ModelState.IsValid)
			{
				
				//add customer details
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

				_context.Add(user);
				await _context.SaveChangesAsync();

				//add user login details
				UserLogin login = new UserLogin();
				login.Roleid = 2;
				login.Username = userName;
				login.Password = password;
				login.Userid = user.Userid;
				_context.Add(login);
				await _context.SaveChangesAsync();



				return RedirectToAction("Login");
			}
			return View("Register");
		}


	}
}

