# 🛒 OnlineStore

> A full-featured e-commerce web application built with **ASP.NET Core MVC**, **Entity Framework Core**, and **SQLite** — featuring a public storefront, shopping cart, secure checkout, and a powerful admin panel.

---

## ✨ Features

### 🛍️ Public Storefront
- Browse all products with category filtering
- Search products by name, description, or category (case-insensitive)
- View detailed product pages with stock status
- Add products to cart with quantity control
- Checkout with delivery address
- Fake payment page with card validation
- Order confirmation page with full order summary

### 👤 User Account
- Register & Login with encrypted passwords (SHA-256)
- View past orders with status tracking (My Orders)
- Change password securely
- Session-based authentication

### 🔐 Admin Panel
> Access via: `http://localhost:5038/Admin/Login`

- Secure login stored in database (hashed password)
- Dashboard with live stats (products, orders, revenue)
- **Manage Products** — Add, Edit, Delete with image URL support
- **Manage Categories** — Add, Edit, Delete categories dynamically
- **Manage Orders** — View customer info, address, items, update status, delete
- Order status: 🟡 Pending → 🔵 Processing → 🟢 Delivered / 🔴 Cancelled
- Payment status tracking (Paid / Unpaid)
- Change admin password

---

## 🖥️ Pages

| Page | URL | Access |
|------|-----|--------|
| Home | `/` | Public |
| Products | `/Products` | Public |
| Product Detail | `/Products/Detail/{id}` | Public |
| Shopping Cart | `/Cart` | Users only |
| Checkout | `/Cart/Checkout` | Users only |
| Payment | `/Cart/Payment` | Users only |
| Order Confirmation | `/Cart/Confirmation` | Users only |
| Register | `/Account/Register` | Guest |
| Login | `/Account/Login` | Guest |
| My Orders | `/Account/MyOrders` | Users only |
| Change Password | `/Account/ChangePassword` | Users only |
| Admin Login | `/Admin/Login` | Admin |
| Admin Dashboard | `/Admin/Dashboard` | Admin only |
| Manage Products | `/Admin/Products` | Admin only |
| Manage Categories | `/Admin/Categories` | Admin only |
| Manage Orders | `/Admin/Orders` | Admin only |

---

## 🗄️ Database Models

```
Product         → Id, Name, Description, Price, Stock, ImageUrl, CategoryId
Category        → Id, Name, Products[]
Order           → Id, OrderDate, Status, TotalPrice, CustomerName,
                  CustomerEmail, Address, PaymentStatus, PaymentMethod, UserId
OrderItem       → Id, Quantity, UnitPrice, OrderId, ProductId
User            → Id, FullName, Email, PasswordHash, CreatedAt
Admin           → Id, Username, PasswordHash
```

---

## 🛠️ Tech Stack

| Technology | Purpose |
|-----------|---------|
| ASP.NET Core MVC (.NET 10) | Web framework |
| Entity Framework Core | ORM & database management |
| SQLite | Database |
| Razor Views (.cshtml) | Server-side templating |
| Bootstrap 5 | UI styling |
| Bootstrap Icons | Icon library |
| SHA-256 | Password hashing |
| Session | Authentication |

---

## 🚀 Getting Started

### Prerequisites
- [.NET SDK 10](https://dotnet.microsoft.com/download)
- [Visual Studio Code](https://code.visualstudio.com)

### Installation

```bash
# Clone the repository
git clone https://github.com/yourusername/OnlineStore.git
cd OnlineStore

# Install EF Core tools (if not installed)
dotnet tool install --global dotnet-ef

# Install dependencies
dotnet restore

# Apply database migrations
dotnet ef database update

# Run the application
dotnet run
```

Open your browser at: `http://localhost:5038`

---

## 🔑 Default Credentials

### Admin
```
URL:      http://localhost:5038/Admin/Login
Username: admin
Password: admin123
```

### User
Register a new account at `/Account/Register`

---

## 📁 Project Structure

```
OnlineStore/
├── Controllers/
│   ├── HomeController.cs
│   ├── ProductsController.cs
│   ├── CartController.cs
│   ├── AccountController.cs
│   └── AdminController.cs
├── Models/
│   ├── Product.cs
│   ├── Category.cs
│   ├── Order.cs
│   ├── OrderItem.cs
│   ├── CartItem.cs
│   ├── User.cs
│   ├── Admin.cs
│   └── AppDbContext.cs
├── Views/
│   ├── Home/          → Index.cshtml
│   ├── Products/      → Index.cshtml, Detail.cshtml
│   ├── Cart/          → Index.cshtml, Checkout.cshtml,
│   │                    Payment.cshtml, Confirmation.cshtml
│   ├── Account/       → Login.cshtml, Register.cshtml,
│   │                    ChangePassword.cshtml, MyOrders.cshtml
│   ├── Admin/         → Login.cshtml, Dashboard.cshtml,
│   │                    Products.cshtml, Categories.cshtml,
│   │                    Orders.cshtml, ChangePassword.cshtml,
│   │                    EditProduct.cshtml
│   └── Shared/        → _Layout.cshtml
├── wwwroot/           → CSS, JS, Images
├── Program.cs
├── appsettings.json
└── store.db
```

---

## 🔒 Security Features

- Passwords hashed with **SHA-256** before storing in database
- Admin credentials stored in database (not hardcoded)
- Session-based authentication with 30-minute timeout
- Admin panel hidden from public navigation
- Stock validation before order placement
- Input validation on all forms

---

## 📦 Key Commands

```bash
# Run the app
dotnet run

# Create a new migration
dotnet ef migrations add MigrationName

# Apply migrations
dotnet ef database update

# Build the project
dotnet build
```

---

## 🤝 Built With

This project was built step by step as a learning project covering:
- ASP.NET Core MVC architecture
- Entity Framework Core with SQLite
- Session-based authentication
- CRUD operations
- Dependency injection
- Razor Views templating
- Bootstrap 5 UI design

---

## 📄 License

This project is open source and available under the [MIT License](LICENSE).

---

<div align="center">
  <strong>Built with ❤️ using ASP.NET Core MVC</strong>
</div>