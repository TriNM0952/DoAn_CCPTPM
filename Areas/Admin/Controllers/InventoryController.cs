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
    public class InventoryController : Controller
    {
        private readonly ApplicationDbContext _db;

        public InventoryController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: Admin/Inventory
        public async Task<IActionResult> Index()
        {
            var inventoryItems = await _db.Inventories
                .Include(i => i.SupplierProduct.Supplier)      // Bao gồm thông tin nhà cung cấp
                .Include(i => i.SupplierProduct.Category)      // Bao gồm thông tin loại sản phẩm
                .Include(i => i.SupplierProduct.SupplierProductConfig)        // Bao gồm cấu hình sản phẩm
                .ToListAsync();
            return View(inventoryItems);
        }





       
        public async Task<IActionResult> EditProduct(int id, string productName, decimal sellingPrice)
        {
            var inventoryItem = await _db.Inventories
                .Include(i => i.SupplierProduct)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (inventoryItem == null)
            {
                return NotFound();
            }

            // Cập nhật tên sản phẩm và giá bán
            inventoryItem.SupplierProduct.ProductName = productName;
            inventoryItem.SellingPrice = sellingPrice;

            _db.Inventories.Update(inventoryItem);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Thông tin sản phẩm đã được cập nhật.";
            return RedirectToAction("Index");
        }



        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var inventoryItem = await _db.Inventories.FindAsync(id);
            if (inventoryItem == null)
            {
                return NotFound();
            }

            // Nếu số lượng là 0, tự động chuyển trạng thái thành "Hết hàng"
            if (inventoryItem.Quantity == 0)
            {
                status = "Hết hàng";
            }

            inventoryItem.Status = status;
            _db.Inventories.Update(inventoryItem);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> UpdateInventory(int id, string productName, decimal sellingPrice, string location, string notes)
        {
            // Tìm Inventory và bao gồm SupplierProduct để tránh null
            var inventory = await _db.Inventories
                                          .Include(i => i.SupplierProduct) // Bao gồm SupplierProduct để tránh null
                                          .FirstOrDefaultAsync(i => i.Id == id);

            if (inventory == null || inventory.SupplierProduct == null)
            {
                return NotFound(); // Nếu không tìm thấy Inventory hoặc SupplierProduct, trả về NotFound
            }

            // Cập nhật thông tin sản phẩm
            inventory.SupplierProduct.ProductName = productName; // Cập nhật tên sản phẩm
            inventory.SellingPrice = sellingPrice; // Cập nhật giá bán
            inventory.Location = location; // Cập nhật vị trí
            inventory.Notes = notes; // Cập nhật ghi chú

            // Lưu thay đổi vào cơ sở dữ liệu
            _db.Inventories.Update(inventory);
            await _db.SaveChangesAsync();

            // Thông báo thành công
            TempData["SuccessMessage"] = "Cập nhật sản phẩm thành công!";
            return RedirectToAction("Index");
        }


        [HttpPost]
        public IActionResult Delete(int id)
        {
            var product = _db.SupplierProducts.Find(id);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Sản phẩm không tồn tại.";
                return RedirectToAction("Index");
            }

            _db.SupplierProducts.Remove(product);
            _db.SaveChanges();

            TempData["SuccessMessage"] = "Sản phẩm đã được xóa thành công.";
            return RedirectToAction("Index");
        }




    }
}
