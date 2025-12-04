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

        public async Task<IActionResult> Index()
        {
            // 1. TÍNH TOÁN SỐ LIỆU CHO CÁC Ô THỐNG KÊ (Đẩy vào ViewBag)
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
                // Nếu chưa có bảng nào thì gán bằng 0 để không lỗi
                ViewBag.TotalHotels = 0;
                ViewBag.TotalRooms = 0;
                ViewBag.TotalTours = 0;
                ViewBag.TotalBookings = 0;
                ViewBag.TotalUsers = 0;
                ViewBag.TotalContacts = 0;
            }

            // 2. LẤY DANH SÁCH 10 ĐƠN ĐẶT MỚI NHẤT (Đẩy vào Model)
            // Lưu ý: Dùng .Include để lấy thông tin User, Tour, Room đi kèm
            var recentBookings = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.ToursNavigation) // Hoặc b.Tour nếu bạn đã đổi tên theo hướng dẫn trước
                .Include(b => b.Room)
                .OrderByDescending(b => b.BookingId)
                .Take(10)
                .ToListAsync();

            // 3. TRẢ VỀ VIEW KÈM MODEL LÀ DANH SÁCH ĐƠN HÀNG
            return View(recentBookings);
        }
    }
}