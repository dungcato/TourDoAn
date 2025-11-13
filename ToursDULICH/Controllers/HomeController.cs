using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ToursDULICH.Models;

namespace ToursDULICH.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        public IActionResult login()
        {
            return View();
        }

        public IActionResult register()
        {
            return View();
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult about()
        {
            return View();
        }

        public IActionResult tour()
        {
            return View();
        }

        public IActionResult hotel()
        {
            return View();
        }

        public IActionResult blog()
        {
            return View();
        }

        public IActionResult contact()
        {
            return View();
        }

        // Trang chi tiết KHÁCH SẠN
        public IActionResult HotelSingle(int id)
        {
            ViewBag.HotelId = id;            // nếu muốn dùng sau này
            return View("hotel-single");     // Views/Home/hotel-single.cshtml
        }

        // Trang chi tiết / ĐẶT TOUR DU LỊCH
        public IActionResult TourSingle(int id)
        {
            ViewBag.TourId = id;             // nếu muốn hiển thị theo id
            return View("tour-single");      // Views/Home/tour-single.cshtml
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
