using System;
using System.Collections.Generic;

namespace ToursDULICH.Areas.Admin.Models
{
    public class DashboardViewModel
    {
        public int TotalHotels { get; set; }
        public int TotalTours { get; set; }
        public int TotalRooms { get; set; }
        public int TotalBookings { get; set; }
        public int TotalUsers { get; set; }

        public List<RecentBookingItem> RecentBookings { get; set; } = new();
    }

    public class RecentBookingItem
    {
        public int BookingId { get; set; }
        public string? UserName { get; set; }
        public string? ServiceName { get; set; }   // tên tour hoặc phòng/khách sạn
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public string Status { get; set; } = "";
        public decimal TotalPrice { get; set; }
    }
}
