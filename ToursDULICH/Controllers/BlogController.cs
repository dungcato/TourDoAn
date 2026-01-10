using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToursDULICH.Models;

namespace ToursDULICH.Controllers
{
    public class BlogController : Controller
    {
        private readonly ToursDuLichContext _context;

        public BlogController(ToursDuLichContext context)
        {
            _context = context;
        }

        // 1. TRANG DANH SÁCH TIN TỨC
        public async Task<IActionResult> Index(string search)
        {
            var query = _context.BlogPosts
                .Include(p => p.Author)
                .Include(p => p.Images)
                .Include(p => p.Comments)
                .AsQueryable();

            // Tính năng tìm kiếm (nếu có nhập từ khóa)
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Title.Contains(search) || p.Content.Contains(search));
                ViewBag.SearchTerm = search;
            }

            var posts = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
            return View(posts);
        }

        // 2. TRANG CHI TIẾT BÀI VIẾT
        public async Task<IActionResult> Detail(int id)
        {
            // Lấy bài viết chính + Bình luận
            var post = await _context.BlogPosts
                .Include(p => p.Author)
                .Include(p => p.Images)
                .Include(p => p.Comments)
                .FirstOrDefaultAsync(p => p.PostId == id);

            if (post == null) return NotFound();

            // --- PHẦN SIDEBAR ĐỘNG (Cột bên phải) ---

            // 1. Lấy 3 bài viết mới nhất (trừ bài đang xem)
            ViewBag.RecentPosts = await _context.BlogPosts
                .Where(p => p.PostId != id)
                .OrderByDescending(p => p.CreatedAt)
                .Take(3)
                .Include(p => p.Images)
                .Include(p => p.Comments)
                .ToListAsync();

            // 2. Đếm số lượng dịch vụ
            ViewBag.TourCount = await _context.Tours.CountAsync();
            ViewBag.HotelCount = await _context.Hotels.CountAsync();
            ViewBag.PostCount = await _context.BlogPosts.CountAsync();
            ViewBag.CityCount = await _context.Cities.CountAsync();

            // Sắp xếp bình luận: Cũ trước, Mới sau (để đọc theo thứ tự)
            post.Comments = post.Comments.OrderBy(c => c.CreatedAt).ToList();

            return View(post);
        }

        // 3. XỬ LÝ KHÁCH GỬI BÌNH LUẬN
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LeaveComment(int PostId, string Name, string Message)
        {
            try
            {
                if (string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(Message))
                {
                    TempData["Error"] = "Vui lòng nhập tên và nội dung!";
                    return RedirectToAction("Detail", new { id = PostId });
                }

                var comment = new Comment
                {
                    PostId = PostId,
                    UserName = Name,
                    Content = Message,
                    CreatedAt = DateTime.Now
                };

                _context.Comments.Add(comment);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Gửi bình luận thành công!";
            }
            catch
            {
                TempData["Error"] = "Có lỗi xảy ra, vui lòng thử lại.";
            }

            // Quay lại đúng bài viết đó
            return RedirectToAction("Detail", new { id = PostId });
        }
    }
}