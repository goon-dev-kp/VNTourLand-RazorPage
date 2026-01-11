using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Models;

namespace DAL.Repositories.Interface
{
    public interface IBookingRepository : IGenericRepository<Booking>
    {
        Task<Booking?> GetBookingDetailByIdAsync(Guid bookingId);
        Task<IEnumerable<Booking>> GetBookingWithTourByUserId(Guid userId);
        Task<List<Booking>> SearchByCodeAsync(string code);
        Task<List<Booking>> GetAllAsync();
        Task<List<Booking>> GetBookingsWithCustomerToursAsync();
        Task<Booking?> GetByIdWithTourAsync(Guid bookingId);
        Task<Booking?> GetBookingWithDetailsForSellerAsync(Guid bookingId);
    }
}
