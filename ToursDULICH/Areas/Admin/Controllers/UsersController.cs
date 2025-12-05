using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToursDULICH.Models;

namespace ToursDULICH.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] // Chỉ Admin mới được vào
    public class UsersController : Controller
    {
        private readonly ToursDuLichContext _context;

        public UsersController(ToursDuLichContext context)
        {
            _context = context;
        }

        // 1. DANH SÁCH NGƯỜI DÙNG
        public async Task<IActionResult> Index()
        {
            // Lấy tất cả user, sắp xếp theo tên
            var users = await _context.Users
                .OrderByDescending(u => u.UserId)
                .ToListAsync();
            return View(users);
        }

        // 2. THÊM MỚI (Admin tạo hộ khách)
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra email trùng
                if (await _context.Users.AnyAsync(u => u.Email == user.Email))
                {
                    ModelState.AddModelError("Email", "Email này đã tồn tại!");
                    return View(user);
                }

                // Nếu Admin không chọn quyền, mặc định là Customer
                if (string.IsNullOrEmpty(user.Role)) user.Role = "Customer";

                _context.Add(user);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Thêm người dùng thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // 3. CẬP NHẬT
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, User user)
        {
            if (id != user.UserId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                    TempData["Message"] = "Cập nhật thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Users.Any(e => e.UserId == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // 4. XÓA NGƯỜI DÙNG
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                // Chặn xóa chính mình (đang đăng nhập)
                if (User.Identity.Name == user.FullName)
                {
                    TempData["Error"] = "Không thể xóa tài khoản đang đăng nhập!";
                    return RedirectToAction(nameof(Index));
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Đã xóa người dùng!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}