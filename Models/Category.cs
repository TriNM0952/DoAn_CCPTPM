using System.ComponentModel.DataAnnotations;

namespace Lab03.Models
{
    public class Category
    {
        public int Id { get; set; }
        [Required, StringLength(50)]
        public string Name { get; set; }
        public List<Inventory>? Inventories { get; set; }


       
    }
}
