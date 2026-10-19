using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using PizzaApi.Data;
using Xunit;

namespace backend.Tests;

public class CustomWebFactory : WebApplicationFactory<Program>
{
    private static readonly SqliteConnection _keepAlive = new("DataSource=:memory:");
    private static bool _isInitialized = false;
    private static readonly object _lock = new();

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null) services.Remove(descriptor);
            var dbDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(AppDbContext));
            if (dbDescriptor != null) services.Remove(dbDescriptor);

            lock (_lock)
            {
                if (_keepAlive.State != System.Data.ConnectionState.Open)
                    _keepAlive.Open();
            }

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite(_keepAlive);
            });

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            lock (_lock)
            {
                if (!_isInitialized)
                {
                    db.Database.EnsureCreated();
                    _isInitialized = true;
                }
            }
        });
    }
}

public class QuoteApiTests : IClassFixture<CustomWebFactory>
{
    private readonly HttpClient _client;
    public QuoteApiTests(CustomWebFactory factory) { _client = factory.CreateClient(); }

    [Fact]
    public async Task Scenario1_Quote_12in_Ham_Pickup_Returns_10_06()
    {
        var payload = new { pizzas = new[] { new { size = "12", crust = "Regular", sauceType = "Regular", ingredients = new[] { "Ham" } } }, type = "Pickup", promoCode = (string?)null };
        var res = await _client.PostAsJsonAsync("/api/quote", payload);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var json = await res.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var total = doc.RootElement.GetProperty("total").GetDecimal();
        Assert.Equal(10.06m, total);
    }
    [Fact]
    public async Task Quote_InvalidTopping_Returns_400_InvalidTopping()
    {
        var payload = new { pizzas = new[] { new { size = "12", crust = "Regular", sauceType = "Regular", ingredients = new[] { "UnicornMeat" } } }, type = "Pickup" };
        var res = await _client.PostAsJsonAsync("/api/quote", payload);
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        var body = await res.Content.ReadAsStringAsync();
        Assert.Contains("Invalid topping", body, StringComparison.OrdinalIgnoreCase);
    }
    [Fact]
    public async Task Quote_InvalidPromoCode_Returns_400()
    {
        var payload = new { pizzas = new[] { new { size = "12", crust = "Regular", sauceType = "Regular", ingredients = new[] { "Ham" } } }, type = "Pickup", promoCode = "FAKECODE" };
        var res = await _client.PostAsJsonAsync("/api/quote", payload);
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        var body = await res.Content.ReadAsStringAsync();
        Assert.Contains("promo", body, StringComparison.OrdinalIgnoreCase);
    }
    [Fact]
    public async Task Quote_HalfAndHalf_Scenario2_Returns_13_38_Subtotal()
    {
        var payload = new { pizzas = new[] { new { size = "14", crust = "Regular", sauceType = "Regular", ingredients = new[] { "Ham", "Onions" }, secondHalfIngredients = new[] { "Shrimp" } } }, type = "Pickup" };
        var res = await _client.PostAsJsonAsync("/api/quote", payload);
        res.EnsureSuccessStatusCode();
        var json = await res.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var subtotal = doc.RootElement.GetProperty("subtotal").GetDecimal();
        Assert.Equal(13.38m, subtotal);
    }
    [Fact]
    public async Task Orders_Status_Preparing_To_Completed_Should_Return_409()
    {
        var createPayload = new { customerName = "Sean", phone = "9021234567", type = "Pickup", address = "", pizzas = new[] { new { size = "12", crust = "Regular", sauceType = "Regular", ingredients = new[] { "Ham" } } } };
        var createRes = await _client.PostAsJsonAsync("/api/orders", createPayload);
        Assert.Equal(HttpStatusCode.Created, createRes.StatusCode);
        var createJson = await createRes.Content.ReadAsStringAsync();
        using var createDoc = JsonDocument.Parse(createJson);
        var id = createDoc.RootElement.GetProperty("id").GetInt32();
        var prepRes = await _client.PatchAsJsonAsync($"/api/orders/{id}/status", new { status = "Preparing" });
        Assert.Equal(HttpStatusCode.OK, prepRes.StatusCode);
        var badRes = await _client.PatchAsJsonAsync($"/api/orders/{id}/status", new { status = "Completed" });
        Assert.Equal(HttpStatusCode.Conflict, badRes.StatusCode);
        var body = await badRes.Content.ReadAsStringAsync();
        Assert.Contains("Disallowed", body, StringComparison.OrdinalIgnoreCase);
    }
}
