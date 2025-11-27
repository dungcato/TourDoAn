using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ToursDULICH.Models
{
    [Table("Tours")]
    public class Tour
    {
        // Hàm tạo để khởi tạo danh sách, tránh lỗi Null khi chạy
        public Tour()
        {
            Bookings = new HashSet<Booking>();
            Images = new HashSet<Image>();   // <-- Thêm cái này
            Reviews = new HashSet<Review>(); // <-- Thêm cái này
        }

        [Key]
        public int TourId { get; set; }

        [Display(Name = "Tên Tour")]
        [Required(ErrorMessage = "Vui lòng nhập tên tour")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Ảnh đại diện")]
        public string? Image { get; set; }

        [Display(Name = "Mô tả chi tiết")]
        public string? Description { get; set; }

        [Display(Name = "Giá gốc")]
        public decimal Price { get; set; }

        [Display(Name = "Giá khuyến mãi")]
        public decimal? SalePrice { get; set; }

        [Display(Name = "Thời gian")]
        public string? Duration { get; set; }

        [Display(Name = "Điểm khởi hành")]
        public string? StartPoint { get; set; }

        [Display(Name = "Địa điểm đến")]
        public int CityId { get; set; }
        [ForeignKey("CityId")]
        public virtual City? City { get; set; }

        // --- CÁC QUAN HỆ (NAVIGATION PROPERTIES) ---
        public virtual ICollection<Booking> Bookings { get; set; }

        // Thêm 2 dòng này để sửa lỗi Controller
        public virtual ICollection<Image> Images { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
    }
}