using System;
using System.Collections.Generic;

namespace ToursDULICH.Models;

public partial class Booking
{
    public int BookingId { get; set; }
    public int? UserId { get; set; }
    public int? RoomId { get; set; }

    // [SỬA] Đổi từ Tours -> TourId cho khớp DB
    public int? TourId { get; set; }

    public DateTime? CheckIn { get; set; } // Lưu ý: DB bạn là DateTime hay DateOnly? Nếu lỗi thì đổi về DateTime
    public DateTime? CheckOut { get; set; }
    public decimal? TotalPrice { get; set; }
    public string? Status { get; set; }

    public virtual Room? Room { get; set; }

    // [SỬA] ToursNavigation là tên quan hệ, giữ nguyên hoặc đổi thành Tour đều được
    public virtual Tour? ToursNavigation { get; set; }
    public virtual User? User { get; set; }
}