using Microsoft.AspNetCore.Mvc;
using ToursDULICH.Models;

namespace ToursDULICH.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AccountController : Controller
    {
        private readonly ToursDuLichContext _context;

        public AccountController(ToursDuLichContext context)
        {
            _context = context;
        }

        // 1. Trang Đăng nhập
        public IActionResult Login()
        {
            return View();
        }

        // 2. Xử lý Đăng nhập
        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            // Tìm user trong DB có đúng Email + Pass + Role='Admin' không
            var admin = _context.Users
                .FirstOrDefault(u => u.Email == email && u.Password == password && u.Role == "Admin");

            if (admin != null)
            {
                // Đăng nhập thành công -> Lưu vào Session
                HttpContext.Session.SetString("AdminEmail", admin.Email);

                // Chuyển hướng vào trang Dashboard
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }

            // Đăng nhập sai
            ViewBag.Error = "Sai tài khoản hoặc không phải Admin!";
            return View();
        }

        // 3. Đăng xuất
        public IActionResult Logout()
        {
            // Xóa Session
            HttpContext.Session.Remove("AdminEmail");
            return RedirectToAction("Login");
        }
    }
}