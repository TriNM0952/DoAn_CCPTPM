using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lab03.Models
{
    public class ChiTietHoaDon
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int HoaDonId { get; set; }
        [ForeignKey("HoaDonId")]
        [ValidateNever]
        public HoaDon HoaDon { get; set; }

        public int InventoryId { get; set; } // ID của sản phẩm trong kho
        [ForeignKey("InventoryId")]
        public Inventory Inventory { get; set; } // Liên kết với bảng Inventory

        public int Quantity { get; set; } // Số lượng sản phẩm trong hóa đơn

        
    }
}
