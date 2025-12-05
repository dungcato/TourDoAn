using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; // Thêm dòng này

namespace ToursDULICH.Models;

public partial class Booking
{
    public int BookingId { get; set; }
    public int? UserId { get; set; }
    public int? RoomId { get; set; }
    public int? TourId { get; set; }

    public DateTime? CheckIn { get; set; }
    public DateTime? CheckOut { get; set; }
    public decimal? TotalPrice { get; set; }
    public string? Status { get; set; }

    // [THÊM MỚI] Các trường thông tin khách hàng (Khớp với SQL đã chạy)
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerEmail { get; set; }

    public virtual Room? Room { get; set; }
    public virtual Tour? ToursNavigation { get; set; }
    public virtual User? User { get; set; }
}