using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToursDULICH.Models;   // namespace context + models của m
using System.Linq;

namespace ToursDULICH.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        private readonly ToursDuLichContext _context;

        public HomeController(ToursDuLichContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // ===== THỐNG KÊ SỐ LƯỢNG =====
            ViewBag.TotalHotels = _context.Hotels.Count();
            ViewBag.TotalRooms = _context.Rooms.Count();
            ViewBag.TotalTours = _context.Tours.Count();
            ViewBag.TotalBookings = _context.Bookings.Count();
            ViewBag.TotalUsers = _context.Users.Count();
            ViewBag.TotalContacts = _context.Contacts.Count();

            // ===== ĐƠN ĐẶT MỚI NHẤT (kèm User + Tour + Room) =====
            var latestBookings = _context.Bookings
                .Include(b => b.User)              // User
                .Include(b => b.Room)              // Room
                .Include(b => b.ToursNavigation)   // Tour (property m vừa có)
                .OrderByDescending(b => b.BookingId)
                .Take(10)
                .ToList();

            // Truyền list booking ra view
            return View(latestBookings);
        }
    }
}
