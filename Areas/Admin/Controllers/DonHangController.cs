using Lab03.Data;
using Lab03.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Lab03.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DonHangController : Controller
    {
        private readonly ApplicationDbContext _db;

        public DonHangController(ApplicationDbContext db)
        {
            _db = db;
        }

        // Hiển thị danh sách hóa đơn
        public async Task<IActionResult> Index()
        {
            var hoadon = await _db.HoaDon.Include(h => h.ApplicationUser).ToListAsync();
            return View(hoadon);
        }



        // Controller action: Hiển thị chi tiết hóa đơn
        public async Task<IActionResult> ChiTietDonHang(int id)
        {
            var hoadon = await _db.HoaDon
                .Include(h => h.ApplicationUser)
                .Include(h => h.ChiTietHoaDons) // Include danh sách sản phẩm
                .ThenInclude(ct => ct.Inventory) // Include thông tin Inventory
                .ThenInclude(i => i.SupplierProduct) // Include thông tin sản phẩm từ SupplierProduct
                .FirstOrDefaultAsync(h => h.Id == id);

            if (hoadon == null)
            {
                return NotFound();
            }

            return View(hoadon);
        }



        public async Task<IActionResult> CapNhatTrangThai(int id, string newStatus)
        {
            if (string.IsNullOrEmpty(newStatus))
            {
                TempData["ErrorMessage"] = "Trạng thái không được để trống.";
                return RedirectToAction("Index");
            }

            // Lấy hóa đơn từ database và bao gồm thông tin chi tiết hóa đơn, SupplierProduct, Supplier, Category
            var hoaDon = await _db.HoaDon
                .Include(h => h.ChiTietHoaDons)
                    .ThenInclude(ct => ct.Inventory) // Bao gồm thông tin Inventory
                        .ThenInclude(inv => inv.SupplierProduct) // Bao gồm SupplierProduct
                            .ThenInclude(sp => sp.Category) // Bao gồm Category
                            
                .FirstOrDefaultAsync(h => h.Id == id);

            if (hoaDon == null)
            {
                return NotFound();
            }

            // Cập nhật trạng thái của hóa đơn
            hoaDon.OrderStatus = newStatus;

            // Nếu trạng thái là "Đã nhận hàng", thêm dữ liệu vào bảng SalesRecord
            if (newStatus == "Đã nhận hàng")
            {
                // Thêm dữ liệu vào bảng SalesRecord cho từng sản phẩm trong chi tiết hóa đơn
                foreach (var chiTiet in hoaDon.ChiTietHoaDons)
                {
                    // Kiểm tra nếu có sản phẩm liên quan
                    var product = chiTiet.Inventory?.SupplierProduct; // Truy cập SupplierProduct
                    if (product != null)
                    {
                        // Lấy thông tin kho của sản phẩm
                        var inventory = await _db.Inventories
                            .Where(i => i.SupplierProductId == product.Id)
                            .FirstOrDefaultAsync();

                        // Nếu tồn kho và sản phẩm hợp lệ
                        if (inventory != null)
                        {
                            // Tạo mới đối tượng SalesRecord
                            var salesRecord = new SalesRecord
                            {
                                CustomerName = hoaDon.CustomerName,
                                ProductName = product.ProductName,
                                ProductCategory = product.Category?.Name, // Truy cập tên Category (kiểm tra null)
                                SaleDate = DateTime.Now, // Ngày bán là hiện tại
                                PurchasePrice = product.CostPrice, // Giá nhập từ SupplierProduct
                                SalePrice = inventory.SellingPrice, // Giá bán từ ChiTietHoaDon
                                TaxAmount = hoaDon.TaxAmount ?? 0, // Tiền thuế
                                ShippingAddress = hoaDon.ShippingAddress,
                                ActualProfit = inventory.SellingPrice - product.CostPrice - hoaDon.ShippingCost, // Lợi nhuận thực tế
                                ShippingCost = hoaDon.ShippingCost,
                                PaymentMethod = hoaDon.PaymentMethod,
                                Discount = hoaDon.DiscountAmount ?? 0, // Giảm giá (nếu có)
                                OrderId = hoaDon.Id,
                                OrderStatus = hoaDon.OrderStatus,
                              
                                OrderTotal = hoaDon.Total // Tổng tiền đơn hàng
                            };

                            // Thêm vào bảng SalesRecord
                            _db.SalesRecords.Add(salesRecord);
                        }
                    }
                }
            }

            // Lưu thay đổi vào cơ sở dữ liệu
            await _db.SaveChangesAsync();

            // Thông báo thành công
            TempData["SuccessMessage"] = "Trạng thái đơn hàng đã được cập nhật thành công!";
            return RedirectToAction("Index");
        }




    }
}
