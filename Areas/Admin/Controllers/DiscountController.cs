using Lab03.Data;
using Lab03.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Lab03.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DiscountController : Controller
    {
        private readonly ApplicationDbContext _db;

        public DiscountController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var discounts = await _db.Discounts.ToListAsync();
            return View(discounts);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Discount discount)
        {
            if (ModelState.IsValid)
            {
                _db.Discounts.Add(discount);
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Mã giảm giá đã được tạo thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(discount);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var discount = await _db.Discounts.FindAsync(id);
            if (discount == null)
            {
                return NotFound();
            }
            return View(discount);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Discount discount)
        {
            if (id != discount.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _db.Update(discount);
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Mã giảm giá đã được cập nhật!";
                return RedirectToAction(nameof(Index));
            }
            return View(discount);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var discount = await _db.Discounts.FindAsync(id);
            if (discount == null)
            {
                return NotFound();
            }

            return View(discount);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var discount = await _db.Discounts.FindAsync(id);
            if (discount == null)
            {
                return NotFound();
            }

            _db.Discounts.Remove(discount);
            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Mã giảm giá đã được xóa!";
            return RedirectToAction(nameof(Index));
        }

    }
}
