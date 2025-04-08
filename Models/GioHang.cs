using Lab03.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class GioHang
{
    [Key]
    public int Id { get; set; }

    public int InventoryId { get; set; }
    [ForeignKey("InventoryId")]
    public Inventory Inventory { get; set; }

    public int Quantity { get; set; }

    public string ApplicationUserId { get; set; }
    [ForeignKey("ApplicationUserId")]
    [ValidateNever]
    public ApplicationUser ApplicationUser { get; set; }

    // Thêm TotalPrice có setter để có thể gán giá trị
    public decimal TotalPrice { get; set; }
}
