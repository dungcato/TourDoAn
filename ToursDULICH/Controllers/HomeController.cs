using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToursDULICH.Models;

namespace ToursDULICH.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ToursDuLichContext _context;

        public HomeController(ILogger<HomeController> logger, ToursDuLichContext context)
        {
            _logger = logger;
            _context = context;
        }

        // --- CÁC TRANG TĨNH & AUTH ---
        // Lưu ý: Viết hoa chữ cái đầu (PascalCase) cho đúng chuẩn C#
        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Blog()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // --- TRANG CHỦ (INDEX) ---
        public async Task<IActionResult> Index()
        {
            // 1. Lấy 6 Tour mới nhất
            var tours = await _context.Tours
                .Include(t => t.City)
                .OrderByDescending(t => t.TourId)
                .Take(6)
                .ToListAsync();

            // 2. Lấy 6 Khách sạn đánh giá cao nhất
            var hotels = await _context.Hotels
                .Include(h => h.City)
                .OrderByDescending(h => h.Rating)
                .Take(6)
                .ToListAsync();

            // 3. Lấy 8 Thành phố nổi bật (có ảnh) để chạy Slider
            var cities = await _context.Cities
                .Include(c => c.Tours)
                .Include(c => c.Hotels)
                .Where(c => !string.IsNullOrEmpty(c.Image))
                .Take(8)
                .ToListAsync();

            // 4. Đóng gói vào ViewModel
            var viewModel = new HomeViewModel
            {
                FeaturedTours = tours,
                FeaturedHotels = hotels,
                FeaturedCities = cities
            };

            return View(viewModel);
        }

        // --- ĐIỀU HƯỚNG (REDIRECT) ---
        public IActionResult Tour()
        {
            return RedirectToAction("Index", "Tour");
        }

        public IActionResult Hotel()
        {
            return RedirectToAction("Index", "Hotel");
        }

        // --- CHỨC NĂNG LIÊN HỆ (CONTACT) ---

        // 1. GET: Hiển thị form
        [HttpGet]
        public IActionResult Contact()
        {
            return View();
        }

        // 2. POST: Xử lý dữ liệu gửi lên
        // Sử dụng 'ToursDULICH.Models.Contact' đầy đủ để tránh trùng tên với hàm Contact
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(ToursDULICH.Models.Contact model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    model.CreatedAt = DateTime.Now;
                    _context.Contacts.Add(model);
                    await _context.SaveChangesAsync();

                    TempData["Message"] = "Cảm ơn bạn! Chúng tôi đã nhận được liên hệ.";
                    return RedirectToAction(nameof(Contact)); // Load lại trang Contact sạch sẽ
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra khi lưu dữ liệu.");
                }
            }

            // Nếu dữ liệu không hợp lệ, trả lại View kèm thông báo lỗi
            return View(model);
        }

        // --- XỬ LÝ LỖI ---
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}