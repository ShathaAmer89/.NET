using System.ComponentModel.DataAnnotations;

namespace HouseRenting.Models
{
    public class Booking
    {
        public int BookingId { get; set; }
        public string BookingDate { get; set; } = String.Empty;

        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }
        public int CustomerId { get; set; }
        public virtual Customer Customer { get; set; } = default!;

        public int ItemId { get; set; }
        public virtual Item? Items { get; set; }
    }
}