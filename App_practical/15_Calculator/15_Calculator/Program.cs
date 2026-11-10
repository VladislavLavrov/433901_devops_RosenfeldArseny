using _15_Calculator.Data;
using _15_Calculator.Services;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var mariaDbConnectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");

builder.Services.AddDbContext<CalculatorContext>(options =>
    options.UseMySql(
        mariaDbConnectionString,
        new MySqlServerVersion(new Version(10, 5, 15))));

builder.Services.AddHttpClient();

// Kafka Producer and Consumer use the same topic and string message format.
builder.Services.AddSingleton<KafkaProducerHandler>();
builder.Services.AddSingleton<KafkaProducerService<Null, string>>();
builder.Services.AddHostedService<KafkaConsumerService>();

var app = builder.Build();

// Apply EF Core migrations before accepting web requests.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CalculatorContext>();
    await db.Database.MigrateAsync();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// HTTPS redirection is intentionally omitted: the container serves HTTP on port 5015.
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Calculator}/{action=Index}/{id?}");

app.Run();
