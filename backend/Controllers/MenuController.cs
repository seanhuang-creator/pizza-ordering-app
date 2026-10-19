
using Microsoft.AspNetCore.Mvc;
using PizzaApi.DTOs;
using PizzaApi.BusinessObjects;
[ApiController, Route("api/menu")]
public class MenuController : ControllerBase
{
    [HttpGet]
    public MenuDto Get()
    {
        var sizes = new List<SizeDto>{ new("10",5m), new("12",8m), new("14",12m), new("16",15m) };
        var crusts = new List<CrustDto>{ new("Regular",0m,null), new("Stuffed",2m,null), new("GlutenFree",3m,new List<string>{"10","12"}) };
        var sauces = new List<SauceDto>{ new("Regular"), new("Spicy") };
        var toppings = Pizza.Toppings.Select(kv=>new ToppingDto(kv.Key, kv.Value.price, kv.Value.premium)).ToList();
        return new(sizes, crusts, sauces, toppings, 4m, 40m, 0.15m, 10);
    }
}
