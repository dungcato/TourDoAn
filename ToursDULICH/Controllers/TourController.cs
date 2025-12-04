using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ToursDULICH.Models;

namespace ToursDULICH.Controllers
{
    public class TourController : Controller
    {
        private readonly ToursDuLichContext _context;

        public TourController(ToursDuLichContext context)
        {
            _context = context;
        }

        // 1. DANH SÁCH TOUR (Có bộ lọc xịn)
        public async Task<IActionResult> Index(string search, int? cityId, string sortOrder)
        {
            var query = _context.Tours.Include(t => t.City).AsQueryable();

            // Lọc theo tên hoặc điểm khởi hành
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t => t.Name.Contains(search) || t.StartPoint.Contains(search));
            }

            // Lọc theo địa điểm (City)
            if (cityId.HasValue && cityId > 0)
            {
                query = query.Where(t => t.CityId == cityId);
            }

            // Sắp xếp giá
            switch (sortOrder)
            {
                case "price_asc":
                    query = query.OrderBy(t => (t.SalePrice ?? t.Price));
                    break;
                case "price_desc":
                    query = query.OrderByDescending(t => (t.SalePrice ?? t.Price));
                    break;
                default:
                    query = query.OrderByDescending(t => t.TourId); // Mới nhất lên đầu
                    break;
            }

            var tours = await query.ToListAsync();

            // Đổ dữ liệu vào Dropdown lọc
            ViewBag.CityList = new SelectList(await _context.Cities.OrderBy(c => c.CityName).ToListAsync(), "CityId", "CityName", cityId);
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentSort = sortOrder;

            return View(tours);
        }

        // 2. CHI TIẾT TOUR
        public async Task<IActionResult> Detail(int id)
        {
            if (id <= 0) return NotFound();

            var tour = await _context.Tours
                .Include(t => t.City)
                .Include(t => t.Images) // Lấy thêm ảnh phụ nếu có
                .Include(t => t.Reviews).ThenInclude(r => r.User) // Lấy đánh giá
                .FirstOrDefaultAsync(t => t.TourId == id);

            if (tour == null) return NotFound();

            return View(tour);
        }
    }
}