namespace Lab03.Models
{
    public class Discount
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public decimal DiscountAmount { get; set; } // Số tiền giảm giá
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = true; // Trạng thái mã giảm giá
    }

}
