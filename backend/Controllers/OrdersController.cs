
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PizzaApi.Data;
using PizzaApi.DTOs;
using PizzaApi.BusinessObjects;
using PizzaApi.Services;
[ApiController, Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IPricingService _pricing;
    public OrdersController(AppDbContext db, IPricingService pricing){ _db=db; _pricing=pricing; }

    private static OrderResponseDto Map(Order o)=> new(
        o.Id, o.CustomerName, o.Phone, o.Type.ToString(), o.Address, o.PromoCode, o.CreatedAt, o.Status.ToString(),
        o.Subtotal, o.Discount, o.DiscountName, o.DeliveryFee, o.Tax, o.Total,
        o.Pizzas.Select(p=>new PizzaQuoteDto(p.Size,p.Crust,p.SauceType,p.Ingredients,p.SecondHalfIngredients,p.Cost)).ToList()
    );

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] OrderCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CustomerName) || dto.CustomerName.Length>100) return BadRequest(new ProblemDetails{Title="Invalid name", Detail="Name required max 100"});
        var phoneDigits = new string(dto.Phone.Where(char.IsDigit).ToArray());
        if (phoneDigits.Length!=10) return BadRequest(new ProblemDetails{Title="Invalid phone", Detail="Phone must be 10 digits"});
        if (dto.Type.Equals("Delivery",StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(dto.Address)) return BadRequest(new ProblemDetails{Title="Address required", Detail="Delivery requires address"});
        if (!string.IsNullOrWhiteSpace(dto.PromoCode) && !dto.PromoCode.Equals("STUDENT10",StringComparison.OrdinalIgnoreCase)) return BadRequest(new ProblemDetails{Title="Invalid promo code", Detail="Invalid code"});
        if (dto.Pizzas==null || dto.Pizzas.Count<1 || dto.Pizzas.Count>10) return BadRequest(new ProblemDetails{Title="Invalid pizzas", Detail="1-10 pizzas"});

        var pizzas = new List<Pizza>();
        foreach(var p in dto.Pizzas)
        {
            var pz = new Pizza{ Size=p.Size, Crust=p.Crust, SauceType=p.SauceType, Ingredients=p.Ingredients??new(), SecondHalfIngredients=p.SecondHalfIngredients };
            try{ pz.CalculateCost(); }catch(Exception ex){ return BadRequest(new ProblemDetails{Title="Invalid pizza", Detail=ex.Message}); }
            pizzas.Add(pz);
        }
        Enum.TryParse<OrderType>(dto.Type,true,out var type);
        var pricing = _pricing.Calculate(pizzas, type, dto.PromoCode);

        var order = new Order{
            CustomerName=dto.CustomerName.Trim(), Phone=phoneDigits, Type=type,
            Address=type==OrderType.Delivery?dto.Address.Trim():"Pickup",
            PromoCode=string.IsNullOrWhiteSpace(dto.PromoCode)?null:dto.PromoCode.ToUpperInvariant(),
            CreatedAt=DateTime.UtcNow, Status=OrderStatus.Pending,
            Subtotal=pricing.Subtotal, Discount=pricing.Discount, DiscountName=pricing.DiscountName,
            DeliveryFee=pricing.DeliveryFee, Tax=pricing.Tax, Total=pricing.Total,
            Pizzas=pricing.PricedPizzas
        };
        _db.Orders.Add(order);
        await _db.SaveChangesAsync();
        var resp = Map(order);
        return CreatedAtAction(nameof(GetById), new{id=order.Id}, resp);
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? status, [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int page=1, [FromQuery] int pageSize=20)
    {
        pageSize = Math.Clamp(pageSize,1,100);
        var q = _db.Orders.Include(o=>o.Pizzas).AsQueryable();
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<OrderStatus>(status,true,out var st)) q=q.Where(o=>o.Status==st);
        if (from.HasValue) q=q.Where(o=>o.CreatedAt>=from.Value.ToUniversalTime());
        if (to.HasValue) q=q.Where(o=>o.CreatedAt<=to.Value.ToUniversalTime());
        q=q.OrderByDescending(o=>o.CreatedAt);
        var total = await q.CountAsync();
        var items = await q.Skip((page-1)*pageSize).Take(pageSize).ToListAsync();
        var result = new PagedResultDto<OrderResponseDto>(items.Select(Map).ToList(), total, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var o = await _db.Orders.Include(x=>x.Pizzas).FirstOrDefaultAsync(x=>x.Id==id);
        if (o==null) return NotFound();
        return Ok(Map(o));
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] StatusUpdateDto dto)
    {
        var o = await _db.Orders.FirstOrDefaultAsync(x=>x.Id==id);
        if (o==null) return NotFound();
        if (!Enum.TryParse<OrderStatus>(dto.Status,true,out var next)) return BadRequest(new ProblemDetails{Title="Invalid status"});
        var allowed = new Dictionary<OrderStatus, OrderStatus[]>{
            {OrderStatus.Pending, new[]{OrderStatus.Preparing, OrderStatus.Cancelled}},
            {OrderStatus.Preparing, new[]{OrderStatus.Ready, OrderStatus.Cancelled}},
            {OrderStatus.Ready, new[]{OrderStatus.Completed}},
            {OrderStatus.Completed, Array.Empty<OrderStatus>()},
            {OrderStatus.Cancelled, Array.Empty<OrderStatus>()}
        };
        if (!allowed[o.Status].Contains(next))
            return Conflict(new ProblemDetails{Title="Disallowed status change", Detail=$"Cannot change from {o.Status} to {next}", Status=409});
        o.Status = next;
        await _db.SaveChangesAsync();
        var full = await _db.Orders.Include(x=>x.Pizzas).FirstAsync(x=>x.Id==id);
        return Ok(Map(full));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var o = await _db.Orders.FirstOrDefaultAsync(x=>x.Id==id);
        if (o==null) return NotFound();
        if (o.Status!=OrderStatus.Cancelled) return Conflict(new ProblemDetails{Title="Cannot delete", Detail="Only cancelled orders may be deleted", Status=409});
        _db.Orders.Remove(o);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
