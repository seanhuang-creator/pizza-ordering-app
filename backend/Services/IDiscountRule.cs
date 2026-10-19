
namespace PizzaApi.Services;
public record DiscountResult(decimal Amount, string Name);
public interface IDiscountRule { DiscountResult Apply(List<BusinessObjects.Pizza> pizzas, decimal subtotal, string? promoCode, DateTime now); }
