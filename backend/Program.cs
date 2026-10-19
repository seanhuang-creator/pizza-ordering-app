using PizzaApi.Data;
using PizzaApi.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers().AddJsonOptions(o=>o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(opt=>opt.UseSqlite("Data Source=data/pizza.db"));
builder.Services.AddScoped<IDiscountRule, Student10Rule>();
builder.Services.AddScoped<IDiscountRule, TwoForTuesdayRule>();
builder.Services.AddScoped<IPricingService, PricingService>();
builder.Services.AddCors(o=>o.AddDefaultPolicy(p=>p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try { db.Database.Migrate(); }
    catch { db.Database.EnsureCreated(); }
}

app.UseCors();
app.UseSwagger();
app.UseSwaggerUI();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.MapControllers();
app.Run();

public partial class Program { }
