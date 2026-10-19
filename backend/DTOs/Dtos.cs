
namespace PizzaApi.DTOs;
public record MenuDto(List<SizeDto> Sizes, List<CrustDto> Crusts, List<SauceDto> Sauces, List<ToppingDto> Toppings, decimal DeliveryFee, decimal FreeDeliveryThreshold, decimal TaxRate, int MaxPizzasPerOrder);
public record SizeDto(string Name, decimal BasePrice);
public record CrustDto(string Name, decimal ExtraPrice, List<string>? AllowedSizes);
public record SauceDto(string Name);
public record ToppingDto(string Name, decimal Price, bool IsPremium);

public record PizzaDto(string Size, string Crust, string SauceType, List<string> Ingredients, List<string>? SecondHalfIngredients);
public record QuoteRequestDto(List<PizzaDto> Pizzas, string? PromoCode, string Type);
public record QuoteResponseDto(List<PizzaQuoteDto> Pizzas, decimal Subtotal, decimal Discount, string? DiscountName, decimal DeliveryFee, decimal Tax, decimal Total);
public record PizzaQuoteDto(string Size, string Crust, string SauceType, List<string> Ingredients, List<string>? SecondHalfIngredients, decimal Cost);

public record OrderCreateDto(string CustomerName, string Phone, string Type, string Address, string? PromoCode, List<PizzaDto> Pizzas);
public record OrderResponseDto(int Id, string CustomerName, string Phone, string Type, string Address, string? PromoCode, DateTime CreatedAt, string Status, decimal Subtotal, decimal Discount, string? DiscountName, decimal DeliveryFee, decimal Tax, decimal Total, List<PizzaQuoteDto> Pizzas);
public record StatusUpdateDto(string Status);
public record PagedResultDto<T>(List<T> Items, int TotalCount, int Page, int PageSize);
