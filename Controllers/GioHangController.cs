using Lab03.Data;
using Lab03.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Lab03.Controllers
{
    
    public class GioHangController : Controller
    {
        private readonly ApplicationDbContext _db;



        public GioHangController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            var identity = (ClaimsIdentity)User.Identity;
            var claim = identity?.FindFirst(ClaimTypes.NameIdentifier);

            if (claim == null)
            {
                return Unauthorized();
            }

            var dsGioHang = _db.GioHang
             .Include(gh => gh.Inventory)                               // Bao gồm Inventory
             .ThenInclude(i => i.SupplierProduct)                       // Bao gồm SupplierProduct của Inventory
             .ThenInclude(sp => sp.ProductImages)                   // Bao gồm ProductImages của SupplierProduct
             .Where(gh => gh.ApplicationUserId == claim.Value)     // Lọc theo ApplicationUserId của người dùng
             .ToList();                                           // Thực thi truy vấn và lấy danh sách



            var giohangViewModel = new GioHangViewModel
            {
                DsGioHang = dsGioHang
            };

            // Tính tổng giá cho tất cả các mục trong giỏ hàng
            decimal totalPrice = dsGioHang.Sum(item => item.Quantity * (item.Inventory?.SellingPrice ?? 0));

            // Áp dụng giảm giá nếu có
            if (TempData["DiscountAmount"] != null)
            {
                // Chuyển đổi lại từ `string` sang `decimal`
                decimal discountAmount = decimal.Parse((string)TempData["DiscountAmount"]);
                totalPrice -= discountAmount;
                if (totalPrice < 0) totalPrice = 0; // Đảm bảo tổng tiền không âm
            }

            ViewBag.TotalItems = totalPrice;

            // Truyền TempData cho View để hiển thị thông báo
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.ErrorMessage = TempData["ErrorMessage"];

            return View(giohangViewModel);
        }



        public async Task<IActionResult> Tang(int giohangId)
        {
            var giohang = await _db.GioHang.Include(g => g.Inventory).FirstOrDefaultAsync(g => g.Id == giohangId);
            if (giohang != null)
            {
                giohang.Quantity++;
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã tăng số lượng sản phẩm!";
            }

            return RedirectToAction("Index");
        }

        // Giảm số lượng sản phẩm trong giỏ hàng
        public async Task<IActionResult> Giam(int giohangId)
        {
            var giohang = await _db.GioHang.Include(g => g.Inventory).FirstOrDefaultAsync(g => g.Id == giohangId);
            if (giohang != null && giohang.Quantity > 1)
            {
                giohang.Quantity--;
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã giảm số lượng sản phẩm!";
            }
            else if (giohang != null && giohang.Quantity == 1)
            {
                _db.GioHang.Remove(giohang); // Xóa sản phẩm nếu số lượng là 1 và giảm nữa
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa sản phẩm khỏi giỏ hàng!";
            }

            return RedirectToAction("GioHang/ChiTietHoaDonCustomer");
        }






      





        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XNTT(GioHangViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var dsGioHang = await _db.GioHang
                .Include(g => g.Inventory)
                .Where(g => g.ApplicationUserId == userId)
                .ToListAsync();

            if (!dsGioHang.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng của bạn trống.";
                return RedirectToAction("Index");
            }

            // Tính tổng giá trị giỏ hàng
            decimal totalAmount = dsGioHang.Sum(item => item.Quantity * (item.Inventory?.SellingPrice ?? 0));

            // Áp dụng giảm giá nếu có
            if (TempData["DiscountAmount"] != null)
            {
                decimal discountAmount = decimal.Parse((string)TempData["DiscountAmount"]);
                totalAmount -= discountAmount;
                if (totalAmount < 0) totalAmount = 0; // Đảm bảo tổng tiền không âm
            }

            // Thêm phí vận chuyển và thuế nếu có
            totalAmount += model.HoaDon.ShippingCost + (model.HoaDon.TaxAmount ?? 0);

            // Chuyển hướng đến Checkout với các thông tin thanh toán
            return RedirectToAction("Checkout", "Checkout", new
            {
                productName = "Đơn hàng của bạn", // Hoặc có thể là tên sản phẩm chính
                quantity = dsGioHang.Count, // Tổng số sản phẩm trong giỏ hàng
                price = totalAmount, // Tổng tiền giỏ hàng
            });
        }




        // Hàm tính tổng tiền
        private decimal CalculateTotal(HoaDon hoaDon)
        {
            // Tính tổng tiền đơn hàng mà không cần Discount
            return hoaDon.Total + hoaDon.ShippingCost + (hoaDon.TaxAmount ?? 0);
        }




        public async Task<IActionResult> HoaDonCustomer()
        {
            // Lấy thông tin người dùng hiện tại
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Kiểm tra xem người dùng đã đăng nhập chưa
            if (userId == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            // Lấy danh sách hóa đơn của người dùng hiện tại
            var hoaDons = await _db.HoaDon
                .Where(h => h.ApplicationUserId == userId)
                .Include(h => h.ChiTietHoaDons) // Bao gồm thông tin chi tiết hóa đơn
                .ToListAsync();

            return View(hoaDons);
        }


        public async Task<IActionResult> ChiTietHoaDonCustomer(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var hoaDon = await _db.HoaDon
                .Include(h => h.ChiTietHoaDons)
                    .ThenInclude(ct => ct.Inventory)
                    .ThenInclude(i => i.SupplierProduct) // Lấy thông tin sản phẩm
                .FirstOrDefaultAsync(h => h.Id == id && h.ApplicationUserId == userId);

            if (hoaDon == null)
            {
                return NotFound();
            }

            var discountAmountString = HttpContext.Session.GetString("DiscountAmount");
            decimal discountAmount = 0;

            ViewBag.DiscountAmount = discountAmount;

            return View(hoaDon);
        }



        public async Task<IActionResult> Xoa(int giohangId)
        {
            var giohang = await _db.GioHang.FirstOrDefaultAsync(g => g.Id == giohangId);
            if (giohang != null)
            {
                _db.GioHang.Remove(giohang);
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa sản phẩm khỏi giỏ hàng!";
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy sản phẩm cần xóa.";
            }

            return RedirectToAction("Index");
        }





        public IActionResult ThanhToan()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            // Lấy danh sách giỏ hàng của người dùng từ cơ sở dữ liệu
            var dsGioHang = _db.GioHang
                .Include(gh => gh.Inventory)
                .ThenInclude(i => i.SupplierProduct)
                .Where(gh => gh.ApplicationUserId == userId)
                .ToList();

            var giohangViewModel = new GioHangViewModel
            {
                DsGioHang = dsGioHang,
                HoaDon = new HoaDon
                {
                    ShippingCost = 30000  // Phí vận chuyển mặc định
                }
            };

            // Tính tổng tiền giỏ hàng
            decimal totalPrice = dsGioHang.Sum(item => item.Quantity * (item.Inventory?.SellingPrice ?? 0));

            // Kiểm tra xem có giảm giá từ session không
            var discountAmountString = HttpContext.Session.GetString("DiscountAmount");
            decimal discountAmount = 0;
            if (!string.IsNullOrEmpty(discountAmountString))
            {
                discountAmount = decimal.Parse(discountAmountString); // Lấy giá trị giảm giá từ session
            }

            // Cập nhật tổng tiền sau khi áp dụng giảm giá
            decimal discountedPrice = totalPrice - discountAmount;

            // Lưu giá trị cuối cùng vào ViewBag để hiển thị
            ViewBag.TotalPrice = totalPrice;
            ViewBag.DiscountedPrice = discountedPrice; // Tổng tiền sau khi áp dụng giảm giá
          

            return View(giohangViewModel);
        }


        [HttpPost]
        public async Task<IActionResult> ApplyDiscount(string discountCode)
        {
            var discount = await _db.Discounts.FirstOrDefaultAsync(d => d.Code == discountCode);
            if (discount != null)
            {
                var discountAmount = discount.DiscountAmount;
                var totalPrice = _db.GioHang.Sum(g => g.Quantity * g.Inventory.SellingPrice);
                var discountedPrice = totalPrice - discountAmount;

                // Lưu discountAmount vào session
                HttpContext.Session.SetString("DiscountAmount", discountAmount.ToString());

                ViewBag.DiscountMessage = $"Áp dụng mã giảm giá thành công. Giảm {discountAmount:#,##0} VNĐ";

                return Json(new { success = true, discountMessage = ViewBag.DiscountMessage, newTotalPrice = discountedPrice });
            }
            else
            {
                return Json(new { success = false, errorMessage = "Mã giảm giá không hợp lệ." });
            }
        }



        public async Task<IActionResult> CapNhatTrangThai(int id, string newStatus)
        {
            if (string.IsNullOrEmpty(newStatus))
            {
                TempData["ErrorMessage"] = "Trạng thái không được để trống.";
                return RedirectToAction("Index");
            }

            var hoaDon = await _db.HoaDon.FindAsync(id);
            if (hoaDon == null)
            {
                return NotFound();
            }

            // Cập nhật trạng thái đơn hàng
            hoaDon.OrderStatus = newStatus;
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Trạng thái đơn hàng đã được cập nhật thành công!";
            return RedirectToAction("Index");
        }


    }
}
