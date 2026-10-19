using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using PizzaApi.BusinessObjects;
using PizzaApi.Services;

namespace backend.Tests;

public class PricingTests
{
    private PricingService CreateService()
    {
        var rules = new List<IDiscountRule> { new TwoForTuesdayRule(), new Student10Rule() };
        return new PricingService(rules);
    }

    private static List<Pizza> MakePizzas(params (string size, string crust, string[] tops)[] defs)
    {
        return defs.Select(d => new Pizza { Size = d.size, Crust = d.crust, SauceType = "Regular", Ingredients = d.tops.ToList() }).ToList();
    }

    [Fact]
    public void Scenario1_12in_Ham_Pickup_Monday()
    {
        // Expected: subtotal 8.75 tax 1.31 total 10.06
        var svc = CreateService();
        var pizzas = MakePizzas(("12", "Regular", new[] { "Ham" }));
        var res = svc.Calculate(pizzas, OrderType.Pickup, null, new DateTime(2026, 10, 19)); // Monday
        Assert.Equal(8.75m, res.Subtotal);
        Assert.Equal(1.31m, res.Tax);
        Assert.Equal(10.06m, res.Total);
    }

    [Fact]
    public void Scenario3_TwoForTuesday_Discount_2_50()
    {
        var svc = CreateService();
        var pizzas = MakePizzas(("14", "Regular", Array.Empty<string>()), ("10", "Regular", Array.Empty<string>()));
        var tuesday = new DateTime(2026, 10, 20);
        var res = svc.Calculate(pizzas, OrderType.Pickup, null, tuesday);
        Assert.Equal(2.50m, res.Discount);
        Assert.Equal("Two-for-Tuesday", res.DiscountName);
    }

    [Fact]
    public void Scenario4_Student10_Discount_1_20()
    {
        var svc = CreateService();
        var pizzas = MakePizzas(("14", "Regular", Array.Empty<string>()));
        var res = svc.Calculate(pizzas, OrderType.Pickup, "STUDENT10", new DateTime(2026, 10, 19));
        Assert.Equal(1.20m, res.Discount);
    }

    [Fact]
    public void Student10_CaseInsensitive()
    {
        var svc = CreateService();
        var pizzas = MakePizzas(("10", "Regular", Array.Empty<string>()));
        var res = svc.Calculate(pizzas, OrderType.Pickup, "student10", DateTime.Now);
        Assert.True(res.Discount > 0);
    }

    [Fact]
    public void Discount_Precedence_Larger_Wins()
    {
        // 3x 16" = 45 subtotal, TwoForTuesday 7.5, Student10 4.5 => pick 7.5
        var svc = CreateService();
        var pizzas = MakePizzas(
            ("16", "Regular", Array.Empty<string>()),
            ("16", "Regular", Array.Empty<string>()),
            ("16", "Regular", Array.Empty<string>()));
        var res = svc.Calculate(pizzas, OrderType.Pickup, "STUDENT10", new DateTime(2026, 10, 20));
        Assert.Equal(7.50m, res.Discount);
        Assert.Equal("Two-for-Tuesday", res.DiscountName);
    }

    [Fact]
    public void Tax_Rounding_HalfAwayFromZero()
    {
        var svc = CreateService();
        var pizzas = MakePizzas(("12", "Regular", new[] { "Ham" }));
        var res = svc.Calculate(pizzas, OrderType.Pickup, null, DateTime.Now);
        Assert.Equal(8.75m, res.Subtotal);
        Assert.Equal(1.31m, res.Tax);
    }

    [Fact]
    public void Scenario5_Delivery_Fee_4_Dollars()
    {
        var svc = CreateService();
        var pizzas = MakePizzas(("10", "Regular", Array.Empty<string>()));
        var res = svc.Calculate(pizzas, OrderType.Delivery, null, DateTime.Now);
        Assert.Equal(4.00m, res.DeliveryFee);
        Assert.Equal(9.75m, res.Total); // 5 + 4 + 0.75 tax
    }

    [Fact]
    public void Scenario6_Delivery_Free_Over_40()
    {
        var svc = CreateService();
        var pizzas = MakePizzas(
            ("16", "Regular", Array.Empty<string>()),
            ("16", "Regular", Array.Empty<string>()),
            ("16", "Regular", Array.Empty<string>()));
        var res = svc.Calculate(pizzas, OrderType.Delivery, null, DateTime.Now);
        Assert.Equal(0m, res.DeliveryFee);
    }

    [Fact]
    public void Invalid_PromoCode_NotApplied_Empty_Discount()
    {
        var svc = CreateService();
        var pizzas = MakePizzas(("12", "Regular", Array.Empty<string>()));
        // Your Student10Rule returns 0 for unknown code, not throw - so check discount 0
        var res = svc.Calculate(pizzas, OrderType.Pickup, "FAKECODE", DateTime.Now);
        Assert.Equal(0m, res.Discount);
    }
}
