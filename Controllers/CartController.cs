using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineStore.Models;
using System.Text.Json;

namespace OnlineStore.Controllers
{
    public class CartController : Controller
    {
        private readonly AppDbContext _context;
        private const string CartKey = "Cart";

        public CartController(AppDbContext context)
        {
            _context = context;
        }

        private List<CartItem> GetCart()
        {
            var json = HttpContext.Session.GetString(CartKey);
            if (string.IsNullOrEmpty(json))
                return new List<CartItem>();
            return JsonSerializer.Deserialize<List<CartItem>>(json) ?? new List<CartItem>();
        }

        private void SaveCart(List<CartItem> cart)
        {
            HttpContext.Session.SetString(CartKey, JsonSerializer.Serialize(cart));
        }

        // ── Cart Index ─────────────────────────────────
        public IActionResult Index()
        {
            var cart = GetCart();
            return View(cart);
        }

        // ── Add to Cart ────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Add(int productId, int quantity)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                return NotFound();

            if (product.Stock < quantity)
            {
                TempData["Error"] = $"Sorry! Only {product.Stock} items available in stock.";
                return RedirectToAction("Detail", "Products", new { id = productId });
            }

            var cart = GetCart();
            var existing = cart.FirstOrDefault(c => c.ProductId == productId);

            if (existing != null)
            {
                if (existing.Quantity + quantity > product.Stock)
                {
                    TempData["Error"] = $"Sorry! Only {product.Stock} items available in stock.";
                    return RedirectToAction("Detail", "Products", new { id = productId });
                }
                existing.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductId   = product.Id,
                    ProductName = product.Name,
                    UnitPrice   = product.Price,
                    Quantity    = quantity,
                    ImageUrl    = product.ImageUrl
                });
            }

            SaveCart(cart);
            TempData["Success"] = $"{product.Name} added to cart!";
            return RedirectToAction("Index", "Cart");
        }

        // ── Remove from Cart ───────────────────────────
        [HttpPost]
        public IActionResult Remove(int productId)
        {
            var cart = GetCart();
            cart.RemoveAll(c => c.ProductId == productId);
            SaveCart(cart);
            return RedirectToAction("Index");
        }

        // ── Checkout Page ──────────────────────────────
        public IActionResult Checkout()
        {
            var cart = GetCart();
            if (!cart.Any())
                return RedirectToAction("Index");

            ViewBag.UserName  = HttpContext.Session.GetString("UserName") ?? "";
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail") ?? "";

            return View(cart);
        }

        // ── Place Order → Go to Payment ────────────────
        [HttpPost]
        public async Task<IActionResult> PlaceOrder(string customerName, string customerEmail, string address)
        {
            var cart = GetCart();
            if (!cart.Any())
                return RedirectToAction("Index");

            if (string.IsNullOrEmpty(customerName) ||
                string.IsNullOrEmpty(customerEmail) ||
                string.IsNullOrEmpty(address))
            {
                TempData["Error"] = "Please fill all fields.";
                return RedirectToAction("Checkout");
            }

            // Check stock for all items
            foreach (var cartItem in cart)
            {
                var product = await _context.Products.FindAsync(cartItem.ProductId);
                if (product == null || product.Stock < cartItem.Quantity)
                {
                    TempData["Error"] = $"Sorry! {cartItem.ProductName} does not have enough stock.";
                    return RedirectToAction("Checkout");
                }
            }

            // Get UserId if logged in
            int? userId = null;
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (!string.IsNullOrEmpty(userEmail))
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
                if (user != null) userId = user.Id;
            }

            // Create order with Unpaid status
            var order = new Order
            {
                OrderDate     = DateTime.Now,
                Status        = "Pending",
                PaymentStatus = "Unpaid",
                PaymentMethod = "Card",
                TotalPrice    = cart.Sum(c => c.Total),
                CustomerName  = customerName,
                CustomerEmail = customerEmail,
                Address       = address,
                UserId        = userId,
                OrderItems    = cart.Select(c => new OrderItem
                {
                    ProductId = c.ProductId,
                    Quantity  = c.Quantity,
                    UnitPrice = c.UnitPrice
                }).ToList()
            };

            _context.Orders.Add(order);

            // Reduce stock
            foreach (var cartItem in cart)
            {
                var product = await _context.Products.FindAsync(cartItem.ProductId);
                if (product != null)
                {
                    product.Stock -= cartItem.Quantity;
                    _context.Products.Update(product);
                }
            }

            await _context.SaveChangesAsync();
            SaveCart(new List<CartItem>());

            // Go to Payment page
            return RedirectToAction("Payment", "Cart", new { orderId = order.Id });
        }

        // ── Payment Page ───────────────────────────────
        public async Task<IActionResult> Payment(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return RedirectToAction("Index", "Home");

            return View(order);
        }

        // ── Process Payment ────────────────────────────
        [HttpPost]
        public async Task<IActionResult> ProcessPayment(int orderId, string cardNumber,
            string cardHolder, string expiryDate, string cvv)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                return RedirectToAction("Index", "Home");

            // Fake validation
            if (string.IsNullOrEmpty(cardNumber) ||
                string.IsNullOrEmpty(cardHolder) ||
                string.IsNullOrEmpty(expiryDate) ||
                string.IsNullOrEmpty(cvv))
            {
                TempData["Error"] = "Please fill all payment fields.";
                return RedirectToAction("Payment", new { orderId });
            }

            // Simulate payment success
            order.PaymentStatus = "Paid";
            order.PaymentMethod = "Card";
            order.Status        = "Processing";
            await _context.SaveChangesAsync();

            return RedirectToAction("Confirmation", "Cart", new { orderId = order.Id });
        }

        // ── Order Confirmation ─────────────────────────
        public async Task<IActionResult> Confirmation(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return RedirectToAction("Index", "Home");

            return View(order);
        }
    }
}