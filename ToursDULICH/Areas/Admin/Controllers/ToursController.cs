using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ToursDULICH.Models;

namespace ToursDULICH.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ToursController : Controller
    {
        private readonly ToursDuLichContext _context;

        public ToursController(ToursDuLichContext context) => _context = context;

        // 1. DANH SÁCH (Có tìm kiếm + Lọc thành phố)
        public async Task<IActionResult> Index(string search, int? cityId)
        {
            var query = _context.Tours.Include(t => t.City).AsNoTracking().AsQueryable();

            // Tìm kiếm
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t => t.Name.Contains(search) || t.StartPoint.Contains(search));
            }
            // Lọc theo điểm đến
            if (cityId.HasValue && cityId > 0)
            {
                query = query.Where(t => t.CityId == cityId);
            }

            var tours = await query.OrderByDescending(t => t.TourId).ToListAsync();

            // Load danh sách thành phố cho Dropdown bộ lọc
            ViewBag.CityList = new SelectList(await _context.Cities.OrderBy(c => c.CityName).ToListAsync(), "CityId", "CityName", cityId);
            ViewBag.Search = search;

            return View(tours);
        }

        // 2. FORM THÊM MỚI
        public IActionResult Create()
        {
            ViewBag.CityId = new SelectList(_context.Cities, "CityId", "CityName");
            return View();
        }

        // 3. XỬ LÝ THÊM MỚI
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Tour model)
        {
            ModelState.Remove("City"); // Bỏ qua validate City object

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(model);
                    await _context.SaveChangesAsync();
                    TempData["Message"] = "Thêm tour mới thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Lỗi lưu dữ liệu: " + ex.Message;
                }
            }
            else
            {
                TempData["Error"] = "Vui lòng kiểm tra lại thông tin nhập vào.";
            }

            ViewBag.CityId = new SelectList(_context.Cities, "CityId", "CityName", model.CityId);
            return View(model);
        }

        // 4. FORM SỬA
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var tour = await _context.Tours.FindAsync(id);
            if (tour == null) return NotFound();

            ViewBag.CityId = new SelectList(_context.Cities, "CityId", "CityName", tour.CityId);
            return View(tour);
        }

        // 5. XỬ LÝ SỬA
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Tour model)
        {
            if (id != model.TourId) return NotFound();
            ModelState.Remove("City");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(model);
                    await _context.SaveChangesAsync();
                    TempData["Message"] = "Cập nhật tour thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Lỗi: " + ex.Message;
                }
            }
            ViewBag.CityId = new SelectList(_context.Cities, "CityId", "CityName", model.CityId);
            return View(model);
        }

        // 6. XÓA
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var tour = await _context.Tours.FindAsync(id);
            if (tour != null)
            {
                _context.Tours.Remove(tour);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Xóa tour thành công!";
            }
            else
            {
                TempData["Error"] = "Không tìm thấy tour!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}