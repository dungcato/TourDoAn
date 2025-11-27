using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ToursDULICH.Models;

namespace ToursDULICH.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class RoomsController : Controller
    {
        private readonly ToursDuLichContext _context;

        public RoomsController(ToursDuLichContext context) => _context = context;

        // GET: Admin/Rooms
        public async Task<IActionResult> Index(string search, int? hotelId)
        {
            var query = _context.Rooms.Include(r => r.Hotel).AsQueryable();

            // 1. Lọc theo tên phòng
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(r => r.RoomName.Contains(search));
            }

            // 2. Lọc theo khách sạn (NẾU CÓ CHỌN)
            if (hotelId.HasValue && hotelId > 0)
            {
                query = query.Where(r => r.HotelId == hotelId);
            }

            // 3. Lấy danh sách khách sạn để đẩy ra Dropdown bộ lọc
            ViewBag.HotelList = new SelectList(await _context.Hotels.OrderBy(h => h.Name).ToListAsync(), "HotelId", "Name", hotelId);
            ViewBag.Search = search; // Giữ lại từ khóa tìm kiếm

            var rooms = await query.OrderByDescending(r => r.RoomId).ToListAsync();
            return View(rooms);
        }
        public IActionResult Create()
        {
            ViewBag.HotelId = new SelectList(_context.Hotels, "HotelId", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Room model)
        {
            ModelState.Remove("Hotel");
            ModelState.Remove("Bookings"); // Bỏ qua check Bookings

            if (ModelState.IsValid)
            {
                _context.Add(model);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Thêm phòng thành công!";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.HotelId = new SelectList(_context.Hotels, "HotelId", "Name", model.HotelId);
            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var room = await _context.Rooms.FindAsync(id);
            if (room == null) return NotFound();
            ViewBag.HotelId = new SelectList(_context.Hotels, "HotelId", "Name", room.HotelId);
            return View(room);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Room model)
        {
            if (id != model.RoomId) return NotFound();
            ModelState.Remove("Hotel");
            ModelState.Remove("Bookings");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(model);
                    await _context.SaveChangesAsync();
                    TempData["Message"] = "Cập nhật thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex) { TempData["Error"] = ex.Message; }
            }
            ViewBag.HotelId = new SelectList(_context.Hotels, "HotelId", "Name", model.HotelId);
            return View(model);
        }

        // 4. XÓA (Đơn giản hóa để tránh lỗi)
        // 4. XÓA (Đơn giản hóa tối đa để tránh lỗi)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            // Chỉ tìm đúng cái phòng cần xóa
            var room = await _context.Rooms.FindAsync(id);

            if (room != null)
            {
                try
                {
                    _context.Rooms.Remove(room);
                    await _context.SaveChangesAsync();
                    TempData["Message"] = "Xóa phòng thành công!";
                }
                catch (Exception ex)
                {
                    // Bắt lỗi nếu có vấn đề gì khác
                    TempData["Error"] = "Không thể xóa: " + ex.Message;
                }
            }
            else
            {
                TempData["Error"] = "Không tìm thấy phòng để xóa!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}