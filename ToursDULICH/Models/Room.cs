using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ToursDULICH.Models
{
    [Table("Rooms")]
    public class Room
    {
        public Room()
        {
            // Khởi tạo danh sách để tránh lỗi Null
            Bookings = new HashSet<Booking>();
        }

        [Key]
        public int RoomId { get; set; }

        [Display(Name = "Tên Phòng")]
        [Required(ErrorMessage = "Tên phòng không được để trống")]
        public string RoomName { get; set; } = string.Empty; // Fix lỗi Non-nullable

        [Display(Name = "Giá phòng")]
        public decimal Price { get; set; } // Fix lỗi PricePerNight

        [Display(Name = "Ảnh đại diện")]
        public string? Image { get; set; }

        [Display(Name = "Mô tả tiện ích")]
        public string? Description { get; set; }

        [Display(Name = "Thuộc Khách sạn")]
        public int HotelId { get; set; }

        [ForeignKey("HotelId")]
        public virtual Hotel? Hotel { get; set; }

        // Thêm cái này để Controller check được "Phòng này có đơn đặt chưa"
        public virtual ICollection<Booking> Bookings { get; set; }
    }
}