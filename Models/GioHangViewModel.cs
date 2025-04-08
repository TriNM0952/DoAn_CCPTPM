namespace Lab03.Models
{
    public class GioHangViewModel
    {
        public IEnumerable<GioHang> DsGioHang { get; set; }
        public HoaDon HoaDon { get; set; }

        // TotalPrice là một property có thể gán
        public decimal TotalPrice { get; set; }
    }
}
