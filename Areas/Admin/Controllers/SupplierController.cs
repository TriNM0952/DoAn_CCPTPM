using Lab03.Data;
using Lab03.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;

namespace Lab03.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class SupplierController : Controller
    {
        private readonly ApplicationDbContext _db;
        public SupplierProduct SupplierProduct { get; set; } 
        public SupplierProductConfig SupplierProductConfig { get; set; }
        public SupplierController(ApplicationDbContext db)
        {
            _db = db;
        }

        // Action để hiển thị danh sách nhà cung cấp
        public async Task<IActionResult> Index()
        {
            var suppliers = await _db.Suppliers
                .Include(s => s.SupplierProducts) // Bao gồm các sản phẩm do nhà cung cấp cung cấp
                .ToListAsync();

            return View(suppliers);
        }


        public IActionResult Details(int id)
        {
            // Lấy thông tin sản phẩm bao gồm các thông tin liên quan như Category, SupplierProductConfig, ProductImages, và Supplier
            var supplierProduct = _db.SupplierProducts
                .Include(sp => sp.Category) // Bao gồm thông tin thể loại sản phẩm
                .Include(sp => sp.SupplierProductConfig) // Bao gồm cấu hình sản phẩm
                .Include(sp => sp.ProductImages) // Bao gồm hình ảnh sản phẩm
                .Include(sp => sp.Supplier) // Bao gồm thông tin nhà cung cấp
                .FirstOrDefault(sp => sp.Id == id); // Lọc theo Id của sản phẩm

            // Nếu không tìm thấy sản phẩm, trả về NotFound
            if (supplierProduct == null)
            {
                return NotFound();
            }

            // Trả về view với đầy đủ thông tin của sản phẩm
            return View(supplierProduct);
        }









        public async Task<IActionResult> Display(int id)
        {
            var supplier = await _db.Suppliers
                .Include(s => s.SupplierProducts)
                    .ThenInclude(sp => sp.Category) // Bao gồm loại sản phẩm
                .Include(s => s.SupplierProducts)
                    .ThenInclude(sp => sp.SupplierProductConfig) // Bao gồm cấu hình sản phẩm
                .FirstOrDefaultAsync(s => s.Id == id);

            if (supplier == null)
            {
                return NotFound();
            }

            // Tạo một Dictionary để lưu trữ thông tin tồn kho của mỗi sản phẩm
            var inventoryInfo = new Dictionary<int, (int Quantity, string Location, decimal SellingPrice, string Notes)>();

            foreach (var product in supplier.SupplierProducts)
            {
                var inventory = await _db.Inventories.FirstOrDefaultAsync(i => i.SupplierProductId == product.Id);

                if (inventory != null)
                {
                    inventoryInfo[product.Id] = (inventory.Quantity, inventory.Location, inventory.SellingPrice, inventory.Notes);
                }
                else
                {
                    // Nếu sản phẩm chưa có trong kho, thiết lập các giá trị mặc định là "NEW"
                    inventoryInfo[product.Id] = (0, "NEW", 0m, "NEW");
                }
            }

            ViewBag.InventoryInfo = inventoryInfo; // Gán dữ liệu vào ViewBag
            return View(supplier);
        }






        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var supplierProduct = await _db.SupplierProducts.FindAsync(id);
            if (supplierProduct == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy sản phẩm.";
                return RedirectToAction("Display", new { id });
            }

            supplierProduct.Status = status; // Cập nhật trạng thái
            _db.SupplierProducts.Update(supplierProduct);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Trạng thái sản phẩm đã được cập nhật thành công.";
            return RedirectToAction("Display", new { id = supplierProduct.SupplierId });
        }



        [HttpPost]
        public async Task<IActionResult> ConfirmProduct(int supplierProductId, int quantity, string location, decimal sellingPrice, string notes)
        {
            // Lấy thông tin sản phẩm từ nhà cung cấp
            var supplierProduct = await _db.SupplierProducts
                .Include(sp => sp.SupplierProductConfig)
                .FirstOrDefaultAsync(sp => sp.Id == supplierProductId);

            if (supplierProduct == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy sản phẩm.";
                return RedirectToAction("Display", new { id = supplierProductId });
            }

            // Kiểm tra số lượng nhập vào có vượt quá số lượng tồn kho không
            if (quantity > supplierProduct.Quantity)
            {
                TempData["ErrorMessage"] = "Số lượng vượt quá số lượng có trong kho.";
                return RedirectToAction("Display", new { id = supplierProduct.SupplierId });
            }

            // Kiểm tra tồn tại trong bảng Inventory
            var inventory = await _db.Inventories.FirstOrDefaultAsync(i => i.SupplierProductId == supplierProductId);

            if (inventory != null)
            {
                // Cập nhật thông tin tồn kho
                inventory.Quantity += quantity;
                inventory.Location = location;
                inventory.SellingPrice = sellingPrice;
                inventory.Notes = notes;

                // Cập nhật thông tin trong bảng Inventory
                _db.Inventories.Update(inventory);


                // Kiểm tra nếu số lượng trong SupplierProduct đủ để trừ
                if (supplierProduct.Quantity < quantity)
                {
                    // Nếu số lượng không đủ để trừ, hiển thị thông báo lỗi
                    TempData["ErrorMessage"] = "Số lượng không đủ để giảm.";
                    return RedirectToAction("Display", new { id = supplierProduct.SupplierId });
                }

                // Trừ bớt số lượng của sản phẩm trong bảng SupplierProduct
                supplierProduct.Quantity -= quantity;



                // Nếu số lượng của sản phẩm trong SupplierProduct bằng 0, cập nhật trạng thái "Hết hàng"
                if (supplierProduct.Quantity == 0)
                {
                    supplierProduct.Status = "Hết hàng";
                }

                // Cập nhật lại thông tin sản phẩm trong SupplierProduct
                _db.SupplierProducts.Update(supplierProduct);
            }
            else
            {
                // Thêm mới vào tồn kho nếu chưa tồn tại
                inventory = new Inventory
                {
                    SupplierProductId = supplierProductId,
                    Quantity = quantity,
                    Location = location,
                    SellingPrice = sellingPrice,
                    Notes = notes
                };


                supplierProduct.Quantity -= quantity;
                // Nếu số lượng của sản phẩm trong SupplierProduct bằng 0, cập nhật trạng thái "Hết hàng"
                if (supplierProduct.Quantity == 0)
                {
                    supplierProduct.Status = "Hết hàng";
                }

                // Cập nhật lại thông tin sản phẩm trong SupplierProduct
                _db.SupplierProducts.Update(supplierProduct);



                await _db.Inventories.AddAsync(inventory);
            }

            // Lưu tất cả thay đổi vào cơ sở dữ liệu
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Sản phẩm đã được thêm hoặc cập nhật trong kho.";
            return RedirectToAction("Display", new { id = supplierProduct.SupplierId });
        }



        // GET: Hiển thị form thêm nhà cung cấp
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Supplier supplier)
        {
            if (!ModelState.IsValid)
            {
                // Nếu model không hợp lệ, quay lại và hiển thị thông báo lỗi
                return View(supplier);
            }

            // Thêm nhà cung cấp vào cơ sở dữ liệu
            _db.Suppliers.Add(supplier);
            await _db.SaveChangesAsync();

            // Chuyển hướng đến danh sách nhà cung cấp sau khi thêm thành công
            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public IActionResult AddProduct(int supplierId)
        {
            // Tạo một đối tượng `SupplierProduct` với SupplierId được gán trước
            var product = new SupplierProduct
            {
                SupplierId = supplierId,
                ImportDate = DateTime.Now,
                Status = "Chưa được xác nhận"
            };

            // Lấy danh sách danh mục sản phẩm từ cơ sở dữ liệu
            ViewBag.Categories = _db.Categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList();

            return View(product);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(SupplierProduct product, List<IFormFile> ImageFiles)
        {
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"ModelState Error: {error.ErrorMessage}");
                }

                ViewBag.Categories = _db.Categories.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList();
                return View(product);
            }

            // Lưu sản phẩm vào cơ sở dữ liệu
            _db.SupplierProducts.Add(product);
            await _db.SaveChangesAsync();
            Console.WriteLine($"Saved Product Id: {product.Id}");

            if (ImageFiles != null && ImageFiles.Any())
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                foreach (var file in ImageFiles)
                {
                    if (file.Length > 0)
                    {
                        try
                        {
                            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(fileStream);
                            }

                            var productImage = new ProductImage
                            {
                                Url = $"/images/{uniqueFileName}",
                                SupplierProductId = product.Id
                            };

                            Console.WriteLine($"Adding ProductImage: {productImage.Url}, SupplierProductId: {productImage.SupplierProductId}");

                            _db.ProductImages.Add(productImage);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error saving image: {ex.Message}");
                        }
                    }
                }

                await _db.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Sản phẩm đã được thêm thành công";
            return RedirectToAction("Display", new { id = product.SupplierId });
        }











        [HttpPost]
        public async Task<IActionResult> AddProductConfig(int productId, SupplierProductConfig configFields)
        {
            var product = await _db.SupplierProducts.FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
            {
                TempData["ErrorMessage"] = "Sản phẩm không tồn tại.";
                return RedirectToAction("Display", new { id = product.SupplierId });
            }

            // Tạo mới cấu hình sản phẩm hoặc cập nhật nếu đã tồn tại
            var config = await _db.SupplierProductConfigs.FirstOrDefaultAsync(c => c.SupplierProductId == productId) ?? new SupplierProductConfig
            {
                SupplierProductId = productId
            };

            // Cập nhật cấu hình từ các trường
            config.ManHinh = configFields.ManHinh;
            config.HeDieuHanh = configFields.HeDieuHanh;
            config.CameraSau = configFields.CameraSau;
            config.CameraTruoc = configFields.CameraTruoc;
            config.CPU = configFields.CPU;
            config.Ram = configFields.Ram;
            config.BoNhoTrong = configFields.BoNhoTrong;
            config.Sim = configFields.Sim;
            config.DungLuong = configFields.DungLuong;
            config.Color = configFields.Color;
            config.CardDoHoa = configFields.CardDoHoa;
            config.CongKetNoi = configFields.CongKetNoi;
            config.TrongLuong = configFields.TrongLuong;
            config.KetNoi = configFields.KetNoi;
            config.ChatLuongAmThanh = configFields.ChatLuongAmThanh;
            config.ThoiLuongPin = configFields.ThoiLuongPin;
            config.CamBien = configFields.CamBien;
            config.KhaNangChongNuoc = configFields.KhaNangChongNuoc;
            config.KichThuoc = configFields.KichThuoc;
            config.Notes = configFields.Notes;

            if (config.Id == 0)
            {
                await _db.SupplierProductConfigs.AddAsync(config);
            }
            else
            {
                _db.SupplierProductConfigs.Update(config);
            }

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cấu hình sản phẩm đã được lưu thành công.";
            return RedirectToAction("Display", new { id = product.SupplierId });
        }





        [HttpPost]
        public async Task<IActionResult> AddProductConfigDetails(int productId, SupplierProductConfig configFields)
        {
            var product = await _db.SupplierProducts.FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
            {
                TempData["ErrorMessage"] = "Sản phẩm không tồn tại.";
                return RedirectToAction("Display", new { id = product.SupplierId });
            }

            // Tạo mới cấu hình sản phẩm hoặc cập nhật nếu đã tồn tại
            var config = await _db.SupplierProductConfigs.FirstOrDefaultAsync(c => c.SupplierProductId == productId) ?? new SupplierProductConfig
            {
                SupplierProductId = productId
            };

            // Cập nhật cấu hình từ các trường
            config.ManHinh = configFields.ManHinh;
            config.HeDieuHanh = configFields.HeDieuHanh;
            config.CameraSau = configFields.CameraSau;
            config.CameraTruoc = configFields.CameraTruoc;
            config.CPU = configFields.CPU;
            config.Ram = configFields.Ram;
            config.BoNhoTrong = configFields.BoNhoTrong;
            config.Sim = configFields.Sim;
            config.DungLuong = configFields.DungLuong;
            config.Color = configFields.Color;
            config.CardDoHoa = configFields.CardDoHoa;
            config.CongKetNoi = configFields.CongKetNoi;
            config.TrongLuong = configFields.TrongLuong;
            config.KetNoi = configFields.KetNoi;
            config.ChatLuongAmThanh = configFields.ChatLuongAmThanh;
            config.ThoiLuongPin = configFields.ThoiLuongPin;
            config.CamBien = configFields.CamBien;
            config.KhaNangChongNuoc = configFields.KhaNangChongNuoc;
            config.KichThuoc = configFields.KichThuoc;
            config.Notes = configFields.Notes;

            if (config.Id == 0)
            {
                await _db.SupplierProductConfigs.AddAsync(config);
            }
            else
            {
                _db.SupplierProductConfigs.Update(config);
            }

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cấu hình sản phẩm đã được lưu thành công.";
            return RedirectToAction("Details", new { id = productId });
        }




        [HttpPost]
        public async Task<IActionResult> DeleteSupplier(int supplierId)
        {
            var supplier = await _db.Suppliers
                .Include(s => s.SupplierProducts) // Bao gồm sản phẩm liên quan
                .FirstOrDefaultAsync(s => s.Id == supplierId);

            if (supplier == null)
            {
                TempData["ErrorMessage"] = "Nhà cung cấp không tồn tại.";
                return RedirectToAction("Index"); // Quay lại danh sách nhà cung cấp
            }

            // Kiểm tra xem nhà cung cấp có sản phẩm liên quan không
            if (supplier.SupplierProducts.Any())
            {
                TempData["ErrorMessage"] = "Không thể xóa nhà cung cấp vì có sản phẩm đã thêm vào kho.";
                return RedirectToAction("Index");
            }

            // Xóa nhà cung cấp
            _db.Suppliers.Remove(supplier);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Xóa nhà cung cấp thành công.";

            // Quay lại trang Display với id của nhà cung cấp đã xóa
            return RedirectToAction("Display", new { id = supplierId });
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








        [HttpPost]
        public IActionResult EditProductDescription(int Id, string ProductName, string Brand, decimal CostPrice, string Status)
        {
            // Tìm sản phẩm theo Id
            var product = _db.SupplierProducts.FirstOrDefault(sp => sp.Id == Id);

            if (product == null)
            {
                return NotFound();
            }

            // Cập nhật thông tin sản phẩm
            product.ProductName = ProductName;
            product.Brand = Brand;
            product.CostPrice = CostPrice;
            product.Status = Status;

            // Lưu thay đổi vào cơ sở dữ liệu
            _db.SaveChanges();

            // Chuyển hướng về trang chi tiết sản phẩm sau khi lưu thay đổi
            return RedirectToAction("Details", new { id = Id });
        }



        [HttpPost]
        public async Task<IActionResult> EditSupplier(int Id, Supplier supplier,int IdProduct)
        {
            var existingSupplier = await _db.Suppliers.FirstOrDefaultAsync(s => s.Id == Id);

            var product = await _db.SupplierProducts.FindAsync(IdProduct);

            if (existingSupplier == null)
            {
                TempData["ErrorMessage"] = "Nhà cung cấp không tồn tại.";
                return RedirectToAction("Index");
            }

            // Cập nhật thông tin nhà cung cấp
            existingSupplier.SupplierName = supplier.SupplierName;
            existingSupplier.ContactName = supplier.ContactName;
            existingSupplier.PhoneNumber = supplier.PhoneNumber;
            existingSupplier.Email = supplier.Email;
            existingSupplier.Address = supplier.Address;
            existingSupplier.BankAccountNumber = supplier.BankAccountNumber;
            existingSupplier.BankName = supplier.BankName;
            existingSupplier.TaxCode = supplier.TaxCode;
            existingSupplier.Notes = supplier.Notes;

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cập nhật nhà cung cấp thành công!";
            return RedirectToAction("Details", new { id = IdProduct });
        }





        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProductImage(int productId, List<IFormFile> ImageFiles)
        {
            if (ImageFiles != null && ImageFiles.Any())
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                foreach (var file in ImageFiles)
                {
                    if (file.Length > 0)
                    {
                        try
                        {
                            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(fileStream);
                            }

                            // Tạo đối tượng ProductImage mới và lưu vào cơ sở dữ liệu
                            var productImage = new ProductImage
                            {
                                Url = $"/images/{uniqueFileName}",
                                SupplierProductId = productId // Lưu ID của sản phẩm
                            };

                            _db.ProductImages.Add(productImage);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Lỗi khi lưu ảnh: {ex.Message}");
                        }
                    }
                }

                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Ảnh đã được thêm thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Vui lòng chọn ảnh để tải lên!";
            }

            return RedirectToAction("Details", new { id = productId });
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProductImage(int imageId, int productId)
        {
            // Tìm ảnh theo ID
            var image = await _db.ProductImages.FindAsync(imageId);

            if (image != null)
            {
                // Kiểm tra và xóa ảnh khỏi hệ thống
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", image.Url.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath); // Xóa ảnh khỏi thư mục
                }
                else
                {
                    TempData["ErrorMessage"] = "Ảnh không tồn tại trên hệ thống!";
                    return RedirectToAction("Details", new { id = productId });
                }

                // Xóa ảnh khỏi cơ sở dữ liệu
                _db.ProductImages.Remove(image);
                await _db.SaveChangesAsync();

                TempData["SuccessMessage"] = "Ảnh đã được xóa thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy ảnh!";
            }

            // Chuyển hướng về trang chi tiết sản phẩm
            return RedirectToAction("Details", new { id = productId });
        }






    }
}
