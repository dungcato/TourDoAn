using Microsoft.EntityFrameworkCore;
using ToursDULICH.Models; // namespace context / models của m

var builder = WebApplication.CreateBuilder(args);

// ===== ĐĂNG KÝ DbContext (kết nối database) =====
var connectionString = builder.Configuration.GetConnectionString("Harmic");
// "Harmic" là key trong appsettings.json của m
builder.Services.AddDbContext<ToursDuLichContext>(options =>
    options.UseSqlServer(connectionString));

// ===== Đăng ký MVC + View =====
builder.Services
    .AddControllersWithViews();
   

var app = builder.Build();

// ===== PIPELINE XỬ LÝ REQUEST =====
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// phục vụ css/js/images trong wwwroot (AdminLTE, template,…)
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// ===== ROUTE CHO AREA ADMIN =====
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// ===== ROUTE MẶC ĐỊNH =====
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
