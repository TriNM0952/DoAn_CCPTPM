using Lab03.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class ProductImage
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Url { get; set; } // Đường dẫn ảnh

    [Required]
    public int SupplierProductId { get; set; } // Khóa ngoại đến SupplierProduct

    [ForeignKey("SupplierProductId")]
    public SupplierProduct? SupplierProduct { get; set; } // Quan hệ ngược với SupplierProduct
}
