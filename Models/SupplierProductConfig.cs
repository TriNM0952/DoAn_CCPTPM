using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lab03.Models
{
    public class SupplierProductConfig
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SupplierProductId { get; set; }
        [ForeignKey("SupplierProductId")]
        public SupplierProduct SupplierProduct { get; set; }  // Sản phẩm từ nhà cung cấp

    



        // Các thuộc tính cấu hình
        public string? ManHinh { get; set; }
        public string? HeDieuHanh { get; set; }
        public string? CameraSau { get; set; }
        public string? CameraTruoc { get; set; }
        public string? CPU { get; set; }
        public string? Ram { get; set; }
        public string? BoNhoTrong { get; set; }
        public string? Sim { get; set; }
        public string? DungLuong { get; set; }
        public string? Color { get; set; }
        public string? CardDoHoa { get; set; }
        public string? CongKetNoi { get; set; }
        public string? TrongLuong { get; set; }
        public string? KetNoi { get; set; }
        public string? ChatLuongAmThanh { get; set; }
        public string? ThoiLuongPin { get; set; }
        public string? CamBien { get; set; }
        public string? KhaNangChongNuoc { get; set; }
        public string? KichThuoc { get; set; }

        public string? Notes { get; set; }  // Ghi chú
    }
}
