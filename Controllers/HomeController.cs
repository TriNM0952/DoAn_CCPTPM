using Lab03.Data;
using Lab03.Models;
using Lab03.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Lab03.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ApplicationDbContext _db;

        // Constructor để tiêm phụ thuộc vào các dịch vụ cần thiết
        public HomeController(
            ILogger<HomeController> logger,
            IInventoryRepository inventoryRepository,
            ICategoryRepository categoryRepository,
            ApplicationDbContext db)
        {
            _logger = logger;
            _inventoryRepository = inventoryRepository;
            _categoryRepository = categoryRepository;
            _db = db;
        }

        // Phương thức Index để liệt kê các sản phẩm với bộ lọc tìm kiếm và danh mục (nếu có)
        public async Task<IActionResult> Index(string SearchString = "", int? Category = null)
        {
            // Bắt đầu câu truy vấn cho các sản phẩm có trạng thái "Đang bán"
            var inventorysQuery = _db.Inventories
                .Include(i => i.SupplierProduct)
                .ThenInclude(sp => sp.Category)
                 .Include(i => i.SupplierProduct)
                    .ThenInclude(sp => sp.Supplier)
                .Include(i => i.SupplierProduct)
                    .ThenInclude(sp => sp.SupplierProductConfig)
                .Include(i => i.SupplierProduct)
                    .ThenInclude(sp => sp.ProductImages)
                .Where(i => i.Status == "Đang bán") // Lọc chỉ sản phẩm đang bán
                .AsQueryable();

            // Áp dụng bộ lọc tìm kiếm nếu có
            if (!string.IsNullOrEmpty(SearchString))
            {
                inventorysQuery = inventorysQuery.Where(x => x.SupplierProduct.ProductName.ToUpper().Contains(SearchString.ToUpper()));
            }

            // Áp dụng bộ lọc theo danh mục nếu có
            if (Category.HasValue)
            {
                inventorysQuery = inventorysQuery.Where(p => p.SupplierProduct.CategoryId == Category.Value);
            }

            // Lấy danh sách tất cả các danh mục để hiển thị trong dropdown
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");

            // Thực thi câu truy vấn và truyền kết quả vào View
            var inventorys = await inventorysQuery.ToListAsync();

            return View(inventorys);
        }



        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult Shop()
        {
            return View();
        }
    }
}
