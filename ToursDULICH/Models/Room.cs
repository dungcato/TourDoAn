using System;
using System.Collections.Generic;

namespace ToursDULICH.Models;

public partial class Room
{
    public int RoomId { get; set; }

    public int? HotelId { get; set; }

    public string? RoomType { get; set; }

    public decimal? PricePerNight { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual Hotel? Hotel { get; set; }
}
