using FakeFlightBookingAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SharedModels;

namespace FakeFlightBookingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookedFlightController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;

        public BookedFlightController(ApplicationDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        [HttpGet("GetBookedFlights")]
        public async Task<ActionResult<IEnumerable<BookedFlight>>> GetBookedFlights()
        {
            const string cacheKey = "BookedFlightsList";
            if (!_cache.TryGetValue(cacheKey, out List<BookedFlight> bookedFlights))
            {
                bookedFlights = await _context.BookedFlights.ToListAsync();
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                };
                _cache.Set(cacheKey, bookedFlights, cacheOptions);
            }

            return bookedFlights;
        }

        [HttpGet("GetBookedFlightByID")]
        public async Task<ActionResult<BookedFlight>> GetBookedFlightByID([FromQuery] int id)
        {
            var bookedFlight = await _context.BookedFlights.FindAsync(id);
            if (bookedFlight == null)
            {
                return NotFound();
            }
            return bookedFlight;
        }

        [HttpPost("AddBookedFlight")]
        public async Task<ActionResult<BookedFlight>> PostBookedFlight([FromBody] BookedFlight bookedFlight)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate the UserID (ensure the user exists)
            var user = await _context.CustomerUsers.FindAsync(bookedFlight.CustomerId);
            if (user == null)
            {
                return BadRequest("Invalid User ID.");
            }

            // Add the booked flight to the database
            bookedFlight.BookingDate = DateTime.UtcNow; // Ensure booking date is set
            _context.BookedFlights.Add(bookedFlight);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBookedFlightByID), new { id = bookedFlight.BookingId }, bookedFlight);
        }

        [HttpPut("UpdateBookedFlightByID")]
        public async Task<IActionResult> PutBookedFlight([FromQuery] int id, BookedFlight bookedFlight)
        {
            if (id != bookedFlight.BookingId)
            {
                return BadRequest();
            }

            _context.Entry(bookedFlight).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("DeleteBookedFlightByID")]
        public async Task<IActionResult> DeleteBookedFlight([FromQuery] int id)
        {
            var bookedFlight = await _context.BookedFlights.FindAsync(id);
            if (bookedFlight == null)
            {
                return NotFound();
            }

            _context.BookedFlights.Remove(bookedFlight);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
