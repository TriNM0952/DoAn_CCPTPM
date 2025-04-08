using Lab03.Data;
using Lab03.Models;
using Lab03.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

public class FeaturedProductsViewComponent : ViewComponent
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly ICategoryRepository _categoryRepository;

    private readonly ApplicationDbContext _db; // Đảm bảo rằng bạn đã inject ApplicationDbContext vào contructor

    public FeaturedProductsViewComponent(IInventoryRepository inventoryRepository, ICategoryRepository categoryRepository, ApplicationDbContext db)
    {
        _inventoryRepository = inventoryRepository;
        _categoryRepository = categoryRepository;

        _db = db;
    }

    public IViewComponentResult Invoke(int? Category = null)
    {
        // Lấy danh sách sản phẩm trong kho với trạng thái "Đang bán"
        var products = _db.Inventories
            .Include(i => i.SupplierProduct)
            .ThenInclude(sp => sp.Category)
            .Include(i => i.SupplierProduct)
                .ThenInclude(sp => sp.Supplier)
            .Include(i => i.SupplierProduct)
                .ThenInclude(sp => sp.SupplierProductConfig)
            .Include(i => i.SupplierProduct)
                .ThenInclude(sp => sp.ProductImages)
            .Where(i => i.Status == "Đang bán") // Chỉ lấy sản phẩm "Đang bán"
            .AsQueryable();

        // Lọc theo Category nếu có
        if (Category.HasValue)
        {
            products = products.Where(i => i.SupplierProduct.CategoryId == Category.Value);
        }

        // Trả về view cho component
        return View(products.ToList()); // Trả về danh sách sản phẩm
    }





}
