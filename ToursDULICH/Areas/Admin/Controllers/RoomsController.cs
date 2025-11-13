using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ToursDULICH.Models;

namespace ToursDULICH.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class RoomsController : Controller
    {
        private readonly ToursDuLichContext _ctx;

        public RoomsController(ToursDuLichContext ctx) => _ctx = ctx;

        // GET: Admin/Rooms?q=&hotelId=
        public async Task<IActionResult> Index(string? q, int? hotelId)
        {
            var query = _ctx.Rooms
                .Include(r => r.Hotel)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(r => r.RoomType.Contains(q));
            }
            if (hotelId.HasValue)
            {
                query = query.Where(r => r.HotelId == hotelId);
            }

            ViewBag.Hotels = new SelectList(await _ctx.Hotels.OrderBy(h => h.Name).ToListAsync(),
                                            "HotelId", "Name", hotelId);
            ViewBag.Q = q;

            var list = await query.OrderByDescending(r => r.RoomId).ToListAsync();
            return View(list); // Areas/Admin/Views/Rooms/Index.cshtml
        }

        // GET: Admin/Rooms/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.HotelId = new SelectList(await _ctx.Hotels.OrderBy(h => h.Name).ToListAsync(),
                                             "HotelId", "Name");
            return View(); // Areas/Admin/Views/Rooms/Create.cshtml
        }

        // POST: Admin/Rooms/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Room model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.HotelId = new SelectList(_ctx.Hotels, "HotelId", "Name", model.HotelId);
                return View(model);
            }

            _ctx.Rooms.Add(model);
            await _ctx.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Rooms/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var room = await _ctx.Rooms
                .Include(r => r.Hotel)
                .FirstOrDefaultAsync(r => r.RoomId == id);

            if (room == null) return NotFound();

            ViewBag.HotelId = new SelectList(_ctx.Hotels, "HotelId", "Name", room.HotelId);
            return View(room); // Areas/Admin/Views/Rooms/Edit.cshtml
        }

        // POST: Admin/Rooms/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Room model)
        {
            if (id != model.RoomId) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.HotelId = new SelectList(_ctx.Hotels, "HotelId", "Name", model.HotelId);
                return View(model);
            }

            var room = await _ctx.Rooms.FindAsync(id);
            if (room == null) return NotFound();

            room.HotelId = model.HotelId;
            room.RoomType = model.RoomType;
            room.PricePerNight = model.PricePerNight;

            await _ctx.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Rooms/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var room = await _ctx.Rooms.Include(r => r.Hotel)
                                       .FirstOrDefaultAsync(r => r.RoomId == id);
            if (room == null) return NotFound();
            return View(room);
        }

        // POST: Admin/Rooms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var room = await _ctx.Rooms
                .Include(r => r.Bookings) // kiểm tra ràng buộc
                .FirstOrDefaultAsync(r => r.RoomId == id);

            if (room == null) return NotFound();

            if (room.Bookings != null && room.Bookings.Any())
            {
                TempData["Error"] = "Phòng đang có đơn đặt, không thể xóa.";
                return RedirectToAction(nameof(Index));
            }

            _ctx.Rooms.Remove(room);
            await _ctx.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
