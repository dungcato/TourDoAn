using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToursDULICH.Models;

namespace ToursDULICH.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        private readonly ToursDuLichContext _context;

        public HomeController(ToursDuLichContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // 1. Thống kê số lượng (Cái này chạy nhanh, không lỗi)
            try
            {
                ViewBag.TotalHotels = _context.Hotels.Count();
                ViewBag.TotalRooms = _context.Rooms.Count();
                ViewBag.TotalTours = _context.Tours.Count();
                // Nếu bảng Bookings hoặc Users chưa có thì để tạm là 0
                ViewBag.TotalBookings = _context.Bookings.Count();
                ViewBag.TotalUsers = _context.Users.Count();
            }
            catch
            {
                // Nếu lỗi kết nối thì cho về 0 hết để web vẫn chạy được
                ViewBag.TotalHotels = 0;
                ViewBag.TotalRooms = 0;
                ViewBag.TotalTours = 0;
            }

            // 2. Tạm thời trả về danh sách RỖNG để không bị lỗi "Invalid column name 'Tours'"
            // Khi nào ông làm xong chức năng Booking thì mình mở lại sau
            return View(new List<Booking>());
        }
    }
}