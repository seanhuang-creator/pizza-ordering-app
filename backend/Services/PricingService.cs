
using PizzaApi.BusinessObjects;
namespace PizzaApi.Services;
public record PricingResult(decimal Subtotal, decimal Discount, string? DiscountName, decimal DeliveryFee, decimal Tax, decimal Total, List<Pizza> PricedPizzas);
public interface IPricingService { PricingResult Calculate(List<Pizza> pizzas, OrderType type, string? promoCode, DateTime? now = null); }

public class PricingService : IPricingService
{
    private readonly IEnumerable<IDiscountRule> _rules;
    public PricingService(IEnumerable<IDiscountRule> rules) { _rules = rules; }

    public PricingResult Calculate(List<Pizza> pizzas, OrderType type, string? promoCode, DateTime? now = null)
    {
        if (pizzas.Count<1 || pizzas.Count>10) throw new ArgumentException("Pizzas 1 to 10 per order");
        foreach(var p in pizzas) { p.Cost = p.CalculateCost(); }
        var subtotal = pizzas.Sum(p=>p.Cost);
        subtotal = Math.Round(subtotal,2,MidpointRounding.AwayFromZero);

        var time = now ?? DateTime.Now;
        DiscountResult best = new(0,"");
        foreach(var r in _rules)
        {
            var dr = r.Apply(pizzas, subtotal, promoCode, time);
            if (dr.Amount > best.Amount) best = dr;
        }

        decimal delivery = 0;
        if (type==OrderType.Delivery)
        {
            delivery = subtotal>40m ? 0m : 4m;
        }

        var taxable = subtotal - best.Amount;
        if (taxable<0) taxable=0;
        var tax = Math.Round(taxable*0.15m,2,MidpointRounding.AwayFromZero);
        var total = Math.Round(taxable + delivery + tax,2,MidpointRounding.AwayFromZero);

        return new PricingResult(subtotal, best.Amount, best.Amount>0?best.Name:null, delivery, tax, total, pizzas);
    }
}
