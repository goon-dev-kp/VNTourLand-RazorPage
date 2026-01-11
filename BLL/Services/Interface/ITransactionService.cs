using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.DTO;
using Common.Enums;

namespace BLL.Services.Interface
{
    public interface ITransactionService
    {
        Task<ResponseDTO> CreateTransactionAsync (CreateTransactionDTO dto);
        Task<bool> UpdateByBookingIdAsync(Guid bookingId, string transactionRef, PaymentStatus newStatus);
        Task<ResponseDTO> CallBackVnPay(Guid transactionId,string transactionRef , PaymentStatus status, BookingStatus bookingStatus , RequestStatus requestStatus);
        Task<ResponseDTO> CallBackStripeAsync(Guid bookingId, PaymentStatus paymentStatus, BookingStatus bookingStatus);
        Task<ResponseDTO> ProcessSepayCallbackAsync(Guid transactionId, bool isSuccess);
        Task<ResponseDTO> GetAllTransactionsAsync();

    }
}
