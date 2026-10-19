
using PizzaApi.BusinessObjects;
namespace PizzaApi.Services;
public class Student10Rule : IDiscountRule
{
    public DiscountResult Apply(List<Pizza> pizzas, decimal subtotal, string? promoCode, DateTime now)
    {
        if (string.IsNullOrWhiteSpace(promoCode)) return new(0,"");
        if (!promoCode.Equals("STUDENT10", StringComparison.OrdinalIgnoreCase)) return new(0,"");
        var amt = Math.Round(subtotal*0.10m,2,MidpointRounding.AwayFromZero);
        return new(amt, "STUDENT10");
    }
}
