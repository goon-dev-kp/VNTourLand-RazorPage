using BLL.Services.Implement;
using BLL.Services.Interface;
using Common.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VNTourLandProject.Pages.Payment
{
    public class CallBackPayPalModel : PageModel
    {
        private readonly IPayPalService _paypalService;
        private readonly IBookingService _bookingService;
        private readonly ITransactionService _transactionService;
        private readonly IEmailService _emailService;
        private readonly IRequestOfCustomerService _requestOfCustomerService;

        public CallBackPayPalModel(IPayPalService paypalService, IBookingService bookingService, ITransactionService transactionService, IEmailService emailService, IRequestOfCustomerService requestOfCustomerService)
        {
            _paypalService = paypalService;
            _bookingService = bookingService;
            _transactionService = transactionService;
            _emailService = emailService;
            _requestOfCustomerService = requestOfCustomerService;
        }

        [BindProperty(SupportsGet = true)]
        public string Token { get; set; }

        public string Message { get; set; }
        public async Task OnGetAsync()
        {
            if (string.IsNullOrEmpty(Token))
            {
                Message = "Không nhận được mã đơn hàng từ PayPal.";
                return;
            }

            Guid bookingId = Guid.Empty; // 👈 KHAI BÁO NGOÀI
            var result = await _paypalService.CaptureOrderAsync(Token);

            if (result)
            {
                var markResult = await _bookingService.MarkAsPaidAsync(Token);

                if (markResult != null)
                {
                    // Nhận cả BookingId và RequestId
                    var (id, requestId) = markResult.Value;
                    bookingId = id;

                    await _transactionService.UpdateByBookingIdAsync(bookingId, Token, PaymentStatus.COMPLETED);
                    // Gửi email xác nhận thanh toán thành công
                    await _emailService.SendEmailBookingAsync(bookingId);

                    // ✅ Nếu có requestId, thì cập nhật trạng thái request
                    if (requestId.HasValue)
                    {
                        await _requestOfCustomerService.ChangeStatusAsync(requestId.Value, RequestStatus.COMPLETED);
                    }

                    Message = "✅ Thanh toán thành công!";
                }
                else
                {
                    Message = "❌ Không thể xác định booking từ đơn hàng.";
                }
            }
            else
            {
                // ❗ Nếu bookingId đã được gán ở trên thì dùng, ngược lại không gọi Update
                if (bookingId != Guid.Empty)
                {
                    await _transactionService.UpdateByBookingIdAsync(bookingId, Token, PaymentStatus.FAILED);
                    // Gửi email xác nhận thanh toán thất bại
                    await _emailService.SendEmailBookingFailedAsync(bookingId);
                }

                Message = "❌ Thanh toán thất bại hoặc bị hủy.";
            }
        }


    }
}
