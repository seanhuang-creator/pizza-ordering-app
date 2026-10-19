
namespace PizzaApi.BusinessObjects;
public class Order
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = "";
    public string Phone { get; set; } = "";
    public OrderType Type { get; set; }
    public string Address { get; set; } = "";
    public string? PromoCode { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }
    public string? DiscountName { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public List<Pizza> Pizzas { get; set; } = new();
}
