using Microsoft.AspNetCore.Mvc;
using ToursDULICH.Models;
using Microsoft.EntityFrameworkCore;
using ToursDULICH.Services;
using Microsoft.AspNetCore.Authorization; // Thêm thư viện này để dùng [Authorize]

namespace ToursDULICH.Controllers
{
    // [QUAN TRỌNG] Thêm Attribute này để bắt buộc phải Đăng nhập mới được vào Controller này
    // Nếu chưa đăng nhập, nó tự đá về trang Login (đã cấu hình trong Program.cs)
    [Authorize]
    public class BookingsController : Controller
    {
        private readonly ToursDuLichContext _context;
        private readonly IConfiguration _configuration;

        public BookingsController(ToursDuLichContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // ==========================================
        // 1. XỬ LÝ ĐẶT PHÒNG KHÁCH SẠN
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> Create(int RoomId, DateTime CheckIn, DateTime CheckOut, string CustomerName, string CustomerPhone, string Status)
        {
            // 1. LẤY USER ID TỪ PHIÊN ĐĂNG NHẬP
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null) return RedirectToAction("Login", "Account"); // Bảo hiểm 2 lớp
            int userId = int.Parse(userIdClaim.Value);

            if (RoomId <= 0) return RedirectToAction("Index", "Hotel");
            if (CheckIn >= CheckOut) CheckOut = CheckIn.AddDays(1);

            var room = await _context.Rooms.FindAsync(RoomId);
            if (room == null) return NotFound();

            int nights = (CheckOut - CheckIn).Days;
            if (nights < 1) nights = 1;
            decimal total = nights * room.Price;

            var booking = new Booking
            {
                RoomId = RoomId,
                CheckIn = CheckIn,
                CheckOut = CheckOut,
                TotalPrice = total,
                Status = "Chờ thanh toán",
                UserId = userId, // [FIX] Dùng ID thật của người đang đăng nhập
                CustomerName = CustomerName, // Lưu tên người đi (có thể khác tên tài khoản)
                CustomerPhone = CustomerPhone
            };

            _context.Add(booking);
            await _context.SaveChangesAsync();

            // Xử lý thanh toán
            if (Status.Contains("Chuyển khoản"))
            {
                var vnPayModel = new VnPayRequestModel
                {
                    Amount = (double)total,
                    CreatedDate = DateTime.Now,
                    Description = $"{CustomerName} thanh toan don {booking.BookingId}",
                    FullName = CustomerName,
                    OrderId = booking.BookingId
                };
                return Redirect(_CreatePaymentUrl(vnPayModel));
            }

            booking.Status = "Đã xác nhận (Tiền mặt)";
            _context.Update(booking);
            await _context.SaveChangesAsync();

            return RedirectToAction("Success", new { id = booking.BookingId });
        }

        // ==========================================
        // 2. XỬ LÝ ĐẶT TOUR
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> CreateTour(int TourId, DateTime CheckIn, int PeopleCount, string CustomerName, string CustomerPhone, string Status)
        {
            // 1. LẤY USER ID TỪ PHIÊN ĐĂNG NHẬP
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null) return RedirectToAction("Login", "Account");
            int userId = int.Parse(userIdClaim.Value);

            var tour = await _context.Tours.FindAsync(TourId);
            if (tour == null) return NotFound();

            decimal price = tour.SalePrice ?? tour.Price;
            decimal total = price * PeopleCount;

            var booking = new Booking
            {
                TourId = TourId,
                CheckIn = CheckIn,
                CheckOut = CheckIn.AddDays(1),
                TotalPrice = total,
                Status = "Chờ thanh toán",
                UserId = userId, // [FIX] Dùng ID thật
                CustomerName = CustomerName,
                CustomerPhone = CustomerPhone
            };

            _context.Add(booking);
            await _context.SaveChangesAsync();

            if (Status.Contains("Chuyển khoản"))
            {
                var vnPayModel = new VnPayRequestModel
                {
                    Amount = (double)total,
                    CreatedDate = DateTime.Now,
                    Description = $"{CustomerName} dat tour {tour.TourId}",
                    FullName = CustomerName,
                    OrderId = booking.BookingId
                };
                return Redirect(_CreatePaymentUrl(vnPayModel));
            }

            booking.Status = "Đã xác nhận (Tiền mặt)";
            _context.Update(booking);
            await _context.SaveChangesAsync();

            return RedirectToAction("Success", new { id = booking.BookingId });
        }

        // ==========================================
        // 3. CÁC HÀM HỖ TRỢ (GIỮ NGUYÊN)
        // ==========================================

        private string _CreatePaymentUrl(VnPayRequestModel model)
        {
            string vnp_Returnurl = "https://localhost:7016/Bookings/PaymentCallback"; // Đổi Port nếu cần
            string vnp_Url = _configuration["VnPay:BaseUrl"];
            string vnp_TmnCode = _configuration["VnPay:TmnCode"];
            string vnp_HashSecret = _configuration["VnPay:HashSecret"];

            VnPayLibrary vnpay = new VnPayLibrary();
            vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
            vnpay.AddRequestData("vnp_Amount", (model.Amount * 100).ToString());
            vnpay.AddRequestData("vnp_CreateDate", model.CreatedDate.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", Utils.GetIpAddress(HttpContext));
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang:" + model.OrderId);
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
            vnpay.AddRequestData("vnp_TxnRef", model.OrderId.ToString());

            return vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
        }

        public async Task<IActionResult> PaymentCallback()
        {
            var response = _configuration.GetSection("VnPay");
            if (Request.Query.Count > 0)
            {
                string vnp_HashSecret = response["HashSecret"];
                var vnpayData = Request.Query;
                VnPayLibrary vnpay = new VnPayLibrary();

                foreach (var s in vnpayData)
                {
                    if (!string.IsNullOrEmpty(s.Key) && s.Key.StartsWith("vnp_"))
                    {
                        vnpay.AddResponseData(s.Key, s.Value);
                    }
                }

                long orderId = Convert.ToInt64(vnpay.GetResponseData("vnp_TxnRef"));
                long vnpErrorCode = Convert.ToInt64(vnpay.GetResponseData("vnp_ResponseCode"));
                bool checkSignature = vnpay.ValidateSignature(vnpay.GetResponseData("vnp_SecureHash"), vnp_HashSecret);

                if (checkSignature)
                {
                    if (vnpErrorCode == 0)
                    {
                        var booking = await _context.Bookings.FindAsync((int)orderId);
                        if (booking != null)
                        {
                            booking.Status = "Đã thanh toán (VNPay)";
                            _context.Update(booking);
                            await _context.SaveChangesAsync();
                        }
                        return RedirectToAction("Success", new { id = orderId });
                    }
                    else
                    {
                        return RedirectToAction("PaymentFail");
                    }
                }
            }
            return RedirectToAction("PaymentFail");
        }

        [AllowAnonymous] // Cho phép vào trang này mà không cần login (để redirect lỗi)
        public IActionResult PaymentFail()
        {
            return View();
        }

        public async Task<IActionResult> Success(int id)
        {
            // Lấy ID người dùng hiện tại
            var userIdClaim = User.FindFirst("UserId");
            int currentUserId = userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;

            var booking = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.ToursNavigation)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            // Bảo mật: Chỉ người đặt mới xem được đơn của mình (hoặc Admin)
            if (booking == null || (booking.UserId != currentUserId && !User.IsInRole("Admin")))
            {
                return RedirectToAction("Index", "Home");
            }

            return View(booking);
        }
        [HttpGet]
        public async Task<IActionResult> History()
        {
            // 1. Lấy ID người dùng đang đăng nhập
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null)
            {
                return RedirectToAction("Login", "Account");
            }
            int userId = int.Parse(userIdClaim.Value);

            // 2. Lấy danh sách đơn hàng của người đó (Sắp xếp mới nhất lên đầu)
            var history = await _context.Bookings
                .Include(b => b.Room).ThenInclude(r => r.Hotel) // Lấy thông tin Phòng + Khách sạn
                .Include(b => b.ToursNavigation)                // Lấy thông tin Tour
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.BookingId)
                .ToListAsync();

            return View(history);
        }
        }

    public class VnPayRequestModel
    {
        public int OrderId { get; set; }
        public string FullName { get; set; }
        public string Description { get; set; }
        public double Amount { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}