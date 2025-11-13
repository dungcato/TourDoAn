using System;
using System.Collections.Generic;

namespace ToursDULICH.Models;

public partial class Booking
{
    public int BookingId { get; set; }

    public int? UserId { get; set; }

    public int? RoomId { get; set; }

    public DateOnly? CheckIn { get; set; }

    public DateOnly? CheckOut { get; set; }

    public decimal? TotalPrice { get; set; }

    public string? Status { get; set; }

    public int? Tours { get; set; }

    public virtual Room? Room { get; set; }

    public virtual Tour? ToursNavigation { get; set; }

    public virtual User? User { get; set; }
}
