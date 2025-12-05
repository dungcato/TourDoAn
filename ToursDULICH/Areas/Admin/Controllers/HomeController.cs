using Microsoft.AspNetCore.Authorization; // Thư viện bảo mật
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToursDULICH.Models;

namespace ToursDULICH.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] // <--- QUAN TRỌNG: Chặn người lạ, chỉ Admin mới được vào
    public class HomeController : Controller
    {
        private readonly ToursDuLichContext _context;

        public HomeController(ToursDuLichContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // 1. TÍNH TOÁN SỐ LIỆU THỐNG KÊ (Cho các ô màu)
            try
            {
                ViewBag.TotalHotels = await _context.Hotels.CountAsync();
                ViewBag.TotalRooms = await _context.Rooms.CountAsync();
                ViewBag.TotalTours = await _context.Tours.CountAsync();
                ViewBag.TotalBookings = await _context.Bookings.CountAsync();
                ViewBag.TotalUsers = await _context.Users.CountAsync();
                ViewBag.TotalContacts = await _context.Contacts.CountAsync();
            }
            catch
            {
                // Nếu Database mới tinh chưa có bảng, gán bằng 0 để không lỗi trang
                ViewBag.TotalHotels = 0;
                ViewBag.TotalRooms = 0;
                ViewBag.TotalTours = 0;
                ViewBag.TotalBookings = 0;
                ViewBag.TotalUsers = 0;
                ViewBag.TotalContacts = 0;
            }

            // 2. LẤY DANH SÁCH 10 ĐƠN ĐẶT MỚI NHẤT (Để hiện bảng theo dõi)
            var recentBookings = await _context.Bookings
                .Include(b => b.User)            // Lấy tên khách
                .Include(b => b.ToursNavigation) // Lấy tên Tour
                .Include(b => b.Room)            // Lấy tên Phòng
                .OrderByDescending(b => b.BookingId) // Mới nhất lên đầu
                .Take(10)                        // Chỉ lấy 10 cái
                .ToListAsync();

            // 3. TRẢ VỀ VIEW KÈM DỮ LIỆU
            return View(recentBookings);
        }
    }
}