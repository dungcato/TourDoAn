using System;
using System.Collections.Generic;

namespace ToursDULICH.Models;

public partial class Image
{
    public int ImageId { get; set; }

    public string? Url { get; set; }

    public int? HotelId { get; set; }

    public int? TourId { get; set; }

    public int? PostId { get; set; }

    public virtual Hotel? Hotel { get; set; }

    public virtual BlogPost? Post { get; set; }

    public virtual Tour? Tour { get; set; }
}
