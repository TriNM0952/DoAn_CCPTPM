using Lab03.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lab03.Repositories
{
    public interface IInventoryRepository
    {
        Task<IEnumerable<Inventory>> GetAllAsync();
        Task<Inventory> GetByIdAsync(int id);
        Task AddAsync(Inventory inventory);
        Task UpdateAsync(Inventory inventory);
        Task DeleteAsync(int id);

        // Phương thức đặc biệt cho Inventory nếu cần
        Task<IEnumerable<Inventory>> GetByLocationAsync(string location);
        Task<IEnumerable<Inventory>> GetLowStockItemsAsync(int threshold);
    }
}
