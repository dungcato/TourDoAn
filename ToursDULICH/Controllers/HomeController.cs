using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToursDULICH.Models;

namespace ToursDULICH.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        // [SỬA] 1. Khai báo biến _context
        private readonly ToursDuLichContext _context;

        // [SỬA] 2. Inject Context vào Constructor
        public HomeController(ILogger<HomeController> logger, ToursDuLichContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult login()
        {
            return View();
        }

        public IActionResult register()
        {
            return View();
        }

        // Action Index lấy dữ liệu động từ Database
        public async Task<IActionResult> Index()
        {
            // 1. Lấy 6 Tour mới nhất (Giữ nguyên code cũ)
            var tours = await _context.Tours
                .Include(t => t.City)
                .OrderByDescending(t => t.TourId)
                .Take(6)
                .ToListAsync();

            // 2. Lấy 6 Khách sạn mới nhất (Giữ nguyên code cũ)
            var hotels = await _context.Hotels
                .Include(h => h.City)
                .OrderByDescending(h => h.Rating)
                .Take(6)
                .ToListAsync();

            // 3. [THÊM MỚI] Lấy danh sách Địa điểm (Cities) để hiển thị Slider
            // Logic: Lấy những thành phố có ảnh, kèm theo danh sách Tours/Hotels để đếm số lượng
            var cities = await _context.Cities
                .Include(c => c.Tours)  // Kèm Tour để đếm số lượng
                .Include(c => c.Hotels) // Kèm Hotel để đếm số lượng
                .Where(c => !string.IsNullOrEmpty(c.Image)) // Chỉ lấy thành phố đã có ảnh
                .Take(8) // Lấy khoảng 8 cái để chạy Slider
                .ToListAsync();

            // 4. Đóng gói tất cả vào ViewModel
            var viewModel = new HomeViewModel
            {
                FeaturedTours = tours,
                FeaturedHotels = hotels,
                FeaturedCities = cities // <--- Gán danh sách thành phố vào đây
            };

            return View(viewModel);
        }

        public IActionResult about()
        {
            return View();
        }

        // Nếu bạn đã có TourController riêng, có thể xóa hoặc giữ lại action này để redirect
        public IActionResult tour()
        {
            return RedirectToAction("Index", "Tour");
        }

        // Nếu bạn đã có HotelController riêng, có thể xóa hoặc giữ lại action này để redirect
        public IActionResult hotel()
        {
            return RedirectToAction("Index", "Hotel");
        }

        public IActionResult blog()
        {
            return View();
        }

        public IActionResult contact()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

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