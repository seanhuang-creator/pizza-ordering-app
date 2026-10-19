using Microsoft.AspNetCore.Mvc;
using PizzaApi.DTOs;
using PizzaApi.BusinessObjects;
using PizzaApi.Services;

[ApiController, Route("api/quote")]
public class QuoteController : ControllerBase
{
    private readonly IPricingService _pricing;
    public QuoteController(IPricingService pricing) { _pricing = pricing; }

    [HttpPost]
    public IActionResult Post([FromBody] QuoteRequestDto req)
    {
        if (req.Pizzas == null || req.Pizzas.Count < 1 || req.Pizzas.Count > 10)
        {
            var err = new Dictionary<string, string[]>
            {
                { "Pizzas", new[] { "Pizzas must be 1 to 10" } }
            };
            return BadRequest(new ValidationProblemDetails(err)
            {
                Title = "Validation failed",
                Detail = "Pizzas must be 1 to 10",
                Status = 400
            });
        }

        var errors = new Dictionary<string, string[]>();
        var pizzas = new List<Pizza>();

        foreach (var p in req.Pizzas)
        {
            try
            {
                var pz = new Pizza
                {
                    Size = p.Size,
                    Crust = p.Crust,
                    SauceType = p.SauceType,
                    Ingredients = p.Ingredients ?? new List<string>(),
                    SecondHalfIngredients = p.SecondHalfIngredients
                };
                pz.CalculateCost();
                pizzas.Add(pz);
            }
            catch (Exception ex)
            {
                if (!errors.ContainsKey("Ingredients"))
                    errors["Ingredients"] = new[] { ex.Message };
                else
                {
                    var list = errors["Ingredients"].ToList();
                    list.Add(ex.Message);
                    errors["Ingredients"] = list.ToArray();
                }
            }
        }

        if (errors.Any())
        {
            var allMsgs = errors.Values.SelectMany(v => v).ToList();
            var toppingErr = allMsgs.FirstOrDefault(m => m.Contains("Invalid topping", StringComparison.OrdinalIgnoreCase));

            return BadRequest(new ValidationProblemDetails(errors)
            {
                Title = toppingErr != null ? "Invalid topping" : "Validation failed",
                Detail = toppingErr ?? "One or more pizzas invalid",
                Status = 400
            });
        }

        if (!Enum.TryParse<OrderType>(req.Type, true, out var type))
            type = OrderType.Pickup;

        if (!string.IsNullOrWhiteSpace(req.PromoCode) && !req.PromoCode.Equals("STUDENT10", StringComparison.OrdinalIgnoreCase))
        {
            var promoErrors = new Dictionary<string, string[]>
            {
                { "PromoCode", new[] { $"Promo code {req.PromoCode} is invalid" } }
            };
            return BadRequest(new ValidationProblemDetails(promoErrors)
            {
                Title = "Validation failed",
                Detail = $"Promo code {req.PromoCode} is invalid",
                Status = 400
            });
        }

        var result = _pricing.Calculate(pizzas, type, req.PromoCode);

        var resp = new QuoteResponseDto(
            result.PricedPizzas.Select(x => new PizzaQuoteDto(x.Size, x.Crust, x.SauceType, x.Ingredients, x.SecondHalfIngredients, x.Cost)).ToList(),
            result.Subtotal,
            result.Discount,
            result.DiscountName,
            result.DeliveryFee,
            result.Tax,
            result.Total
        );

        return Ok(resp);
    }
}