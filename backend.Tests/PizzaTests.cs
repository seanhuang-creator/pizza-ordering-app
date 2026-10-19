using System.Collections.Generic;
using Xunit;
using PizzaApi.BusinessObjects;

namespace backend.Tests;

public class PizzaTests
{
    [Theory]
    [InlineData("10", 5.00)]
    [InlineData("12", 8.00)]
    [InlineData("14", 12.00)]
    [InlineData("16", 15.00)]
    public void Cost_Each_Size_Base(string size, decimal expected)
    {
        var pizza = new Pizza { Size = size, Crust = "Regular", SauceType = "Regular", Ingredients = new List<string>() };
        Assert.Equal(expected, pizza.CalculateCost());
    }

    [Fact]
    public void Cost_StuffedCrust_Adds_2()
    {
        var pizza = new Pizza { Size = "12", Crust = "Stuffed", SauceType = "Regular", Ingredients = new List<string>() };
        Assert.Equal(10.00m, pizza.CalculateCost());
    }

    [Fact]
    public void Cost_GlutenFree_Adds_3()
    {
        var pizza = new Pizza { Size = "10", Crust = "GlutenFree", SauceType = "Regular", Ingredients = new List<string>() };
        Assert.Equal(8.00m, pizza.CalculateCost());
    }

    [Fact]
    public void Cost_StandardTopping_0_75_Scenario1()
    {
        // Acceptance #1: 12" + Ham = 8.75
        var pizza = new Pizza { Size = "12", Crust = "Regular", SauceType = "Regular", Ingredients = new List<string> { "Ham" } };
        Assert.Equal(8.75m, pizza.CalculateCost());
    }

    [Fact]
    public void Cost_PremiumTopping_1_25()
    {
        var pizza = new Pizza { Size = "12", Crust = "Regular", SauceType = "Regular", Ingredients = new List<string> { "Prosciutto" } };
        Assert.Equal(9.25m, pizza.CalculateCost());
    }

    [Fact]
    public void Cost_HalfAndHalf_Scenario2_13_38()
    {
        var pizza = new Pizza
        {
            Size = "14",
            Crust = "Regular",
            SauceType = "Regular",
            Ingredients = new List<string> { "Ham", "Onions" },
            SecondHalfIngredients = new List<string> { "Shrimp" }
        };
        Assert.Equal(13.38m, pizza.CalculateCost());
    }

    [Fact]
    public void GlutenFree_NotAllowed_For_14_Should_Throw()
    {
        var pizza = new Pizza { Size = "14", Crust = "GlutenFree", SauceType = "Regular" };
        var ex = Assert.Throws<System.ArgumentException>(() => pizza.CalculateCost());
        Assert.Contains("Gluten", ex.Message);
    }

    [Fact]
    public void Duplicate_Topping_Within_Half_Should_Throw()
    {
        var pizza = new Pizza { Size = "12", Crust = "Regular", SauceType = "Regular", Ingredients = new List<string> { "Ham", "Ham" } };
        Assert.Throws<System.ArgumentException>(() => pizza.CalculateCost());
    }

    [Fact]
    public void ToString_Exists()
    {
        var pizza = new Pizza { Size = "12", Crust = "Regular", SauceType = "Regular", Ingredients = new List<string> { "Ham" } };
        Assert.False(string.IsNullOrWhiteSpace(pizza.ToString()));
    }
}
