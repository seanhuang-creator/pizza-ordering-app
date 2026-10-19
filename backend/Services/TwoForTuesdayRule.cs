
using PizzaApi.BusinessObjects;
namespace PizzaApi.Services;
public class TwoForTuesdayRule : IDiscountRule
{
    public DiscountResult Apply(List<Pizza> pizzas, decimal subtotal, string? promoCode, DateTime now)
    {
        // shop local timezone check - for test use Tuesday
        var isTuesday = now.DayOfWeek == DayOfWeek.Tuesday;
        // allow forcing via promo? No, always on Tuesday per BR-4, but spec says discount applies on Tuesdays automatically
        // For acceptance we apply regardless if caller wants? We check Tuesday
        if (!isTuesday) return new(0,"");
        if (pizzas.Count<2) return new(0,"");
        var sorted = pizzas.Select(p=>p.Cost).OrderByDescending(c=>c).ToList();
        decimal discount=0;
        for(int i=0;i<sorted.Count-1;i+=2)
        {
            var cheaper = Math.Min(sorted[i], sorted[i+1]);
            discount += cheaper*0.5m;
        }
        discount = Math.Round(discount,2,MidpointRounding.AwayFromZero);
        return new(discount, "Two-for-Tuesday");
    }
}
