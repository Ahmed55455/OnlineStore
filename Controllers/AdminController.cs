using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineStore.Models;
using System.Security.Cryptography;
using System.Text;

namespace OnlineStore.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        private bool IsAdminLoggedIn()
        {
            return HttpContext.Session.GetString("AdminLoggedIn") == "true";
        }

        // ── Login ──────────────────────────────────────
        public IActionResult Login()
        {
            if (IsAdminLoggedIn())
                return RedirectToAction("Dashboard");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var admin = await _context.Admins
                .FirstOrDefaultAsync(a => a.Username == username
                                    && a.PasswordHash == HashPassword(password));

            if (admin != null)
            {
                HttpContext.Session.SetString("AdminLoggedIn", "true");
                HttpContext.Session.SetString("AdminUsername", admin.Username);
                return RedirectToAction("Dashboard");
            }

            ViewBag.Error = "Invalid username or password.";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("AdminLoggedIn");
            HttpContext.Session.Remove("AdminUsername");
            return RedirectToAction("Index", "Home");
        }

        // ── Dashboard ──────────────────────────────────
        public async Task<IActionResult> Dashboard()
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            ViewBag.TotalProducts   = await _context.Products.CountAsync();
            ViewBag.TotalCategories = await _context.Categories.CountAsync();
            ViewBag.TotalOrders     = await _context.Orders.CountAsync();
            ViewBag.TotalRevenue    = await _context.Orders.SumAsync(o => o.TotalPrice);

            var recentOrders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync();

            return View(recentOrders);
        }

        // ── Products ───────────────────────────────────
        public async Task<IActionResult> Products()
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            var products = await _context.Products
                .Include(p => p.Category)
                .ToListAsync();

            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(products);
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct(Product product)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            if (string.IsNullOrEmpty(product.Description))
                product.Description = "No description available.";

            if (string.IsNullOrEmpty(product.ImageUrl))
                product.ImageUrl = "https://placehold.co/400x300?text=No+Image";

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Product added successfully!";
            return RedirectToAction("Products");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Product deleted successfully!";
            }
            return RedirectToAction("Products");
        }

        public async Task<IActionResult> EditProduct(int id)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> EditProduct(Product product)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            if (string.IsNullOrEmpty(product.Description))
                product.Description = "No description available.";

            if (string.IsNullOrEmpty(product.ImageUrl))
                product.ImageUrl = "https://placehold.co/400x300?text=No+Image";

            _context.Products.Update(product);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Product updated successfully!";
            return RedirectToAction("Products");
        }

        // ── Categories ─────────────────────────────────
        public async Task<IActionResult> Categories()
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            var categories = await _context.Categories
                .Include(c => c.Products)
                .ToListAsync();

            return View(categories);
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory(string name)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            if (string.IsNullOrEmpty(name))
            {
                TempData["Error"] = "Category name cannot be empty.";
                return RedirectToAction("Categories");
            }

            var existing = await _context.Categories
                .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());

            if (existing != null)
            {
                TempData["Error"] = "Category already exists.";
                return RedirectToAction("Categories");
            }

            _context.Categories.Add(new Category { Name = name });
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Category '{name}' added successfully!";
            return RedirectToAction("Categories");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                TempData["Error"] = "Category not found.";
                return RedirectToAction("Categories");
            }

            if (category.Products.Any())
            {
                TempData["Error"] = $"Cannot delete '{category.Name}' — it has {category.Products.Count} product(s). Delete the products first.";
                return RedirectToAction("Categories");
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Category '{category.Name}' deleted successfully!";
            return RedirectToAction("Categories");
        }

        [HttpPost]
        public async Task<IActionResult> EditCategory(int id, string name)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                TempData["Error"] = "Category not found.";
                return RedirectToAction("Categories");
            }

            category.Name = name;
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Category updated to '{name}' successfully!";
            return RedirectToAction("Categories");
        }

        // ── Orders ─────────────────────────────────────
        public async Task<IActionResult> Orders()
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string status)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            var order = await _context.Orders.FindAsync(orderId);
            if (order != null)
            {
                order.Status = status;
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Order #{orderId} status updated to {status}";
            }
            return RedirectToAction("Orders");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteOrder(int orderId)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order != null)
            {
                _context.OrderItems.RemoveRange(order.OrderItems);
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Order #{orderId} deleted successfully!";
            }
            return RedirectToAction("Orders");
        }

        // ── Change Password ────────────────────────────
        public IActionResult ChangePassword()
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            if (!IsAdminLoggedIn())
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

            var username = HttpContext.Session.GetString("AdminUsername");
            var admin = await _context.Admins
                .FirstOrDefaultAsync(a => a.Username == username
                                    && a.PasswordHash == HashPassword(currentPassword));

            if (admin == null)
            {
                ViewBag.Error = "Current password is incorrect.";
                return View();
            }

            admin.PasswordHash = HashPassword(newPassword);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Password changed successfully!";
            return RedirectToAction("Dashboard");
        }
    }
}