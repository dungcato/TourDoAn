using System;
using System.Collections.Generic;

namespace ToursDULICH.Models;

public partial class Hotel
{
    public int HotelId { get; set; }

    public string? Name { get; set; }

    public int? CityId { get; set; }

    public string? Description { get; set; }

    public decimal? BasePrice { get; set; }

    public virtual City? City { get; set; }

    public virtual ICollection<Image> Images { get; set; } = new List<Image>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
}
