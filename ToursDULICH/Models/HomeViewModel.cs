using System.Collections.Generic;

namespace ToursDULICH.Models
{
    public class HomeViewModel
    {
        public IEnumerable<Tour> FeaturedTours { get; set; }
        public IEnumerable<Hotel> FeaturedHotels { get; set; }
    }
}