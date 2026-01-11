using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Enums;
using DAL.Data;
using DAL.Models;
using DAL.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories.Implement
{
    public class BookingRepository : GenericRepository<Booking> , IBookingRepository
    {
        public readonly ApplicationDbContext _context;
        public BookingRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Booking?> GetBookingDetailByIdAsync(Guid bookingId)
        {
            return await _context.Bookings
                .Include(b => b.Tour)
                    .ThenInclude(t => t.TourLocations)
                        .ThenInclude(tl => tl.Location)
                .Include(b => b.User)
               
                //.Include(b => b.OptionOnTours)
                //    .ThenInclude(o => o.AddOnOption)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);
        }

        public async Task<IEnumerable<Booking>> GetBookingWithTourByUserId(Guid userId)
        {
            return await _context.Bookings
                .Include(b => b.Tour)
                    .ThenInclude(t => t.TourLocations)
                        .ThenInclude(tl => tl.Location)
                .Where(b => b.UserId == userId)
                .ToListAsync();
        }

        public async Task<List<Booking>> GetAllAsync()
        {
            return await _context.Bookings
                .Include(b => b.Tour)
                .ToListAsync();
        }
        public async Task<List<Booking>> SearchByCodeAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return await  GetAllAsync();
            }

            return await _context.Bookings
                .Include(b => b.Tour)
                .Where(b => b.Code.Contains(code))
                .ToListAsync();
        }

        public async Task<List<Booking>> GetBookingsWithCustomerToursAsync()
        {
            return await _context.Bookings
                .Include(b => b.Tour)
                    .ThenInclude(t => t.Participants)
                .ToListAsync();
        }

        public async Task<Booking?> GetByIdWithTourAsync(Guid bookingId)
        {
            return await _context.Bookings
                .Include(b => b.Tour)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);
        }

        public async Task<Booking?> GetBookingWithDetailsForSellerAsync(Guid bookingId)
        {
            return await _context.Bookings
    .Include(b => b.Tour)
        .ThenInclude(t => t.Itineraries)
            .ThenInclude(i => i.Activities)
    .Include(b => b.Tour)
        .ThenInclude(t => t.Included)
    .Include(b => b.Tour)
        .ThenInclude(t => t.NotIncluded)
    .Include(b => b.Tour)
        .ThenInclude(t => t.TourLocations)     // Include TourLocations collection
            .ThenInclude(tl => tl.Location)    // Include related Location entity for each TourLocation
    .FirstOrDefaultAsync(b => b.BookingId == bookingId);

        }


    }
}
