using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToursDULICH.Models;

namespace ToursDULICH.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class BlogPostsController : Controller
    {
        private readonly ToursDuLichContext _context;

        public BlogPostsController(ToursDuLichContext context)
        {
            _context = context;
        }

        // 1. DANH SÁCH BÀI VIẾT
        public async Task<IActionResult> Index()
        {
            var posts = await _context.BlogPosts
                .Include(p => p.Author)
                .Include(p => p.Images) // Load ảnh để hiện thumbnail
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
            return View(posts);
        }

        // 2. THÊM MỚI (GET)
        public IActionResult Create()
        {
            return View();
        }

        // 3. THÊM MỚI (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BlogPost blogPost, string FeatureImage)
        {
            if (ModelState.IsValid)
            {
                // Tự động gán ngày tạo và Tác giả (Tạm lấy User đầu tiên nếu chưa login)
                blogPost.CreatedAt = DateTime.Now;

                // Lấy ID người đang đăng nhập (nếu có), ko thì gán mặc định 1
                var userEmail = User.Identity?.Name;
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
                blogPost.AuthorId = user?.UserId ?? 1;

                _context.Add(blogPost);
                await _context.SaveChangesAsync(); // Lưu bài viết trước để có ID

                // Xử lý ảnh đại diện (Lưu vào bảng Images)
                if (!string.IsNullOrEmpty(FeatureImage))
                {
                    var img = new Image
                    {
                        PostId = blogPost.PostId,
                        Url = FeatureImage
                    };
                    _context.Images.Add(img);
                    await _context.SaveChangesAsync();
                }

                TempData["Message"] = "Thêm bài viết thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(blogPost);
        }

        // 4. SỬA (GET)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var blogPost = await _context.BlogPosts
                .Include(p => p.Images)
                .FirstOrDefaultAsync(m => m.PostId == id);

            if (blogPost == null) return NotFound();

            // Lấy ảnh đầu tiên ra để hiển thị lại vào ô input
            ViewBag.CurrentImage = blogPost.Images.FirstOrDefault()?.Url ?? "";

            return View(blogPost);
        }

        // 5. SỬA (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BlogPost blogPost, string FeatureImage)
        {
            if (id != blogPost.PostId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Giữ nguyên ngày tạo và tác giả cũ (chỉ update nội dung)
                    var oldPost = await _context.BlogPosts.AsNoTracking().FirstOrDefaultAsync(p => p.PostId == id);
                    blogPost.CreatedAt = oldPost?.CreatedAt;
                    blogPost.AuthorId = oldPost?.AuthorId;

                    _context.Update(blogPost);
                    await _context.SaveChangesAsync();

                    // Cập nhật ảnh đại diện:
                    // 1. Xóa ảnh cũ của bài này
                    var oldImages = _context.Images.Where(i => i.PostId == id);
                    _context.Images.RemoveRange(oldImages);

                    // 2. Thêm ảnh mới
                    if (!string.IsNullOrEmpty(FeatureImage))
                    {
                        _context.Images.Add(new Image { PostId = id, Url = FeatureImage });
                    }
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BlogPostExists(blogPost.PostId)) return NotFound();
                    else throw;
                }
                TempData["Message"] = "Cập nhật thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(blogPost);
        }

        // 6. XÓA
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var blogPost = await _context.BlogPosts.FindAsync(id);
            if (blogPost != null)
            {
                // Phải xóa ảnh và comment liên quan trước (nếu database không set Cascade Delete)
                var images = _context.Images.Where(i => i.PostId == id);
                _context.Images.RemoveRange(images);

                var comments = _context.Comments.Where(c => c.PostId == id);
                _context.Comments.RemoveRange(comments);

                _context.BlogPosts.Remove(blogPost);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool BlogPostExists(int id)
        {
            return _context.BlogPosts.Any(e => e.PostId == id);
        }
    }
}