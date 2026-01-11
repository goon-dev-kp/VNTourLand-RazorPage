using BLL.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VNTourLandProject.Pages.Payment
{
    public class CallBackStripeModel : PageModel
    {
        private readonly ITransactionService _transactionService;
        private readonly IEmailService _emailService;
        private readonly IBookingService _bookingService;

        public CallBackStripeModel(ITransactionService transactionService, IEmailService emailService, IBookingService bookingService)
        {
            _transactionService = transactionService;
            _emailService = emailService;
            _bookingService = bookingService;
        }

        [BindProperty(SupportsGet = true)]
        public Guid BookingId { get; set; }

        public string Message { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (BookingId == Guid.Empty)
            {
                Message = "Booking not found.";
                return Page();
            }

            // Gọi cập nhật trạng thái giao dịch
            var response = await _transactionService.CallBackStripeAsync(BookingId, Common.Enums.PaymentStatus.COMPLETED, Common.Enums.BookingStatus.CONFIRMED);

            if (response.IsSuccess)
            {
                await _emailService.SendEmailBookingAsync(BookingId);
                Message = "Payment successfullt!";
            }
            else
            {
                Message = "Error while updating Stripe transaction.";
            }

            return Page();
        }
    }
}
