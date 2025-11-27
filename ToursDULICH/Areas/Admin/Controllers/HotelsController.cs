using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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

        // 1. DANH SÁCH KHÁCH SẠN
        public async Task<IActionResult> Index(string search)
        {
            var query = _context.Hotels
                .Include(h => h.City) // Nối bảng City để lấy tên thành phố
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(h =>
                    EF.Functions.Like(h.Name, $"%{search}%") ||
                    (h.City != null && EF.Functions.Like(h.City.CityName, $"%{search}%"))
                );
            }

            var hotels = await query.OrderByDescending(h => h.HotelId).ToListAsync();
            ViewBag.Search = search;
            return View(hotels);
        }

        // 2. TRANG THÊM MỚI (GET - Để hiển thị form)
        public async Task<IActionResult> Create()
        {
            // Load danh sách thành phố vào Dropdown
            ViewBag.CityId = new SelectList(await _context.Cities.ToListAsync(), "CityId", "CityName");
            return View();
        }

        // 3. XỬ LÝ LƯU (POST - Khi bấm nút Lưu)
        [HttpPost]
        [ValidateAntiForgeryToken] // <--- QUAN TRỌNG: Dòng này bảo vệ form
        public async Task<IActionResult> Create(Hotel model)
        {
            // Bỏ qua check lỗi các bảng phụ không nhập
            ModelState.Remove("City");
            ModelState.Remove("Rooms");
            ModelState.Remove("Images");
            ModelState.Remove("Reviews");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(model);
                    await _context.SaveChangesAsync();
                    TempData["Message"] = "Thêm khách sạn thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Lỗi lưu database: " + ex.Message;
                }
            }
            else
            {
                // Gom lỗi lại để hiển thị
                var errors = string.Join("; ", ModelState.Values
                                    .SelectMany(v => v.Errors)
                                    .Select(e => e.ErrorMessage));
                TempData["Error"] = "Dữ liệu không hợp lệ: " + errors;
            }

            // Nếu lỗi thì load lại dropdown để không bị mất
            ViewBag.CityId = new SelectList(await _context.Cities.ToListAsync(), "CityId", "CityName", model.CityId);
            return View(model);
        }

        // 4. TRANG SỬA (GET) - Lấy dữ liệu cũ lên form
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var hotel = await _context.Hotels.FindAsync(id);
            if (hotel == null) return NotFound();

            // Load danh sách thành phố, chọn sẵn thành phố hiện tại (hotel.CityId)
            var cities = await _context.Cities.OrderBy(c => c.CityName).ToListAsync();
            ViewBag.CityId = new SelectList(cities, "CityId", "CityName", hotel.CityId);

            return View(hotel);
        }

        // 5. XỬ LÝ SỬA (POST) - Lưu thay đổi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Hotel model)
        {
            if (id != model.HotelId) return NotFound();

            // 1. Bỏ qua validate các bảng phụ để tránh lỗi ModelState.IsValid bị False oan
            ModelState.Remove("City");
            ModelState.Remove("Rooms");
            ModelState.Remove("Images");
            ModelState.Remove("Reviews");

            if (ModelState.IsValid)
            {
                try
                {
                    // --- CÁCH CẬP NHẬT AN TOÀN NHẤT (Fetch & Update) ---
                    // 1. Tìm thằng cũ trong database
                    var existingHotel = await _context.Hotels.FindAsync(id);

                    if (existingHotel == null) return NotFound();

                    // 2. Gán dữ liệu mới từ form vào thằng cũ
                    existingHotel.Name = model.Name;
                    existingHotel.CityId = model.CityId;
                    existingHotel.BasePrice = model.BasePrice;
                    existingHotel.Image = model.Image;
                    existingHotel.Description = model.Description;

                    // --- THÊM DÒNG NÀY VÀO ---
                    existingHotel.Rating = model.Rating;
                    // -------------------------

                    // 3. Lưu lại
                    _context.Update(existingHotel);
                    await _context.SaveChangesAsync();

                    TempData["Message"] = "Cập nhật khách sạn thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Hotels.AnyAsync(e => e.HotelId == id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        TempData["Error"] = "Có người khác đang sửa cùng lúc. Vui lòng thử lại.";
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Lỗi Database: " + ex.Message;
                }
            }
            else
            {
                // Gom lỗi hiển thị ra nếu có
                var errors = string.Join("; ", ModelState.Values
                                                .SelectMany(v => v.Errors)
                                                .Select(e => e.ErrorMessage));
                TempData["Error"] = "Không lưu được: " + errors;
            }

            // --- QUAN TRỌNG: Load lại Dropdown nếu bị lỗi để không bị mất danh sách ---
            var citiesList = await _context.Cities.OrderBy(c => c.CityName).ToListAsync();
            ViewBag.CityId = new SelectList(citiesList, "CityId", "CityName", model.CityId);

            return View(model);
        }


        // 6. XÓA KHÁCH SẠN
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var hotel = await _context.Hotels.FindAsync(id);
            if (hotel != null)
            {
                // Xóa các ràng buộc nếu cần (Code cũ đã có Transaction, ở đây viết gọn lại)
                _context.Hotels.Remove(hotel);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Xóa thành công!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}