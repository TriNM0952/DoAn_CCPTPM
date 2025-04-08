using Lab03.Data;
using Lab03.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lab03.Repositories
{
    public class EFInventoryRepository : IInventoryRepository
    {
        private readonly ApplicationDbContext _context;

        public EFInventoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Lấy tất cả Inventory
        public async Task<IEnumerable<Inventory>> GetAllAsync()
        {
            return await _context.Inventories.ToListAsync();
        }

        // Lấy Inventory theo Id
        public async Task<Inventory> GetByIdAsync(int id)
        {
            return await _context.Inventories.FindAsync(id);
        }

        // Thêm Inventory
        public async Task AddAsync(Inventory inventory)
        {
            await _context.Inventories.AddAsync(inventory);
            await _context.SaveChangesAsync();
        }

        // Cập nhật Inventory
        public async Task UpdateAsync(Inventory inventory)
        {
            _context.Inventories.Update(inventory);
            await _context.SaveChangesAsync();
        }

        // Xóa Inventory theo Id
        public async Task DeleteAsync(int id)
        {
            var inventory = await _context.Inventories.FindAsync(id);
            if (inventory != null)
            {
                _context.Inventories.Remove(inventory);
                await _context.SaveChangesAsync();
            }
        }

        // Lấy Inventory theo vị trí kho
        public async Task<IEnumerable<Inventory>> GetByLocationAsync(string location)
        {
            return await _context.Inventories
                .Where(i => i.Location == location)
                .ToListAsync();
        }

        // Lấy các Inventory có số lượng dưới ngưỡng nhất định
        public async Task<IEnumerable<Inventory>> GetLowStockItemsAsync(int threshold)
        {
            return await _context.Inventories
                .Where(i => i.Quantity < threshold)
                .ToListAsync();
        }
    }
}
