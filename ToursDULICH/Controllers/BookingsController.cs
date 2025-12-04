using Microsoft.AspNetCore.Mvc;
using ToursDULICH.Models;
using Microsoft.EntityFrameworkCore;
using ToursDULICH.Services;

namespace ToursDULICH.Controllers
{
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
                UserId = 1,
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
        // 2. XỬ LÝ ĐẶT TOUR (FIX LỖI CỦA BẠN Ở ĐÂY)
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> CreateTour(int TourId, DateTime CheckIn, int PeopleCount, string CustomerName, string CustomerPhone, string Status)
        {
            var tour = await _context.Tours.FindAsync(TourId);
            if (tour == null) return NotFound();

            // Tính giá: Giá (gốc hoặc sale) * Số người
            decimal price = tour.SalePrice ?? tour.Price;
            decimal total = price * PeopleCount;

            var booking = new Booking
            {
                TourId = TourId,
                CheckIn = CheckIn,
                CheckOut = CheckIn.AddDays(1),
                TotalPrice = total,
                Status = "Chờ thanh toán", // Mặc định chờ
                UserId = 1,
            };

            _context.Add(booking);
            await _context.SaveChangesAsync();

            // Logic thanh toán VNPay
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

            // Nếu tiền mặt thì update luôn
            booking.Status = "Đã xác nhận (Tiền mặt)";
            _context.Update(booking);
            await _context.SaveChangesAsync();

            return RedirectToAction("Success", new { id = booking.BookingId });
        }

        // ==========================================
        // 3. CÁC HÀM HỖ TRỢ VNPAY & CALLBACK
        // ==========================================

        // Tạo URL thanh toán VNPay
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

            string paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
            return paymentUrl;
        }

        // Xử lý khi Ngân hàng trả về
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

        public IActionResult PaymentFail()
        {
            return View();
        }

        public async Task<IActionResult> Success(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.ToursNavigation) // Thêm cái này để hiển thị tên Tour
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.BookingId == id);
            return View(booking);
        }
    }

    // Class phụ để truyền dữ liệu (Để ở đây hoặc tách file đều được)
    public class VnPayRequestModel
    {
        public int OrderId { get; set; }
        public string FullName { get; set; }
        public string Description { get; set; }
        public double Amount { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}