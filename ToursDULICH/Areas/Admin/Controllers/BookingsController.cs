using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToursDULICH.Models;

namespace ToursDULICH.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BookingsController : Controller
    {
        private readonly ToursDuLichContext _context;

        public BookingsController(ToursDuLichContext context)
        {
            _context = context;
        }

        // 1. HIỂN THỊ DANH SÁCH ĐƠN HÀNG
        public async Task<IActionResult> Index()
        {
            // Lấy dữ liệu kèm thông tin Tour, Phòng, Khách hàng
            var bookings = await _context.Bookings
                .Include(b => b.ToursNavigation) // Lấy thông tin Tour
                .Include(b => b.Room)            // Lấy thông tin Phòng
                .Include(b => b.User)            // Lấy thông tin User
                .OrderByDescending(b => b.BookingId) // Mới nhất lên đầu
                .ToListAsync();

            return View(bookings);
        }

        // 2. CẬP NHẬT TRẠNG THÁI (Duyệt đơn / Hủy đơn)
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                booking.Status = status;
                _context.Update(booking);
                await _context.SaveChangesAsync();
                TempData["Message"] = $"Cập nhật đơn #{id} thành công!";
            }
            return RedirectToAction(nameof(Index));
        }

        // 3. XÓA ĐƠN HÀNG
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Đã xóa đơn hàng!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}