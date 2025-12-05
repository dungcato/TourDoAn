using System.Collections.Generic;

namespace ToursDULICH.Models
{
    public class HomeViewModel
    {
        public IEnumerable<Tour> FeaturedTours { get; set; }
        public IEnumerable<Hotel> FeaturedHotels { get; set; }

        // [THÊM MỚI] Danh sách các địa điểm nổi bật
        public IEnumerable<City> FeaturedCities { get; set; }
    }
}