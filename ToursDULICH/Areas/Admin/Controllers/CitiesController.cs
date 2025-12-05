using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToursDULICH.Models;

namespace ToursDULICH.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CitiesController : Controller
    {
        private readonly ToursDuLichContext _context;

        public CitiesController(ToursDuLichContext context)
        {
            _context = context;
        }

        // 1. DANH SÁCH (Hiển thị kèm số lượng Tour/Hotel đang có)
        public async Task<IActionResult> Index()
        {
            var cities = await _context.Cities
                .Include(c => c.Tours)
                .Include(c => c.Hotels)
                .OrderBy(c => c.CityName)
                .ToListAsync();
            return View(cities);
        }

        // 2. TẠO MỚI
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(City city)
        {
            // Bỏ qua validate các trường quan hệ (Tours, Hotels) để tránh lỗi ModelState
            ModelState.Remove("Tours");
            ModelState.Remove("Hotels");

            if (ModelState.IsValid)
            {
                // [FIX] Code cũ của bạn thiếu lưu Image, ở đây Model Binder sẽ tự map nếu form view đúng tên "Image"
                _context.Add(city);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Thêm địa điểm thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(city);
        }

        // 3. CẬP NHẬT (GET)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var city = await _context.Cities.FindAsync(id);
            if (city == null) return NotFound();
            return View(city);
        }

        // 3. CẬP NHẬT (POST) - ĐÃ SỬA LOGIC LƯU ẢNH
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, City city)
        {
            if (id != city.CityId) return NotFound();

            ModelState.Remove("Tours");
            ModelState.Remove("Hotels");

            if (ModelState.IsValid)
            {
                try
                {
                    // [QUAN TRỌNG] Kỹ thuật Fetch & Update để không mất ảnh nếu user không chọn ảnh mới
                    var existingCity = await _context.Cities.FindAsync(id);

                    if (existingCity == null) return NotFound();

                    // Cập nhật từng trường
                    existingCity.CityName = city.CityName;
                    existingCity.Country = city.Country;

                    // Chỉ cập nhật ảnh nếu người dùng có nhập link ảnh mới
                    // Nếu ô ảnh để trống, giữ nguyên ảnh cũ
                    if (!string.IsNullOrEmpty(city.Image))
                    {
                        existingCity.Image = city.Image;
                    }

                    _context.Update(existingCity);
                    await _context.SaveChangesAsync();

                    TempData["Message"] = "Cập nhật thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Cities.Any(e => e.CityId == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(city);
        }

        // 4. XÓA
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var city = await _context.Cities
                .Include(c => c.Tours)
                .Include(c => c.Hotels)
                .FirstOrDefaultAsync(c => c.CityId == id);

            if (city != null)
            {
                if (city.Tours.Any() || city.Hotels.Any())
                {
                    TempData["Error"] = $"Không thể xóa {city.CityName} vì đang có dữ liệu Tour/Khách sạn liên quan!";
                }
                else
                {
                    _context.Cities.Remove(city);
                    await _context.SaveChangesAsync();
                    TempData["Message"] = "Xóa địa điểm thành công!";
                }
            }
            return RedirectToAction(nameof(Index));
        }
    }
}