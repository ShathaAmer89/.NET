using Microsoft.EntityFrameworkCore;
using HouseRenting.Models;
using HouseRenting.Migrations;

namespace HouseRenting.DAL
{
    public class BookingRepository : IBookingRepository
    {
        private readonly ItemDbContext _db;
        private readonly ILogger<BookingRepository> _logger;
        public BookingRepository(ItemDbContext db, ILogger<BookingRepository> logger)
        {
            _db = db;
            _logger = logger;
        }
        public async Task<bool> CreateBookingItem(Booking booking, int itemId)
        {
            try
            {
                _db.Bookings.Add(booking);
                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError("[BookingRepository] Booking creation failed, error message: {e}", booking, e.Message);
                return false;
            }

        }
        public async Task<Booking?> GetBookingById(int bookingId)
        {
            try
            {
                return await _db.Bookings.FindAsync( bookingId);
            }
            catch (Exception e)
            {
                _logger.LogError("[BookingRepository] GetBookingById failed, error message: {e}", e.Message);
                return null; 
            }
        }
        public async Task<List<Booking?>> GetBookingByItemId(int itemId)
        {
            try
            {
                return await _db.Bookings.Where(b => b.ItemId == itemId).ToListAsync();
            }
            catch (Exception e)
            {
                _logger.LogError("[BookingRepository] GetBookingByItemId failed, error message: {ErrorMessage}", e.ToString());
                return null;  
            }
        }





        public async Task<bool> DeleteBooking(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogError("[BookingRepository] Invalid BookingId provided for deletion: {BookingId}", id);
                    return false;
                }
                var booking = await _db.Bookings.FindAsync(id);
                if (booking == null)
                {
                    _logger.LogError("[BookingRepository] booking not found for the BookingId {BookingId:0000}", id);
                    return false;
                }                
                _db.Bookings.Remove(booking);
                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError("[BookingRepository] booking deletion failed for the BookingId {BookingId:0000}, error message: {e}", id, e.Message);
                return false;
            }
        }



    }
}
