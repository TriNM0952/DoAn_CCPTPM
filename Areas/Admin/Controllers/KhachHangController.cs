using Lab03.Data;
using Lab03.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Lab03.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class KhachHangController : Controller
    {
        private readonly ApplicationDbContext _db;

        public KhachHangController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            var applications = _db.ApplicationUser.ToList();

            // Xác định quyền theo Area
            var currentArea = (string)RouteData.Values["area"];
            ViewData["UserRole"] = currentArea switch
            {
                "Admin" => "Admin",
                "Sales" => "Sales",
                _ => "Customer"
            };

            return View(applications);
        }
    }
}
