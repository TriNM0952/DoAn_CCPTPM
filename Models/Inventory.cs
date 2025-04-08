using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lab03.Models
{
    public class Inventory
    {
        [Key]
        public int Id { get; set; } // Khóa chính

        // Tham chiếu đến SupplierProduct
        public int SupplierProductId { get; set; }

        [ForeignKey("SupplierProductId")]
        public SupplierProduct SupplierProduct { get; set; } // Liên kết với SupplierProduct

        // Các trường thông tin tồn kho
        public int Quantity { get; set; } // Số lượng trong kho

        [Column(TypeName = "decimal(18,2)")]
        public decimal SellingPrice { get; set; } // Giá bán

        public DateTime ImportDate { get; set; } // Ngày nhập kho
        
        [Required]
        public string Location { get; set; } // Vị trí kho
        public string? Notes { get; set; } // Ghi chú thêm về sản phẩm trong kho

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Chờ xác nhận"; // Trạng thái của sản phẩm
    }
}
