using System.ComponentModel.DataAnnotations;

namespace Lab03.Models
{
    public class SalesRecord
    {
        [Key]
        public int Id { get; set; }  // Khóa chính

        public string CustomerName { get; set; }           // Tên khách hàng
        public string ProductName { get; set; }            // Tên sản phẩm
        public string ProductCategory { get; set; }        // Loại sản phẩm
        public DateTime SaleDate { get; set; }             // Ngày bán
        public decimal PurchasePrice { get; set; }         // Giá nhập
        public decimal SalePrice { get; set; }             // Giá bán
        public decimal TaxAmount { get; set; }             // Tiền thuế
        public string ShippingAddress { get; set; }        // Địa chỉ vận chuyển
        public decimal ActualProfit { get; set; }          // Lợi nhuận thực tế
        public decimal ShippingCost { get; set; }          // Phí vận chuyển (nếu có)
        public string PaymentMethod { get; set; }          // Hình thức thanh toán
        public decimal Discount { get; set; }              // Giảm giá hoặc khuyến mãi (nếu có)
        public int OrderId { get; set; }                // Mã đơn hàng
        public string OrderStatus { get; set; }            // Trạng thái đơn hàng
        

        public decimal OrderTotal { get; set; }            // Tổng đơn hàng
    }
}
