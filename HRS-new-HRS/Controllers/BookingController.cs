using HouseRenting.DAL;
using HouseRenting.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging; 
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using HouseRenting.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace HouseRenting.Controllers
{
    public class BookingController : Controller
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly ILogger<BookingController> _logger;
        private readonly ItemDbContext _itemDbContext;

        public BookingController(ItemDbContext itemDbContext, IBookingRepository bookingRepository, ILogger<BookingController> logger)
        {
            _itemDbContext = itemDbContext;
            _bookingRepository = bookingRepository;
            _logger = logger;
        }

        public async Task<IActionResult> Table()
        {
            string loggedInUserEmail = User.FindFirstValue(ClaimTypes.Email);
            List<Booking> bookings = await _itemDbContext.Bookings
                .Where(b => b.Customer.Email == loggedInUserEmail)
                .ToListAsync();
            return View(bookings);
        }
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> AllBookingsTable()
        {
            //string loggedInUserEmail = User.Identity.Name;
            List<Booking> allBookings = await _itemDbContext.Bookings        
                .ToListAsync();
            return View(allBookings);
        }


        [HttpGet]
        [Authorize]
        public IActionResult CreateBookingItem(int itemId)
        {
            var item = _itemDbContext.Items.FirstOrDefault(i => i.ItemId == itemId);

            if (item == null)
            {
                return NotFound();
            }

            Booking booking = new Booking { ItemId = itemId };

            
            var viewModel = new CreateBookingViewModel
            {
                Item = item,
                Booking = booking
            };

            
            return View(viewModel);
        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateBookingItem(Booking booking, int itemId)
        {
            // Fetch the Item using the itemId
            var item = _itemDbContext.Items.FirstOrDefault(i => i.ItemId == itemId);

            // Create the view model
            var viewModel = new CreateBookingViewModel
            {
                Item = item,
                Booking = booking
            };

            if (ModelState.IsValid)
            {
                // Check if booking period overlaps with existing bookings
                if (await BookingPeriodOverlaps(booking))
                {
                    ModelState.AddModelError("Booking.StartDate", "The selected date range overlaps with an existing booking.");
                    return View("CreateBookingItem", viewModel);
                }

                // Check if EndDate is not less than StartDate
                if (booking.EndDate < booking.StartDate)
                {
                    ModelState.AddModelError("Booking.EndDate", "End date cannot be earlier than start date.");
                    return View("CreateBookingItem", viewModel);
                }

                // Check if booking was created successfully
                bool isBookingCreated = await _bookingRepository.CreateBookingItem(booking, itemId);

                if (isBookingCreated)
                {
                    booking.Items.IsBooked = true;

                    await _itemDbContext.SaveChangesAsync();
                    DateTime startDate = (DateTime)booking.StartDate;
                    string formattedStartDate = startDate.ToShortDateString();
                    DateTime endDate = (DateTime)booking.EndDate;
                    string formattedEndDate = endDate.ToShortDateString();

                    // Table message
                    string tableHtml = $"<table class=\"table table-striped\">" +
                        $"<tr><th>House number</th><th>Customer Name</th><th>Booking Date</th><th>Start reservation Date</th><th>End reservation Date</th></tr>" +
                        $"<tr><td>{booking.ItemId}</td><td>{booking.Customer.Name}</td><td>{booking.BookingDate}</td><td>{formattedStartDate}</td><td>{formattedEndDate}</td></tr>" +
                        $"</table>";

                    TempData["BookingTable"] = tableHtml;

                    // Confirmation message without the table
                    string confirmationMessage = $"We confirm that the house number {booking.ItemId} are booked to {booking.Customer.Name} in the date:{booking.BookingDate} in the period[{formattedStartDate} ,{formattedEndDate}]!";

                    TempData["BookingConfirmation"] = confirmationMessage;

                    // Redirect to Receipt view
                    return View("Receipt", viewModel);
                }
            }

            // If there are validation errors or booking creation failed, set ModelState for ItemId and return to the same view with the validation errors
            _logger.LogWarning("[BookingController] Booking creation failed", booking);

            return View("CreateBookingItem", viewModel);
        }

        private async Task<bool> BookingPeriodOverlaps(Booking newBooking)
        {
            // Get existing bookings for the same item
            var existingBookings = await _bookingRepository.GetBookingByItemId(newBooking.ItemId);

            // Check for date overlap


            foreach (var b in existingBookings)
            {
                if (newBooking.StartDate < b.EndDate && newBooking.EndDate > b.StartDate)
                {
                    // Overlap found
                    return true;
                }
            }

            // No overlap
            return false;
        }
        [HttpGet]
        public async Task<IActionResult> BookingPeriods(int itemId)
        {
            var existingBookings = await _bookingRepository.GetBookingByItemId(itemId);
            if (existingBookings != null)
            {
                string periods = $"This House is not available in the periods:\n";
                foreach (var b in existingBookings)
                {
                    if (DateTime.Now < b.EndDate)
                    {
                        await _itemDbContext.SaveChangesAsync();
                        DateTime startDate = (DateTime)b.StartDate;
                        string formattedStartDate = startDate.ToShortDateString();
                        DateTime endDate = (DateTime)b.EndDate;
                        string formattedEndDate = endDate.ToShortDateString();
                        periods += $"[{formattedStartDate}, {formattedEndDate}]\n";
                    }
                }

                return Content(periods, "text/plain");
            }

            return Content(null, "text/plain");
        }




        [HttpGet]
        [Authorize]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            var booking = await _bookingRepository.GetBookingById(id);
            if (booking == null)
            {
                _logger.LogError("[BookingController] Booking not found for the BookingId {BookingId:0000}", id);
                return BadRequest("Booking not found for the BookingId");
            }
            return View(booking);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteBookingConfirmed(int id)
        {
            bool Ok = await _bookingRepository.DeleteBooking(id);
            if (!Ok)
            {
                _logger.LogError("[BookingController] Booking deletion failed for the  BookingId { BookingId:0000}", id);
                return BadRequest(" Booking deletion failed");
            }
            return RedirectToAction(nameof(Table));
        }

       
    }
}
