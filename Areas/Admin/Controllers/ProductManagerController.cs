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
    public class ProductManagerController : Controller
    {
        private readonly ApplicationDbContext   _db;

        public ProductManagerController(ApplicationDbContext context)
        {
            _db = context;
        }

        // Action Index: Hiển thị các sản phẩm có trạng thái "Đang bán"
        public async Task<IActionResult> Index()
        {
            var products = await _db.Inventories
                .Include(i => i.SupplierProduct)
                    .ThenInclude(sp => sp.Category)
                .Include(i => i.SupplierProduct)
                    .ThenInclude(sp => sp.Supplier)
                .Include(i => i.SupplierProduct)
                    .ThenInclude(sp => sp.SupplierProductConfig)
                .Include(i => i.SupplierProduct)
                    .ThenInclude(sp => sp.ProductImages)
                .Where(i => i.Status == "Đang bán")
                .ToListAsync();

            // Kiểm tra xem SupplierProductConfig có dữ liệu không
            foreach (var product in products)
            {
                if (product.SupplierProduct.SupplierProductConfig == null)
                {
                    Console.WriteLine($"Sản phẩm {product.SupplierProduct.ProductName} không có cấu hình.");
                }
            }

            return View(products);
        }




        // Action UpdateInventory: Cập nhật thông tin sản phẩm
        [HttpPost]
        public async Task<IActionResult> UpdateInventory(int id, string productName, decimal sellingPrice, string location, string notes)
        {
            var inventory = await _db.Inventories
                                          .Include(i => i.SupplierProduct)
                                          .FirstOrDefaultAsync(i => i.Id == id && i.Status == "Đang bán"); // Chỉ cập nhật sản phẩm "Đang bán"

            if (inventory == null || inventory.SupplierProduct == null)
            {
                return NotFound(); // Không tìm thấy sản phẩm phù hợp
            }

            // Cập nhật thông tin sản phẩm
            inventory.SupplierProduct.ProductName = productName;
            inventory.SellingPrice = sellingPrice;
            inventory.Location = location;
            inventory.Notes = notes;

            _db.Inventories.Update(inventory);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cập nhật sản phẩm thành công!";
            return RedirectToAction("Index");
        }

        // Action UpdateStatus: Cập nhật trạng thái sản phẩm
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var inventory = await _db.Inventories
                                          .FirstOrDefaultAsync(i => i.Id == id);

            if (inventory == null)
            {
                return NotFound();
            }

            inventory.Status = status;
            _db.Inventories.Update(inventory);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Trạng thái sản phẩm đã được cập nhật!";
            return RedirectToAction("Index");
        }
    }
}
