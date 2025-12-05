using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using ToursDULICH.Models;

var builder = WebApplication.CreateBuilder(args);

// ==============================================
// 1. ĐĂNG KÝ DỊCH VỤ (SERVICES)
// ==============================================

// Kết nối Database
builder.Services.AddDbContext<ToursDuLichContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Harmic")));

// Đăng ký dịch vụ Authentication (Cookie) - Dùng cho Khách hàng
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";   // Chưa đăng nhập thì về đây
        options.AccessDeniedPath = "/Account/AccessDenied"; // Không đủ quyền thì về đây
        options.ExpireTimeSpan = TimeSpan.FromDays(1); // Cookie tồn tại 1 ngày
    });

// Đăng ký dịch vụ Session - Dùng cho Admin (Cách đơn giản)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session tồn tại 30 phút
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Đăng ký MVC
builder.Services.AddControllersWithViews();

// ==============================================
// 2. XÂY DỰNG ỨNG DỤNG
// ==============================================
var app = builder.Build();

// ==============================================
// 3. CẤU HÌNH PIPELINE (MIDDLEWARE)
// ==============================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// --- BỘ BA QUAN TRỌNG (Phải đúng thứ tự này) ---
app.UseAuthentication(); // 1. Xác thực (Kiểm tra danh tính)
app.UseAuthorization();  // 2. Phân quyền (Kiểm tra quyền hạn)
app.UseSession();        // 3. Kích hoạt Session
// -----------------------------------------------

// Định tuyến cho Admin (Area)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// Định tuyến mặc định (User)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();