using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.DTO;
using Common.Enums;

namespace BLL.Services.Interface
{
    public interface IBookingService
    {
        Task<ResponseDTO> CreateBooking(CreateBookingDTO dto);
        Task<ResponseDTO> GetBookingById(Guid bookingId);
        Task<(Guid bookingId, Guid? requestId)?> MarkAsPaidAsync(string paypalOrderId);
        Task<ResponseDTO> GetMyBookingAsync(Guid userId);

        Task<ResponseDTO> GetAllBookingsAsync();
        Task<ResponseDTO> SearchBookingsByCodeAsync(string code);
        Task<ResponseDTO> GetBookingForSeller();
        Task<ResponseDTO> GetBookingDetailsForSellerAsync(Guid bookingId);

        Task<ResponseDTO> ChangeStatusForManager(Guid bookingId, BookingStatus bookingStatus);
    }
}
