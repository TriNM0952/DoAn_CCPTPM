using Microsoft.AspNetCore.Mvc;
using Net.payOS.Types;
using Net.payOS;
using Lab03.Models;
using Microsoft.EntityFrameworkCore;
using Lab03.Data;
using System.Security.Claims;

namespace NetCoreDemo.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly PayOS _payOS;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _dbContext;

        public CheckoutController(PayOS payOS, IHttpContextAccessor httpContextAccessor, ApplicationDbContext dbContext)
        {
            _payOS = payOS;
            _httpContextAccessor = httpContextAccessor;
            _dbContext = dbContext;
        }

        [HttpGet("/Checkout")]
        public IActionResult Index()
        {
            return View("index");
        }

        [HttpGet("/Checkout/Cancel")]
        public IActionResult Cancel()
        {
            return View("cancel");
        }

        [HttpGet("/Checkout/Success")]
        public async Task<IActionResult> Success()
        {
            try
            {
                // Gọi trực tiếp phương thức XNTT() để xử lý đơn hàng
                return await XNTT(); // Gọi lại action XNTT
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi xử lý đơn hàng.";
                return RedirectToAction("Index", "Home"); // Điều hướng về trang chủ nếu có lỗi
            }
        }





        public async Task<IActionResult> XNTT()
        {
            try
            {


               //số tiền được giảm giá
                var discountAmount = decimal.Parse(HttpContext.Session.GetString("DiscountAmount"));



                // Lấy ApplicationUserId từ session
                var applicationUserId = HttpContext.Session.GetString("ApplicationUserId");
                // Lấy thông tin từ session
                var customerName = HttpContext.Session.GetString("CustomerName");
                var phoneNumber = HttpContext.Session.GetString("PhoneNumber");
                var address = HttpContext.Session.GetString("Address");
                var shippingAddress = HttpContext.Session.GetString("ShippingAddress");
                var paymentMethod = HttpContext.Session.GetString("PaymentMethod");
                var note = HttpContext.Session.GetString("Note");
                var totalAmount = decimal.Parse(HttpContext.Session.GetString("TotalAmount"));
                var shippingCost = decimal.Parse(HttpContext.Session.GetString("ShippingCost"));
                var taxAmount = decimal.Parse(HttpContext.Session.GetString("TaxAmount"));
                var orderDate = DateTime.Parse(HttpContext.Session.GetString("OrderDate"));
                var orderStatus = HttpContext.Session.GetString("OrderStatus");


                if (string.IsNullOrEmpty(customerName) || string.IsNullOrEmpty(phoneNumber))
                {
                    TempData["ErrorMessage"] = "Thông tin khách hàng không hợp lệ.";
                    return RedirectToAction("Index", "Home");
                }

                // Lưu thông tin vào HoaDon
                HoaDon hoaDon = new HoaDon
                {
                    ApplicationUserId = applicationUserId,

                    CustomerName = customerName,
                    PhoneNumber = phoneNumber,
                    Address = address,
                    ShippingAddress = shippingAddress,
                    PaymentMethod = paymentMethod,
                    Total = totalAmount,
                    ShippingCost = shippingCost,
                    TaxAmount = taxAmount,
                    Note = note,
                    OrderDate = orderDate,
                    OrderStatus = orderStatus,
                    DiscountAmount = discountAmount
                };

                _dbContext.HoaDon.Add(hoaDon);
                await _dbContext.SaveChangesAsync();

                // Lưu chi tiết hóa đơn (từ giỏ hàng)
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userCart = _dbContext.GioHang.Where(g => g.ApplicationUserId == userId).ToList();

                foreach (var item in userCart)
                {
                    var chiTietHoaDon = new ChiTietHoaDon
                    {
                        HoaDonId = hoaDon.Id,
                        InventoryId = item.InventoryId,
                        Quantity = item.Quantity
                    };

                    _dbContext.ChiTietHoaDons.Add(chiTietHoaDon);
                }

                await _dbContext.SaveChangesAsync();

                // Xóa giỏ hàng sau khi thanh toán thành công
                _dbContext.GioHang.RemoveRange(userCart);
                await _dbContext.SaveChangesAsync();

                // Chuyển hướng đến trang xác nhận đơn hàng thành công
                return View("Success");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi xử lý đơn hàng.";
                return RedirectToAction("Index", "Home");
            }
        }








        // Quy trình thanh toán, tạo payment link
        [HttpPost("/Checkout/Process")]
        public async Task<IActionResult> Checkout(GioHangViewModel model)
        {
            try
            {
                var applicationUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userCart = _dbContext.GioHang
                    .Where(g => g.ApplicationUserId == userId)
                    .Include(g => g.Inventory)
                    .ToList();

                if (userCart == null || !userCart.Any())
                {
                    TempData["ErrorMessage"] = "Giỏ hàng của bạn trống.";
                    return RedirectToAction("Index");
                }

                // Lấy giá trị DiscountAmount từ session (là giá trị giảm giá)
                var discountAmountString = HttpContext.Session.GetString("DiscountAmount");
                decimal discountAmount = 0;

                if (!string.IsNullOrEmpty(discountAmountString))
                {
                    discountAmount = Convert.ToDecimal(discountAmountString); // Chuyển sang decimal
                }

                // Tính toán tổng tiền giỏ hàng (bao gồm shipping và thuế)
                decimal totalAmount = userCart.Sum(item => item.Quantity * (item.Inventory?.SellingPrice ?? 0))
                                      + model.HoaDon.ShippingCost + (model.HoaDon.TaxAmount ?? 0);

                // Trừ đi giá trị giảm giá từ session (discountAmount)
                totalAmount -= discountAmount;

                // Lưu các thông tin vào session
                HttpContext.Session.SetString("ApplicationUserId", applicationUserId);
                HttpContext.Session.SetString("CustomerName", model.HoaDon.CustomerName);
                HttpContext.Session.SetString("PhoneNumber", model.HoaDon.PhoneNumber);
                HttpContext.Session.SetString("Address", model.HoaDon.Address);
                HttpContext.Session.SetString("ShippingAddress", model.HoaDon.ShippingAddress);
                HttpContext.Session.SetString("OrderDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                HttpContext.Session.SetString("OrderStatus", "Đang chờ xác nhận");
                HttpContext.Session.SetString("PaymentMethod", model.HoaDon.PaymentMethod);
                HttpContext.Session.SetString("Note", model.HoaDon.Note ?? string.Empty);
                HttpContext.Session.SetString("ShippingCost", model.HoaDon.ShippingCost.ToString());
                HttpContext.Session.SetString("TaxAmount", (model.HoaDon.TaxAmount ?? 0).ToString());
                HttpContext.Session.SetString("TotalAmount", totalAmount.ToString());
                HttpContext.Session.SetString("DiscountAmount", discountAmount.ToString());



                // Lấy mã đơn hàng
                int orderCode = int.Parse(DateTimeOffset.Now.ToString("ffffff"));
                ItemData item = new ItemData("Đơn hàng của bạn", 1, (int)totalAmount);
                List<ItemData> items = new List<ItemData> { item };

                // Get the current request's base URL
                var request = _httpContextAccessor.HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";

                // Tạo đối tượng PaymentData
                PaymentData paymentData = new PaymentData(
                    orderCode,
                    (int)totalAmount, // Convert totalAmount to int since API expects int
                    "Thanh toán đơn hàng",
                    items,
                    $"{baseUrl}/Checkout/Cancel",
                    $"{baseUrl}/Checkout/Success"
                );

                // Gọi API để tạo liên kết thanh toán
                CreatePaymentResult createPayment = await _payOS.createPaymentLink(paymentData);

                // Chuyển hướng người dùng tới trang thanh toán
                return Redirect(createPayment.checkoutUrl);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return RedirectToAction("Index", "Home"); // Chuyển hướng về Home nếu có lỗi
            }
        }



        [HttpPost("/Checkout/ProcessPayment")]
        public async Task<IActionResult> ProcessPayment(GioHangViewModel model)
        {
            try
            {
                var applicationUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userCart = _dbContext.GioHang
                    .Where(g => g.ApplicationUserId == userId)
                    .Include(g => g.Inventory)
                    .ToList();

                if (userCart == null || !userCart.Any())
                {
                    TempData["ErrorMessage"] = "Giỏ hàng của bạn trống.";
                    return RedirectToAction("Index");
                }

                // Lấy giá trị DiscountAmount từ session (là giá trị giảm giá)
                var discountAmountString = HttpContext.Session.GetString("DiscountAmount");
                decimal discountAmount = 0;

                if (!string.IsNullOrEmpty(discountAmountString))
                {
                    discountAmount = Convert.ToDecimal(discountAmountString); // Chuyển sang decimal
                }

                // Tính toán tổng tiền giỏ hàng (bao gồm shipping và thuế)
                decimal totalAmount = userCart.Sum(item => item.Quantity * (item.Inventory?.SellingPrice ?? 0))
                                       + model.HoaDon.ShippingCost + (model.HoaDon.TaxAmount ?? 0);

                // Trừ đi giá trị giảm giá từ session (discountAmount)
                totalAmount -= discountAmount;

                // Lưu các thông tin vào session
                HttpContext.Session.SetString("ApplicationUserId", applicationUserId);
                HttpContext.Session.SetString("CustomerName", model.HoaDon.CustomerName);
                HttpContext.Session.SetString("PhoneNumber", model.HoaDon.PhoneNumber);
                HttpContext.Session.SetString("Address", model.HoaDon.Address);
                HttpContext.Session.SetString("ShippingAddress", model.HoaDon.ShippingAddress);
                HttpContext.Session.SetString("OrderDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                HttpContext.Session.SetString("OrderStatus", "Đang chờ xác nhận");
                HttpContext.Session.SetString("PaymentMethod", model.HoaDon.PaymentMethod);
                HttpContext.Session.SetString("Note", model.HoaDon.Note ?? string.Empty);
                HttpContext.Session.SetString("ShippingCost", model.HoaDon.ShippingCost.ToString());
                HttpContext.Session.SetString("TaxAmount", (model.HoaDon.TaxAmount ?? 0).ToString());
                HttpContext.Session.SetString("TotalAmount", totalAmount.ToString());
                HttpContext.Session.SetString("DiscountAmount", discountAmount.ToString());

                // Nếu là thanh toán khi nhận hàng, bỏ qua API tạo liên kết thanh toán
                if (model.HoaDon.PaymentMethod == "CashOnDelivery")
                {
                    return await XNTT(); // Chuyển hướng về action XNTT sau khi lưu session
                }

                // Nếu là chuyển khoản ngân hàng, gọi API tạo liên kết thanh toán
                int orderCode = int.Parse(DateTimeOffset.Now.ToString("ffffff"));
                ItemData item = new ItemData("Đơn hàng của bạn", 1, (int)totalAmount);
                List<ItemData> items = new List<ItemData> { item };

                var request = _httpContextAccessor.HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";

                // Tạo đối tượng PaymentData
                PaymentData paymentData = new PaymentData(
                    orderCode,
                    (int)totalAmount, // Convert totalAmount to int since API expects int
                    "Thanh toán đơn hàng",
                    items,
                    $"{baseUrl}/Checkout/Cancel",
                    $"{baseUrl}/Checkout/Success"
                );

                // Gọi API để tạo liên kết thanh toán
                CreatePaymentResult createPayment = await _payOS.createPaymentLink(paymentData);

                // Chuyển hướng người dùng tới trang thanh toán
                return Redirect(createPayment.checkoutUrl);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi xử lý thanh toán.";
                return RedirectToAction("Index", "Home");
            }
        }



    }
}
