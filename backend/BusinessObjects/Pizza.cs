
namespace PizzaApi.BusinessObjects;
public class Pizza
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string Size { get; set; } = "12in";
    public string Crust { get; set; } = "Regular";
    public string SauceType { get; set; } = "Regular";
    public List<string> Ingredients { get; set; } = new();
    public List<string>? SecondHalfIngredients { get; set; }
    public decimal Cost { get; set; }

    public static readonly Dictionary<string, decimal> SizeBase = new() {
        {"10",5m},{"12",8m},{"14",12m},{"16",15m},
        {"10in",5m},{"12in",8m},{"14in",12m},{"16in",15m},
        {"10\"",5m},{"12\"",8m},{"14\"",12m},{"16\"",15m}
    };
    public static readonly Dictionary<string, (decimal price, bool premium)> Toppings = new(){
        {"Pepperoni", (0.75m,false)}, {"Ham",(0.75m,false)}, {"Mushroom",(0.75m,false)},
        {"Onions",(0.75m,false)}, {"Sausage",(0.75m,false)}, {"Bacon",(0.75m,false)},
        {"Extra Cheese",(0.75m,false)}, {"Black Olives",(0.75m,false)}, {"Green Peppers",(0.75m,false)},
        {"Pineapple",(0.75m,false)}, {"Prosciutto",(1.25m,true)}, {"Shrimp",(1.25m,true)}, {"Goat Cheese",(1.25m,true)}
    };

    private string NormalizeSize(string s)
    {
        s = s.Trim();
        var m = System.Text.RegularExpressions.Regex.Match(s, @"\d+");
        if (!m.Success) return "12in";
        var num = m.Value;
        return num + "in";
    }

    public decimal CalculateCost()
    {
        var normSize = NormalizeSize(Size);
        var altKey = Size;
        decimal basePrice;
        if (!SizeBase.TryGetValue(normSize, out basePrice) && !SizeBase.TryGetValue(altKey, out basePrice))
            throw new ArgumentException($"Invalid size {Size}");

        decimal crustPrice = 0;
        if (Crust.Contains("Gluten", StringComparison.OrdinalIgnoreCase)) crustPrice = 3m;
        else if (Crust.Contains("Stuffed", StringComparison.OrdinalIgnoreCase)) crustPrice = 2m;

        var sizeNum = int.Parse(System.Text.RegularExpressions.Regex.Match(normSize, @"\d+").Value);
        if (Crust.Contains("Gluten", StringComparison.OrdinalIgnoreCase) && sizeNum != 10 && sizeNum != 12)
            throw new ArgumentException("Gluten-free only for 10 and 12");

        decimal toppingCost = 0;
        bool half = SecondHalfIngredients != null;
        if (half)
        {
            var left = Ingredients ?? new List<string>();
            var right = SecondHalfIngredients ?? new List<string>();
            if (left.GroupBy(x=>x.ToLower()).Any(g=>g.Count()>1) || right.GroupBy(x=>x.ToLower()).Any(g=>g.Count()>1))
                throw new ArgumentException("Duplicate topping within half");
            foreach (var t in left.Concat(right))
            {
                if (!Toppings.TryGetValue(t, out var td)) throw new ArgumentException($"Invalid topping {t}");
                toppingCost += td.price * 0.5m;
            }
        }
        else
        {
            var list = Ingredients ?? new();
            if (list.GroupBy(x=>x.ToLower()).Any(g=>g.Count()>1)) throw new ArgumentException("Duplicate topping");
            foreach (var t in list)
            {
                if (!Toppings.TryGetValue(t, out var td)) throw new ArgumentException($"Invalid topping {t}");
                toppingCost += td.price;
            }
        }
        var total = basePrice + crustPrice + toppingCost;
        return Math.Round(total, 2, MidpointRounding.AwayFromZero);
    }

    public override string ToString() => $"{Size} {Crust} {SauceType} [{string.Join(",", Ingredients)}]";
}
