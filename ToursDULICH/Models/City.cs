using System;
using System.Collections.Generic;

namespace ToursDULICH.Models;

public partial class City
{
    public int CityId { get; set; }

    public string? CityName { get; set; }

    public string? Country { get; set; }

    public virtual ICollection<Hotel> Hotels { get; set; } = new List<Hotel>();

    public virtual ICollection<Tour> Tours { get; set; } = new List<Tour>();
}
