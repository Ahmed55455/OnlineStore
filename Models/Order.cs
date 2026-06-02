namespace OnlineStore.Models
{
    public class Order
    {
        public int Id { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        public string Status { get; set; } = "Pending";

        public decimal TotalPrice { get; set; }

        // Customer Info
        public string CustomerName  { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string Address       { get; set; } = string.Empty;

        // Payment Info
        public string PaymentStatus { get; set; } = "Unpaid";
        public string PaymentMethod { get; set; } = "Card";

        // Link to User
        public int? UserId { get; set; }
        public User? User  { get; set; }

        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}