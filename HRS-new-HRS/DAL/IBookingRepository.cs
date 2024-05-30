using Microsoft.AspNetCore.Mvc;

using HouseRenting.Models;
namespace HouseRenting.DAL
{
    public interface IBookingRepository
    {
  
        Task<bool> CreateBookingItem(Booking booking, int itemId);
        Task<List<Booking?>> GetBookingByItemId(int id);
        Task<Booking?> GetBookingById(int id);
        Task<bool> DeleteBooking(int id);
        

    }
}
