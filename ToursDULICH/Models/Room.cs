using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ToursDULICH.Models
{
    [Table("Rooms")]
    public class Room
    {
        public Room()
        {
            Bookings = new HashSet<Booking>();
        }

        [Key]
        public int RoomId { get; set; }

        // [SỬA] Database của bạn là RoomType, không phải RoomName
        [Column("RoomType")]
        [Display(Name = "Tên/Loại Phòng")]
        public string RoomName { get; set; }

        // [SỬA] Database của bạn là PricePerNight, không phải Price
        [Column("PricePerNight")]
        [Display(Name = "Giá phòng")]
        public decimal Price { get; set; }

        public string? Image { get; set; }
        public string? Description { get; set; }

        public int HotelId { get; set; }
        [ForeignKey("HotelId")]
        public virtual Hotel? Hotel { get; set; }

        public virtual ICollection<Booking> Bookings { get; set; }
    }
}