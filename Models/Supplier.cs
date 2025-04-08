using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lab03.Models
{
    public class Supplier
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)] // Giới hạn độ dài
        public string SupplierName { get; set; }

        [MaxLength(50)]
        public string ContactName { get; set; }

        [MaxLength(15)] // Định dạng số điện thoại
        public string PhoneNumber { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [MaxLength(200)]
        public string Address { get; set; }

        public DateTime ContractStartDate { get; set; }

        public DateTime? ContractEndDate { get; set; }

        [MaxLength(20)]
        public string BankAccountNumber { get; set; }

        [MaxLength(50)]
        public string BankName { get; set; }

        [MaxLength(15)]
        public string TaxCode { get; set; }

        public string Notes { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CostPrice { get; set; } // Giá nhà cung cấp (nếu cần)

        // Một nhà cung cấp có thể cung cấp nhiều sản phẩm
        public ICollection<SupplierProduct> SupplierProducts { get; set; } = new List<SupplierProduct>();
    }
}
