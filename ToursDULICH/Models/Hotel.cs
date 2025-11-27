using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ToursDULICH.Models
{
    [Table("Hotels")]
    public class Hotel
    {
        [Key]
        public int HotelId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên khách sạn")]
        [Display(Name = "Tên Khách Sạn")]
        public string Name { get; set; }

        [Display(Name = "Thành phố")]
        public int CityId { get; set; }
        [ForeignKey("CityId")]
        public virtual City City { get; set; }

        [Display(Name = "Giá cơ bản")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal BasePrice { get; set; }

        [Display(Name = "Mô tả chi tiết")]
        public string Description { get; set; }

        // ...

        [Display(Name = "Hạng sao")]
        public int Rating { get; set; } // Thêm dòng này

        [Display(Name = "Ảnh đại diện")] // <--- Dán nó xuống đây mới đúng chỗ
        public string Image { get; set; }

        // ...

        // --- THÊM 3 DÒNG NÀY VÀO ĐỂ FIX LỖI ---
        // Đây là cầu nối để Entity Framework biết 1 Khách sạn có nhiều Phòng, Ảnh, Đánh giá
        public virtual ICollection<Room> Rooms { get; set; }
        public virtual ICollection<Image> Images { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
    }
}