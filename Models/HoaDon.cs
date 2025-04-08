using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lab03.Models
{
    public class HoaDon
    {
        [Key]
        public int Id { get; set; }

        // Thông tin khách hàng
        public string ApplicationUserId { get; set; }
        [ForeignKey("ApplicationUserId")]
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }

        public string CustomerName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string ShippingAddress { get; set; }

        // Thông tin hóa đơn
        public DateTime OrderDate { get; set; } = DateTime.Now; // Tự động gán ngày đặt hàng
        public string OrderStatus { get; set; } = "Đang chờ xác nhận"; // Trạng thái mặc định

        public string PaymentMethod { get; set; }

        // Chi phí và tính toán
        public decimal ShippingCost { get; set; } = 0;
        public decimal? TaxAmount { get; set; } = 0;
        public decimal Total { get; set; } // Tổng tiền đơn hàng sau tính toán

        public string Note { get; set; } // Ghi chú đơn hàng nếu có

        // Số tiền giảm giá
        public decimal? DiscountAmount { get; set; } = 0;

        public ICollection<ChiTietHoaDon> ChiTietHoaDons { get; set; }
    }
}
