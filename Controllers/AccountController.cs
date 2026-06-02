using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineStore.Models;
using System.Security.Cryptography;
using System.Text;

namespace OnlineStore.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        // ── Register ───────────────────────────────────
        public IActionResult Register()
        {
            if (HttpContext.Session.GetString("UserLoggedIn") == "true")
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string fullName, string email, string password, string confirmPassword)
        {
            if (password != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match.";
                return View();
            }

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (existingUser != null)
            {
                ViewBag.Error = "An account with this email already exists.";
                return View();
            }

            var user = new User
            {
                FullName     = fullName,
                Email        = email,
                PasswordHash = HashPassword(password),
                CreatedAt    = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            HttpContext.Session.SetString("UserLoggedIn", "true");
            HttpContext.Session.SetString("UserName", user.FullName);
            HttpContext.Session.SetString("UserEmail", user.Email);

            TempData["Success"] = "Account created successfully! Welcome!";
            return RedirectToAction("Index", "Home");
        }

        // ── Login ──────────────────────────────────────
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("UserLoggedIn") == "true")
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.PasswordHash == HashPassword(password));

            if (user == null)
            {
                ViewBag.Error = "Invalid email or password.";
                return View();
            }

            HttpContext.Session.SetString("UserLoggedIn", "true");
            HttpContext.Session.SetString("UserName", user.FullName);
            HttpContext.Session.SetString("UserEmail", user.Email);

            TempData["Success"] = $"Welcome back, {user.FullName}!";
            return RedirectToAction("Index", "Home");
        }

        // ── Logout ─────────────────────────────────────
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("UserLoggedIn");
            HttpContext.Session.Remove("UserName");
            HttpContext.Session.Remove("UserEmail");
            return RedirectToAction("Index", "Home");
        }

        // ── Change Password ────────────────────────────
        public IActionResult ChangePassword()
        {
            if (HttpContext.Session.GetString("UserLoggedIn") != "true")
            {
                TempData["Error"] = "You must be logged in to change your password.";
                return RedirectToAction("Login");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            if (HttpContext.Session.GetString("UserLoggedIn") != "true")
                return RedirectToAction("Login");

            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "New passwords do not match.";
                return View();
            }

            if (newPassword.Length < 6)
            {
                ViewBag.Error = "New password must be at least 6 characters.";
                return View();
            }

            var email = HttpContext.Session.GetString("UserEmail");
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.PasswordHash == HashPassword(currentPassword));

            if (user == null)
            {
                ViewBag.Error = "Current password is incorrect.";
                return View();
            }

            user.PasswordHash = HashPassword(newPassword);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Password changed successfully!";
            return RedirectToAction("Index", "Home");
        }

        // ── My Orders ──────────────────────────────────
        public async Task<IActionResult> MyOrders()
        {
            if (HttpContext.Session.GetString("UserLoggedIn") != "true")
                return RedirectToAction("Login");

            var email = HttpContext.Session.GetString("UserEmail");
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.CustomerEmail == email)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }
    }
}