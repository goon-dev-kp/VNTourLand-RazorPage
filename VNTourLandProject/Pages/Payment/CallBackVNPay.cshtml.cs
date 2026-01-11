using BLL.Services.Implement;
using BLL.Services.Interface;
using DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VNTourLandProject.Pages.Payment
{
    public class CallBackVNPayModel : PageModel
    {
        private readonly IVnPayService _vnPayService;
        private readonly IBookingService _bookingService;
        private readonly ITransactionService _transactionService;
        private readonly IEmailService _emailService;

        public CallBackVNPayModel(IVnPayService vnPayService, IBookingService bookingService, ITransactionService transactionService, IEmailService emailService)
        {
            _vnPayService = vnPayService;
            _bookingService = bookingService;
            _transactionService = transactionService;
            _emailService = emailService;
        }

        public string Message { get; set; }
        public Guid TransactionId { get; set; }
        public string VnpayTransactionNo { get; set; } = "";
        public string VnpayResponseCode { get; set; } = "";
        public async Task OnGetAsync()
        {
            // 🔹 Lấy giá trị từ query string
            string vnpOrderInfo = Request.Query["vnp_OrderInfo"]; // Chứa TransactionId
            string vnpayTransactionNo = Request.Query["vnp_TransactionNo"];
            string responseCode = Request.Query["vnp_ResponseCode"];

            // 🔹 Kiểm tra Transaction ID
            if (string.IsNullOrEmpty(vnpOrderInfo) || !vnpOrderInfo.Contains("/"))
            {
                return;
            }

            string transactionIdStr = vnpOrderInfo.Split('/')[0];

            if (!Guid.TryParse(transactionIdStr, out Guid transactionId))
            {
                return;
            }



            TransactionId = transactionId;
            VnpayTransactionNo = vnpayTransactionNo;
            VnpayResponseCode = responseCode;

            // 🔹 Kiểm tra trạng thái thanh toán
            if (responseCode == "00") // 00 = thành công
            {
                var responseSuccess = await _transactionService.CallBackVnPay(TransactionId, vnpayTransactionNo, Common.Enums.PaymentStatus.COMPLETED, Common.Enums.BookingStatus.CONFIRMED, Common.Enums.RequestStatus.COMPLETED);
                if (responseSuccess.IsSuccess && responseSuccess.Result is Guid bookingId)
                {

                    await _emailService.SendEmailBookingAsync(bookingId);
                    Message = "Payment successfully!";

                }
            }
            else
            {

                var responseFalse = await _transactionService.CallBackVnPay(TransactionId, vnpayTransactionNo, Common.Enums.PaymentStatus.FAILED, Common.Enums.BookingStatus.CANCELLED, Common.Enums.RequestStatus.WAITING_PAYMENT);
                if (responseFalse.IsSuccess && responseFalse.Result is Guid bookingId)
                {
                    await _emailService.SendEmailBookingAsync(bookingId);
                    Message = "Payment failed!";

                }

            }
            
        }
    }
    
}
