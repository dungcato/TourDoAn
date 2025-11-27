using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ToursDULICH.Models;

namespace ToursDULICH.Controllers
{
    public class HotelController : Controller
    {
        private readonly ToursDuLichContext _context;

        public HotelController(ToursDuLichContext context)
        {
            _context = context;
        }

        // 1. TRANG DANH SÁCH (Thay thế cho hotel.cshtml cũ)
        // GET: /Hotel/Index
        // GET: /Hotel/Index
        public async Task<IActionResult> Index(string search, int? cityId, int? rating)
        {
            var query = _context.Hotels.Include(h => h.City).AsQueryable();

            // 1. Tìm theo tên
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(h => h.Name.Contains(search));
            }

            // 2. Lọc theo địa điểm
            if (cityId.HasValue && cityId > 0)
            {
                query = query.Where(h => h.CityId == cityId);
            }

            // 3. LỌC THEO SAO (MỚI)
            // Nếu chọn 4 sao thì hiện các khách sạn >= 4 sao
            if (rating.HasValue && rating > 0)
            {
                query = query.Where(h => h.Rating >= rating);
            }

            var hotels = await query.OrderByDescending(h => h.HotelId).ToListAsync();

            ViewBag.CityList = new SelectList(await _context.Cities.ToListAsync(), "CityId", "CityName", cityId);
            ViewBag.CurrentSearch = search;

            // Lưu lại sao đang chọn để tích vào ô radio bên View
            ViewBag.CurrentRating = rating;

            return View(hotels);
        }

        // 2. TRANG CHI TIẾT (Thay thế cho hotel-single.cshtml cũ)
        public async Task<IActionResult> Detail(int id)
        {
            // Lấy thông tin khách sạn + Thành phố + Danh sách Phòng
            var hotel = await _context.Hotels
                .Include(h => h.City)
                .Include(h => h.Rooms) // Quan trọng: Lấy kèm danh sách phòng để hiện thị
                .FirstOrDefaultAsync(h => h.HotelId == id);

            if (hotel == null) return NotFound();

            return View(hotel);
        }
    }
}