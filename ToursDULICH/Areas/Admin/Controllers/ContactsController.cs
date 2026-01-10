using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToursDULICH.Models;

namespace ToursDULICH.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] // Chỉ Admin mới được vào xem
    public class ContactsController : Controller
    {
        private readonly ToursDuLichContext _context;

        public ContactsController(ToursDuLichContext context)
        {
            _context = context;
        }

        // 1. DANH SÁCH LIÊN HỆ
        public async Task<IActionResult> Index()
        {
            // Lấy danh sách, tin mới nhất nằm trên cùng
            var items = await _context.Contacts
                .OrderByDescending(c => c.ContactId) // Hoặc c.CreatedAt nếu bảng có cột này
                .ToListAsync();

            return View(items);
        }

        // 2. XÓA LIÊN HỆ (Để xóa tin nhắn rác)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var contact = await _context.Contacts.FindAsync(id);
            if (contact != null)
            {
                _context.Contacts.Remove(contact);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Đã xóa tin nhắn liên hệ này!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}