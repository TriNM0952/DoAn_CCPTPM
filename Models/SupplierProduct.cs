using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Lab03.Models
{
    public class SupplierProduct
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)] // Giới hạn độ dài tên sản phẩm
        public string ProductName { get; set; }

        [Required] // Đảm bảo CategoryId không null
        public int CategoryId { get; set; } // Khóa ngoại đến bảng Category

        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal CostPrice { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng không thể nhỏ hơn ).")]
        public int Quantity { get; set; }

        [Required]
        public DateTime ImportDate { get; set; }

        [Required]
        [DefaultValue("Chưa được xác nhận")]
        public string Status { get; set; } = "Chưa được xác nhận";

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public string Notes { get; set; } = string.Empty;

        [Required]
        public int SupplierId { get; set; }

        [ForeignKey("SupplierId")]
        public Supplier? Supplier { get; set; }

        [MaxLength(50)]
        public string Brand { get; set; } = "No Brand";

        public SupplierProductConfig? SupplierProductConfig { get; set; }

        // Quan hệ một-nhiều với ProductImage
        public ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

        public SupplierProduct()
        {
            ImportDate = DateTime.Now;
            Status = "Chưa được xác nhận";
            Notes = string.Empty;
            Brand = "No Brand";
            ProductImages = new List<ProductImage>();
        }
    }
}
