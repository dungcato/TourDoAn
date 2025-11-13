using System;
using System.Collections.Generic;

namespace ToursDULICH.Models;

public partial class Tour
{
    public int TourId { get; set; }

    public string? Name { get; set; }

    public int? CityId { get; set; }

    public string? Description { get; set; }

    public decimal? Price { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual City? City { get; set; }

    public virtual ICollection<Image> Images { get; set; } = new List<Image>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}
