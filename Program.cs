using Microsoft.EntityFrameworkCore;
using StockManager.Data;
using StockManager.Services;

var builder = WebApplication.CreateBuilder(args);

//Crea la conexion a la base de datos SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
options.UseSqlite("Data Source = StockDB.db"));

// Add services to the container.
builder.Services.AddControllersWithViews();

//Add servicios personalizados en orden de dependencia
builder.Services.AddScoped<CategoryPathService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<StockMovementService>();
builder.Services.AddScoped<ProductSearchService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
