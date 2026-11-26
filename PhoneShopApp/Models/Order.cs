namespace PhoneShopApp.Models;

public class Order
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.Now;
    public string Status { get; set; } = "Feldolgozás alatt";
    public decimal TotalAmount { get; set; }

    public string UserId { get; set; } = "";
    public AppUser? User { get; set; }

    public List<OrderItem> Items { get; set; } = new();
}
