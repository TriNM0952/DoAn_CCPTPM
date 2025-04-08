using Lab03.Data;
using Lab03.Models;
using Lab03.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Lab03.Controllers
{
    public class inventoryController : Controller
    {
        private readonly IInventoryRepository _inventoryRepository;
        private readonly ICategoryRepository _categoryRepository;
        
        private readonly ApplicationDbContext _db; // Đảm bảo rằng bạn đã inject ApplicationDbContext vào contructor
        
        public inventoryController(IInventoryRepository inventoryRepository, ICategoryRepository categoryRepository, ApplicationDbContext db)
        {
            _inventoryRepository = inventoryRepository;
            _categoryRepository = categoryRepository;
            
            _db = db;
        }

        // Hiển thị danh sách sản phẩm
        public async Task<IActionResult> Index(string SearchString = "", int? Category = null)
        {
            // Lấy danh sách sản phẩm trong kho với trạng thái "Đang bán"
            var inventorys = _db.Inventories
                .Include(i => i.SupplierProduct)
                    .ThenInclude(sp => sp.Category)
                .Include(i => i.SupplierProduct)
                    .ThenInclude(sp => sp.Supplier)
                .Include(i => i.SupplierProduct)
                    .ThenInclude(sp => sp.SupplierProductConfig)
                .Include(i => i.SupplierProduct)
                    .ThenInclude(sp => sp.ProductImages)
                .Where(i => i.Status == "Đang bán") // Chỉ lấy sản phẩm đang bán
                .AsQueryable();

            // Tìm kiếm theo từ khóa
            if (!string.IsNullOrEmpty(SearchString))
            {
                inventorys = inventorys.Where(x => x.SupplierProduct.ProductName.ToUpper().Contains(SearchString.ToUpper()));
            }

            // Lọc theo danh mục
            if (Category.HasValue)
            {
                inventorys = inventorys.Where(p => p.SupplierProduct.CategoryId == Category.Value);
            }

            // Lấy toàn bộ danh sách danh mục để hiển thị trong dropdown
            var categories = await _db.Categories.ToListAsync(); // Sử dụng trực tiếp từ DbContext
            ViewBag.Categories = new SelectList(categories, "Id", "Name");

            // Trả về view với danh sách sản phẩm
            return View(await inventorys.ToListAsync());
        }


        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int id, int newQuantity)
        {
            var inventoryItem = await _db.Inventories.FindAsync(id);
            if (inventoryItem == null)
            {
                return NotFound();
            }

            inventoryItem.Quantity = newQuantity;

            // Cập nhật trạng thái thành "Hết hàng" nếu số lượng bằng 0
            if (inventoryItem.Quantity == 0)
            {
                inventoryItem.Status = "Hết hàng";
            }

            _db.Inventories.Update(inventoryItem);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Số lượng và trạng thái sản phẩm đã được cập nhật.";
            return RedirectToAction("Index");
        }


        /*
                // Xử lý thêm sản phẩm mới
                [HttpPost]
                public async Task<IActionResult> Add(inventory inventory, IFormFile imageUrl)
                {
                    if (ModelState.IsValid)
                    {
                        if (imageUrl != null)
                        {
                            // Lưu hình ảnh đại diện tham khảo bài 02 hàm SaveImage
                            inventory.Inventory.Images = await SaveImage(imageUrl);
                        }

                        await _inventoryRepository.AddAsync(inventory);
                        return RedirectToAction(nameof(Index));
                    }
                    // Nếu ModelState không hợp lệ, hiển thị form với dữ liệu đã nhập
                    var categories = await _categoryRepository.GetAllAsync();
                    ViewBag.Categories = new SelectList(categories, "Id", "Name");
                    return View(inventory);
                }

                // Viết thêm hàm SaveImage (tham khảo bào 02)
                private async Task<string> SaveImage(IFormFile image)
                {
                    var savePath = Path.Combine("wwwroot/images", image.FileName);
                    using (var fileStream = new FileStream(savePath, FileMode.Create))
                    {
                        await image.CopyToAsync(fileStream);
                    }
                    return Path.Combine("/images", image.FileName);
                }
        */

        public async Task<IActionResult> Display(int inventoryId)
        {
            var inventory = await _db.Inventories
                .Include(i => i.SupplierProduct)
                    .ThenInclude(sp => sp.Category)
                .Include(i => i.SupplierProduct)
                    .ThenInclude(sp => sp.Supplier)
                .Include(i => i.SupplierProduct)
                    .ThenInclude(sp => sp.SupplierProductConfig)
                .Include(i => i.SupplierProduct)
                    .ThenInclude(sp => sp.ProductImages)
                .FirstOrDefaultAsync(i => i.Id == inventoryId);

            if (inventory == null)
            {
                return NotFound("Sản phẩm không tồn tại.");
            }

            ViewBag.CategoryName = inventory.SupplierProduct.Category?.Name;
            ViewBag.CategoryId = inventory.SupplierProduct.CategoryId;
            return View(inventory);
        }




        [HttpPost]
        [Authorize]  // Đảm bảo người dùng đã đăng nhập
        public async Task<IActionResult> ThemVaoGio(int inventoryId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Kiểm tra lại nếu không có userId (không đăng nhập)
            if (string.IsNullOrEmpty(userId))
            {
                // Nếu chưa đăng nhập, chuyển hướng đến trang đăng nhập
                return RedirectToAction("Login", "Account");
            }

            var inventory = await _db.Inventories
                .Include(i => i.SupplierProduct)
                .FirstOrDefaultAsync(i => i.Id == inventoryId);

            if (inventory == null || inventory.Status != "Đang bán" || inventory.Quantity <= 0)
            {
                TempData["ErrorMessage"] = "Sản phẩm không thể thêm vào giỏ hàng do trạng thái không phù hợp hoặc đã hết hàng.";
                return RedirectToAction("Index", "Inventory");
            }

            var existingItem = await _db.GioHang
                .FirstOrDefaultAsync(i => i.InventoryId == inventoryId && i.ApplicationUserId == userId);

            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                var newItem = new GioHang
                {
                    InventoryId = inventory.Id,
                    Quantity = 1,
                    ApplicationUserId = userId,
                    TotalPrice = inventory.SellingPrice
                };
                _db.GioHang.Add(newItem);
            }

            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Sản phẩm đã được thêm vào giỏ hàng!";
            return RedirectToAction("Index", "Inventory");
        }

        // Phương thức "Mua ngay" (chuyển hướng đến trang giỏ hàng sau khi thêm)
        [HttpPost]
        public async Task<IActionResult> MuaNgay(int inventoryId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var inventory = await _db.Inventories
                .Include(i => i.SupplierProduct)
                .FirstOrDefaultAsync(i => i.Id == inventoryId);

            if (inventory == null || inventory.Status != "Đang bán" || inventory.Quantity <= 0)
            {
                TempData["ErrorMessage"] = "Sản phẩm không thể mua do trạng thái không phù hợp hoặc đã hết hàng.";
                return RedirectToAction("Index", "Inventory");
            }

            var existingItem = await _db.GioHang
                .FirstOrDefaultAsync(i => i.InventoryId == inventoryId && i.ApplicationUserId == userId);

            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                var newItem = new GioHang
                {
                    InventoryId = inventory.Id,
                    Quantity = 1,
                    ApplicationUserId = userId,
                    TotalPrice = inventory.SellingPrice
                };
                _db.GioHang.Add(newItem);
            }

            await _db.SaveChangesAsync();
            return RedirectToAction("Index", "GioHang"); // Chuyển hướng đến trang giỏ hàng
        }

        /*
        // Hiển thị form cập nhật sản phẩm
        public async Task<IActionResult> Update(int id)
        {
            var inventory = await _inventoryRepository.GetByIdAsync(id);
            if (inventory == null)
            {
                return NotFound();
            }

            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", inventory.Inventory.CategoryId);
            return View(inventory);
        }

        // Xử lý cập nhật sản phẩm
        [HttpPost]
        public async Task<IActionResult> Update(int id, inventory inventory, IFormFile imageUrl)
        {
            ModelState.Remove("ImageUrl"); // Loại bỏ xác thực ModelState cho ImageUrl
            if (id != inventory.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existinginventory = await _inventoryRepository.GetByIdAsync(id);

                // Giữ nguyên thông tin hình ảnh nếu không có hình mới được tải lên
                if (imageUrl == null)
                {
                    inventory.Inventory.Images = existinginventory.Inventory.Images;
                }
                else
                {
                    // Lưu hình ảnh mới
                    inventory.Inventory.Images = await SaveImage(imageUrl);
                }
                // Cập nhật các thông tin khác của sản phẩm
                existinginventory.Inventory.Name = inventory.Inventory.Name;
                existinginventory.Inventory.SellingPrice = inventory.Inventory.SellingPrice;
                existinginventory.Inventory.Description = inventory.Inventory.Description;
                existinginventory.Inventory.CategoryId = inventory.Inventory.CategoryId;
                existinginventory.Inventory.Images = inventory.Inventory.Images;

                await _inventoryRepository.UpdateAsync(existinginventory);

                return RedirectToAction(nameof(Index));
            }
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View(inventory);
        }

        // Hiển thị form xác nhận xóa sản phẩm
        public async Task<IActionResult> Delete(int id)
        {
            var inventory = await _inventoryRepository.GetByIdAsync(id);
            if (inventory == null)
            {
                return NotFound();
            }
            return View(inventory);
        }

        // Xử lý xóa sản phẩm
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _inventoryRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }*/
    }
}
