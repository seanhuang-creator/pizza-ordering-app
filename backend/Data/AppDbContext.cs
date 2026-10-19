using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using PizzaApi.BusinessObjects;

namespace PizzaApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> opt) : base(opt) { }
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Pizza> Pizzas => Set<Pizza>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Order>().HasMany(o => o.Pizzas).WithOne().HasForeignKey(p => p.OrderId).OnDelete(DeleteBehavior.Cascade);

        var comparer = new ValueComparer<List<string>>(
            (c1, c2) => (c1 == null && c2 == null) || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
            c => c == null ? 0 : c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList());

        var nullableComparer = new ValueComparer<List<string>?>(
            (c1, c2) => (c1 == null && c2 == null) || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
            c => c == null ? 0 : c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c == null ? null : c.ToList());

        b.Entity<Pizza>().Property(p => p.Ingredients).HasConversion(
            v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
            v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new()
        ).Metadata.SetValueComparer(comparer);

        b.Entity<Pizza>().Property(p => p.SecondHalfIngredients).HasConversion(
            v => v == null ? null : System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
            v => v == null ? null : System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null)
        ).Metadata.SetValueComparer(nullableComparer);
    }
}
