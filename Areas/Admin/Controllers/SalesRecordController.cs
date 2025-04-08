using Lab03.Data;
using Lab03.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Lab03.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class SalesRecordController : Controller
    {
        private readonly ApplicationDbContext _db;

        public SalesRecordController(ApplicationDbContext db)
        {
            _db = db;
        }

        // Hiển thị bản ghi doanh số bán hàng
        public async Task<IActionResult> Index()
        {
            // Lấy tất cả các bản ghi doanh thu
            var salesRecords = _db.SalesRecords.ToList();

            // Tính doanh thu (Tổng giá bán - Tổng giá nhập - Tổng chi phí vận chuyển - Tổng thuế)
            var totalRevenue = salesRecords.Sum(sr => sr.SalePrice - sr.PurchasePrice - sr.ShippingCost - sr.TaxAmount);

            // Trả về View với dữ liệu doanh thu
            ViewBag.TotalRevenue = totalRevenue;
            return View(salesRecords);
        }



    }
}
