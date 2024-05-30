using HouseRenting.Models;
using HouseRenting.ViewModels;

namespace HouseRenting.DAL
{
    public interface IItemRepository
    {
        Task<IEnumerable<Item>> GetAll();
        Task<Item?> GetItemById(int id);
        Task<bool> Create(ItemImagesViewModel viewModel);
        Task<bool> Update(ItemImagesViewModel viewModel);
        Task<bool> Delete(int id);
    }
}
