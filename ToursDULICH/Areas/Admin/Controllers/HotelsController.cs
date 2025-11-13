using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using ToursDULICH.Models;

namespace ToursDULICH.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HotelsController : Controller
    {
        private readonly ToursDuLichContext _context;

        public HotelsController(ToursDuLichContext context)
        {
            _context = context;
        }

        // GET: /Admin/Hotels
        public async Task<IActionResult> Index(string search)
        {
            var query = _context.Hotels
                .Include(h => h.City)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                // Tránh null: h.City có thể null -> check trước khi truy cập CityName
                query = query.Where(h =>
                    EF.Functions.Like(h.Name, $"%{search}%") ||
                    (h.City != null && EF.Functions.Like(h.City.CityName, $"%{search}%"))
                );
            }

            var hotels = await query
                .OrderBy(h => h.HotelId)
                .ToListAsync();

            ViewBag.Search = search;
            return View(hotels);
        }

        // GET: /Admin/Hotels/Create
        public async Task<IActionResult> Create()
        {
            await LoadCitiesDropDownAsync();
            return View();
        }

        // POST: /Admin/Hotels/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Hotel model, string imageUrl)
        {
            if (!ModelState.IsValid)
            {
                await LoadCitiesDropDownAsync(model.CityId);
                return View(model);
            }

            // 1) Lưu hotel
            _context.Hotels.Add(model);
            await _context.SaveChangesAsync(); // HotelId có giá trị

            // 2) Nếu có URL ảnh -> lưu 1 bản ghi Images
            if (!string.IsNullOrWhiteSpace(imageUrl))
            {
                var image = new Image
                {
                    HotelId = model.HotelId,
                    Url = imageUrl.Trim(),
                    TourId = null,
                    PostId = null
                };
                _context.Images.Add(image);
                await _context.SaveChangesAsync();
            }

            TempData["Message"] = "Thêm khách sạn thành công!";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/Hotels/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var hotel = await _context.Hotels
                .Include(h => h.City)
                .FirstOrDefaultAsync(h => h.HotelId == id.Value);

            if (hotel == null) return NotFound();

            var img = await _context.Images
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.HotelId == hotel.HotelId);
            ViewBag.ImageUrl = img?.Url;

            await LoadCitiesDropDownAsync(hotel.CityId);
            return View(hotel);
        }

        // POST: /Admin/Hotels/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Hotel model, string imageUrl)
        {
            if (id != model.HotelId) return NotFound();

            if (!ModelState.IsValid)
            {
                var imgOld = await _context.Images.AsNoTracking()
                    .FirstOrDefaultAsync(i => i.HotelId == model.HotelId);
                ViewBag.ImageUrl = imgOld?.Url;

                await LoadCitiesDropDownAsync(model.CityId);
                return View(model);
            }

            try
            {
                // Cập nhật hotel
                _context.Entry(model).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                // Ảnh: có URL mới thì cập nhật/thêm
                if (!string.IsNullOrWhiteSpace(imageUrl))
                {
                    var existingImage = await _context.Images
                        .FirstOrDefaultAsync(i => i.HotelId == model.HotelId);

                    if (existingImage != null)
                    {
                        existingImage.Url = imageUrl.Trim();
                        _context.Images.Update(existingImage);
                    }
                    else
                    {
                        var image = new Image
                        {
                            HotelId = model.HotelId,
                            Url = imageUrl.Trim(),
                            TourId = null,
                            PostId = null
                        };
                        _context.Images.Add(image);
                    }

                    await _context.SaveChangesAsync();
                }

                TempData["Message"] = "Cập nhật khách sạn thành công!";
            }
            catch (DbUpdateConcurrencyException)
            {
                var exists = await _context.Hotels.AnyAsync(h => h.HotelId == id);
                if (!exists) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Hotels/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var hotel = await _context.Hotels.FindAsync(id);
            if (hotel == null)
            {
                TempData["Error"] = "Không tìm thấy khách sạn để xóa!";
                return RedirectToAction(nameof(Index));
            }

            // Transaction cho chắc an toàn dữ liệu
            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1) Rooms
                var rooms = await _context.Rooms
                    .Where(r => r.HotelId == id)
                    .ToListAsync();

                if (rooms.Any())
                {
                    var roomIds = rooms.Select(r => r.RoomId).ToList();

                    // 2) Bookings dùng các phòng này
                    var bookings = await _context.Bookings
                        .Where(b => b.RoomId != null && roomIds.Contains(b.RoomId.Value))
                        .ToListAsync();
                    if (bookings.Any()) _context.Bookings.RemoveRange(bookings);

                    // 3) Xóa phòng
                    _context.Rooms.RemoveRange(rooms);
                }

                // 4) Images của hotel
                var images = await _context.Images
                    .Where(i => i.HotelId == id)
                    .ToListAsync();
                if (images.Any()) _context.Images.RemoveRange(images);

                // 5) Reviews của hotel
                var reviews = await _context.Reviews
                    .Where(r => r.HotelId == id)
                    .ToListAsync();
                if (reviews.Any()) _context.Reviews.RemoveRange(reviews);

                // 6) Cuối cùng xóa hotel
                _context.Hotels.Remove(hotel);
                await _context.SaveChangesAsync();

                await tx.CommitAsync();

                TempData["Message"] = "Đã xóa khách sạn và dữ liệu liên quan thành công!";
            }
            catch
            {
                await tx.RollbackAsync();
                TempData["Error"] = "Có lỗi khi xóa dữ liệu, vui lòng thử lại.";
            }

            return RedirectToAction(nameof(Index));
        }

        // Helper: City dropdown
        private async Task LoadCitiesDropDownAsync(int? selectedId = null)
        {
            var cities = await _context.Cities
                .AsNoTracking()
                .OrderBy(c => c.CityName)
                .ToListAsync();

            ViewBag.CityId = new SelectList(cities, "CityId", "CityName", selectedId);
        }
    }
}
