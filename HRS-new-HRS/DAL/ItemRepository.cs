using Microsoft.EntityFrameworkCore;
using HouseRenting.Models;
using HouseRenting.ViewModels;

namespace HouseRenting.DAL
{
    public class ItemRepository : IItemRepository
    {
        private readonly ItemDbContext _db;
        private readonly ILogger<ItemRepository> _logger;
        public ItemRepository(ItemDbContext db, ILogger<ItemRepository> logger)
        {
            _db = db;
            _logger = logger;
        }
        public async Task<IEnumerable<Item>> GetAll()
        {
            try
            {
                return await _db.Items.ToListAsync();
            }
            catch (Exception e) 
            {
                _logger.LogError("[ItemRepository] items ToListAsync() failed when GetAll(), error message: {e}", e.Message);
                return null;            
            }
            
        }
        public async Task<Item?> GetItemById(int id)
        {
            try
            {
                return await _db.Items.FindAsync(id);
            }
            catch (Exception e)
            {
                _logger.LogError("[ItemRepository] item FindAsync(id) failed when GetItemById for ItemId {ItemId:0000}, error message: {e}", id, e.Message);
                return null;
            }
            
        }
        public async Task<bool> Create(ItemImagesViewModel viewModel)
        {
            try
            {
                var item = viewModel.Item;
                var images = viewModel.Images;
                _db.Items.Add(item);
                await _db.SaveChangesAsync();
                foreach (var image in images)
                {
                    image.ItemId = item.ItemId;
                    _db.Images.Add(image);
                }
                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError("[ItemRepository] item creation failed for item {@viewModel}, error message: {e}", viewModel, e.Message);
                return false;
            }
        }

        public async Task<bool> Update(ItemImagesViewModel viewModel)
        {

            try
            {
                _db.Items.Attach(viewModel.Item);
                _db.Entry(viewModel.Item).State = EntityState.Modified;                
                _db.Images.RemoveRange(_db.Images.Where(img => img.ItemId == viewModel.Item.ItemId));                
                foreach (var image in viewModel.Images)
                {
                    image.ItemId = viewModel.Item.ItemId;
                    _db.Images.Add(image);
                }

                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError("[ItemRepository] Item update failed for ItemId {ItemId:0000}, error message: {e}", viewModel.Item.ItemId, e.Message);
                
                return false;
            }
        }

        public async Task<bool> Delete(int id)
        {
            try
            {
                var item = await _db.Items.FindAsync(id);
                if (item == null)
                {
                    _logger.LogError("[ItemRepository] item not found for the ItemId {ItemId:0000}", id);
                    return false;
                }

                _db.Items.Remove(item);
                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError("[ItemRepository] item deletion failed for the ItemId {ItemId:0000}, error message: {e}", id, e.Message);
                return false;
            }
        }
    }
}
