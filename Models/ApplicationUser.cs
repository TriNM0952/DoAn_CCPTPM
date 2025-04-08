using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Lab03.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string Name { get; set; }
        public string? Address { get; set; }
        // Liên kết tới bảng trung gian UserRoles
        public ICollection<IdentityUserRole<string>> UserRoles { get; set; }
    }
}
