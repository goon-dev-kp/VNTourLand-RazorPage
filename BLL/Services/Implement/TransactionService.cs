using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLL.Services.Interface;
using Common.DTO;
using Common.Enums;
using DAL.Models;
using DAL.UnitOfWork;

namespace BLL.Services.Implement
{
    public class TransactionService : ITransactionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRequestOfCustomerService _requestOfCustomerService;

        public TransactionService(IUnitOfWork unitOfWork, IRequestOfCustomerService requestOfCustomerService)
        {
            _unitOfWork = unitOfWork;
            _requestOfCustomerService = requestOfCustomerService;
        }
        public async Task<ResponseDTO> CreateTransactionAsync(CreateTransactionDTO dto)
        {
            try
            {
                var transaction = new Transaction
                {
                    TransactionId = Guid.NewGuid(),
                    BookingId = dto.BookingId,
                    Amount = dto.Amount,
                    PaymentMethod = dto.PaymentMethod,
                    TransactionReference = dto.TransactionReference,
                    Status = PaymentStatus.PENDING,
                    PaymentDate = DateTime.UtcNow
                };

                await _unitOfWork.TransactionRepo.AddAsync(transaction);
                await _unitOfWork.SaveChangeAsync();

                var resultDto = new TransactionDTO
                {
                    TransactionId = transaction.TransactionId,
                    BookingId = transaction.BookingId,
                    Amount = transaction.Amount,
                    PaymentMethod = transaction.PaymentMethod,
                    TransactionReference = transaction.TransactionReference,
                    PaymentDate = transaction.PaymentDate,
                    Status = transaction.Status.ToString()
                };

                return new ResponseDTO("Transaction created successfully", 200, true, resultDto);
            }
            catch (Exception ex)
            {
                return new ResponseDTO("Failed to create transaction", 500, false, ex.Message);
            }
        }


        public async Task<bool> UpdateByBookingIdAsync(Guid bookingId, string transactionRef, PaymentStatus newStatus)
        {
            // Find the first transaction associated with the booking
            var transaction = await _unitOfWork.TransactionRepo
                .GetFirstOrDefaultAsync(t => t.BookingId == bookingId);

            if (transaction == null)
            {
                Console.WriteLine($"❌ No transaction found for BookingId = {bookingId}");
                return false;
            }

            transaction.Status = newStatus;
            transaction.TransactionReference = transactionRef;
            transaction.PaymentDate = DateTime.Now;

            await _unitOfWork.SaveChangeAsync();
            Console.WriteLine($"✅ Transaction {transaction.TransactionId} has been updated with status {newStatus}");
            return true;
        }


        public async Task<ResponseDTO> CallBackVnPay(Guid transactionId, string transactionRef, PaymentStatus status, BookingStatus bookingStatus, RequestStatus requestStatus)
        {
            // Retrieve the transaction by ID
            var transaction = await _unitOfWork.TransactionRepo.GetByIdAsync(transactionId);

            if (transaction == null)
            {
                return new ResponseDTO("Transaction not found!", 400, false);
            }

            transaction.Status = status; // Update the transaction status
            transaction.TransactionReference = transactionRef;

            var booking = await _unitOfWork.BookingRepo.GetByIdWithTourAsync(transaction.BookingId);

            if (booking == null)
            {
                return new ResponseDTO("Booking not found!", 400, false);
            }

            booking.Status = bookingStatus; // Update the booking status

            // ✅ If the tour is linked with a request, update the request status
            if (booking.Tour?.RequestId != null)
            {
                await _requestOfCustomerService.ChangeStatusAsync(
                    booking.Tour.RequestId.Value,
                    requestStatus
                );
            }

            await _unitOfWork.SaveChangeAsync(); // ✅ Update the transaction status in the DB
            return new ResponseDTO("Payment successful", 200, true, booking.BookingId);
        }


        public async Task<ResponseDTO> CallBackStripeAsync(Guid bookingId, PaymentStatus status, BookingStatus bookingStatus)
        {
            // Retrieve the first transaction by bookingId
            var transaction = await _unitOfWork.TransactionRepo
                .GetFirstOrDefaultAsync(t => t.BookingId == bookingId);

            if (transaction == null)
            {
                return new ResponseDTO("Transaction not found!", 400, false);
            }

            transaction.Status = status;
            transaction.TransactionReference = "STRIPE-" + DateTime.UtcNow.Ticks;
            transaction.PaymentDate = DateTime.Now;

            // Update the booking status
            var booking = await _unitOfWork.BookingRepo.GetByIdWithTourAsync(bookingId);
            if (booking == null)
            {
                return new ResponseDTO("Booking not found!", 400, false);
            }

            booking.Status = bookingStatus;

            // ✅ If the tour is linked with a request, update the request status
            if (booking.Tour?.RequestId != null)
            {
                await _requestOfCustomerService.ChangeStatusAsync(
                    booking.Tour.RequestId.Value,
                    RequestStatus.COMPLETED
                );
            }

            await _unitOfWork.SaveChangeAsync();
            return new ResponseDTO("Stripe callback successful!", 200, true, booking.BookingId);
        }


        public async Task<ResponseDTO> ProcessSepayCallbackAsync(Guid transactionId, bool isSuccess)
        {
            // Lấy transaction từ DB
            var transaction = await _unitOfWork.TransactionRepo.GetByIdAsync(transactionId);
            if (transaction == null)
            {
                return new ResponseDTO("Transaction not found!", 400, false);
            }

            // ✅ Gán status & transactionRef
            transaction.Status = isSuccess ? PaymentStatus.COMPLETED : PaymentStatus.FAILED;
            transaction.TransactionReference = "SEPAY_" + DateTime.Now.Ticks; // giả sử không có mã giao dịch thật từ Sepay

            // Lấy booking tương ứng
            var booking = await _unitOfWork.BookingRepo.GetByIdWithTourAsync(transaction.BookingId);
            if (booking == null)
            {
                return new ResponseDTO("Booking not found!", 400, false);
            }

            // ✅ Gán trạng thái Booking
            booking.Status = isSuccess ? BookingStatus.CONFIRMED : BookingStatus.CANCELLED;

            // ✅ Nếu booking có request -> cập nhật request status
            if (booking.Tour?.RequestId != null)
            {
                await _requestOfCustomerService.ChangeStatusAsync(
                    booking.Tour.RequestId.Value,
                    isSuccess ? RequestStatus.COMPLETED : RequestStatus.WAITING_PAYMENT
                );
            }

            await _unitOfWork.SaveChangeAsync();

            return new ResponseDTO(
                isSuccess ? "Sepay payment completed" : "Sepay payment failed",
                200,
                true,
                booking.BookingId
            );
        }


        public async Task<ResponseDTO> GetAllTransactionsAsync()
        {
            try
            {
                var transactions = await _unitOfWork.TransactionRepo.GetAllAsync();

                var result = transactions.Select(t => new TransactionManage
                {
                    TransactionId = t.TransactionId,
                    Amount = t.Amount,
                    PaymentMethod = t.PaymentMethod,
                    PaymentDate = t.PaymentDate,
                    Status = t.Status,
                    TransactionReference = t.TransactionReference,
                    BookingId = t.BookingId,
                    FullName = t.Booking?.FullName,
                    Email = t.Booking?.Email,
                    TourName = t.Booking?.Tour?.TourName,
                    Booking = new BookingManage
                    {
                        BookingId = t.Booking.BookingId,
                        FullName = t.Booking.FullName,
                        Email = t.Booking.Email,

                        BookingDate = t.Booking.BookingDate,
                    }
                }).ToList();

                return new ResponseDTO("Transaction list retrieved successfully", 200, true, result);
            }
            catch (Exception ex)
            {
                return new ResponseDTO("Error retrieving transaction list", 500, false, ex.Message);
            }
        }

    }
}
