using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ToursDULICH.Models;

namespace ToursDULICH.Controllers
{
    public class AccountController : Controller
    {
        private readonly ToursDuLichContext _context;

        public AccountController(ToursDuLichContext context)
        {
            _context = context;
        }

        // 1. ĐĂNG KÝ (GET)
        public IActionResult Register()
        {
            return View();
        }

        // 2. XỬ LÝ ĐĂNG KÝ (POST)
        [HttpPost]
        public async Task<IActionResult> Register(User user)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra email trùng
                var checkEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
                if (checkEmail != null)
                {
                    ViewBag.Error = "Email này đã được sử dụng!";
                    return View(user);
                }

                // Mặc định role là Customer
                user.Role = "Customer";

                // Lưu vào DB (Lưu ý: Đồ án thì lưu plain text ok, thực tế nên mã hóa MD5/BCrypt)
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }
            return View(user);
        }

        // 3. ĐĂNG NHẬP (GET)
        public IActionResult Login()
        {
            return View();
        }

        // 4. XỬ LÝ ĐĂNG NHẬP (POST)
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            // Tìm user trong DB
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.Password == password);

            if (user != null)
            {
                // Tạo thông tin phiên đăng nhập (Claims)
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.FullName), // Lưu tên
                    new Claim(ClaimTypes.Email, user.Email),   // Lưu email
                    new Claim("UserId", user.UserId.ToString()), // Lưu ID để dùng khi đặt hàng
                    new Claim(ClaimTypes.Role, user.Role)      // Lưu Quyền (Admin/Customer)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                // Ghi Cookie (Đăng nhập thành công)
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

                // Phân quyền chuyển hướng
                if (user.Role == "Admin")
                {
                    return RedirectToAction("Index", "Home", new { area = "Admin" });
                }

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Email hoặc mật khẩu không đúng!";
            return View();
        }

        // 5. ĐĂNG XUẤT
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}