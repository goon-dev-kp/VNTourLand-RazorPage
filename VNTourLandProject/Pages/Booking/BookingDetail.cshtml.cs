using BLL.Services.Implement;
using BLL.Services.Interface;
using BLL.Utilities;
using Common.DTO;
using Common.Enums;
using Common.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.V2;
using VNPAY.NET.Enums;
using VNPAY.NET.Models;

namespace VNTourLandProject.Pages.Booking
{
    public class BookingDetailModel : PageModel
    {
        private readonly UserUtility _userUtility;
        private readonly IBookingService _bookingService;
        private readonly IPayPalService _paypalService;
        private readonly ITransactionService _transactionService;
        private readonly IVnPayService _vnpayService;
        private readonly IStripePaymentService _stripePaymentService;
        private readonly IChatService _chatService;
        private readonly IFileUploadService _fileUploadService;
        private readonly IOptions<StripeOptions> _stripeOptions;
        private readonly ISepayService _sepayService;


        public BookingDetailModel(UserUtility userUtility, IBookingService bookingService, IPayPalService paypalService, ITransactionService transactionService, IVnPayService vnpayService, IStripePaymentService stripePaymentService, IChatService chatService, IFileUploadService fileUploadService, IOptions<StripeOptions> stripeOptions, ISepayService sepayService)
        {
            _userUtility = userUtility;
            _bookingService = bookingService;
            _paypalService = paypalService;
            _transactionService = transactionService;
            _vnpayService = vnpayService;
            _stripePaymentService = stripePaymentService;
            _chatService = chatService;
            _fileUploadService = fileUploadService;
            _stripeOptions = stripeOptions;
            _sepayService = sepayService;
        }

        //chat
        public List<MessageDTO> ChatHistory { get; set; } = new();
        public List<UserChatDTO> ChatCustomers { get; set; }

        public bool isAuth => _userUtility.IsAuthenticated();
        public Guid CurrentUserId => isAuth ? _userUtility.GetUserIDFromToken() : Guid.Empty;
        public string Role => isAuth ? _userUtility.GetRoleFromToken() : string.Empty;
        public string UserName => isAuth ? _userUtility.GetFullNameFromToken() : "Guest";
        public string StripePublicKey => _stripeOptions.Value.PublicKey;
        public string StripeSecretKey => _stripeOptions.Value.SecretKey;




        [BindProperty (SupportsGet = true)]
        public Guid BookingId { get; set; }

        public GetBookingDTO Booking { get; set; }



        public async Task<IActionResult> OnGetAsync()
        {
            if (!isAuth)
            {
                TempData["Error"] = "You need to log in first.";
                return RedirectToPage("/Auth/Login");
            }

            var bookingResponse = await _bookingService.GetBookingById(BookingId);

            if (!bookingResponse.IsSuccess || bookingResponse.Result is not GetBookingDTO dto)
            {
                TempData["Error"] = "Failed to load booking details.";
                return RedirectToPage("/Error");
            }

            Booking = dto;
            ChatCustomers = await LoadChatCustomersAsync();

            return Page();
        }



        // 👇 HÀM XỬ LÝ KHI CLICK NÚT PAYPAL
        public async Task<IActionResult> OnPostPayWithPaypalAsync()
        {
            var bookingResponse = await _bookingService.GetBookingById(BookingId);
            if (!bookingResponse.IsSuccess || bookingResponse.Result is not GetBookingDTO dto)
            {
                TempData["Error"] = "Booking not found.";
                return RedirectToPage("/Error");
            }

            var amount = dto.TotalPrice;

            var createTransactionDTO = new CreateTransactionDTO
            {
                BookingId = BookingId,
                Amount = amount,
                PaymentMethod = "PayPal",
                TransactionReference = "N/A"
            };

            var transactionResponse = await _transactionService.CreateTransactionAsync(createTransactionDTO);
            if (!transactionResponse.IsSuccess)
            {
                TempData["Error"] = "Failed to create transaction.";
                return RedirectToPage("/Error");
            }

            var redirectUrl = await _paypalService.CreateOrderAndGetRedirectUrlAsync(amount, BookingId);
            TempData["Message"] = "Redirecting to PayPal...";
            return Redirect(redirectUrl);
        }


        // 👇 HÀM XỬ LÝ KHI CLICK NÚT VNPAY
        public async Task<IActionResult> OnPostPayWithVNPayAsync()
        {
            var bookingResponse = await _bookingService.GetBookingById(BookingId);
            if (!bookingResponse.IsSuccess || bookingResponse.Result is not GetBookingDTO dto)
            {
                TempData["Error"] = "Booking not found.";
                return RedirectToPage("/Error");
            }

            var amount = dto.TotalPrice;

            var createTransactionDTO = new CreateTransactionDTO
            {
                BookingId = BookingId,
                Amount = amount,
                PaymentMethod = "VNPay",
                TransactionReference = "N/A"
            };

            var transactionResponse = await _transactionService.CreateTransactionAsync(createTransactionDTO);
            if (!transactionResponse.IsSuccess || transactionResponse.Result is not TransactionDTO transaction)
            {
                TempData["Error"] = "Failed to create transaction.";
                return RedirectToPage("/Error");
            }

            var ipAddress = "127.0.0.1";

            var finalAmount = (long)(dto.TotalPrice * 26000); // VNPay yêu cầu số tiền tính bằng đồng

            var paymentRequest = new PaymentRequest
            {
                PaymentId = DateTime.UtcNow.Ticks,
                Money = finalAmount,
                Description = $"{transaction.TransactionId}/booking",
                IpAddress = ipAddress,
                BankCode = BankCode.ANY,
                CreatedDate = DateTime.UtcNow,
                Currency = Currency.VND,
                Language = DisplayLanguage.Vietnamese
            };

            var vnPayURL = await _vnpayService.CreatePaymentUrlAsync(paymentRequest);
            TempData["Message"] = "Redirecting to VNPay...";
            return Redirect(vnPayURL);
        }


        public async Task<IActionResult> OnPostCreateIntentAsync([FromBody] CreateIntentDTO createDto)
        {
            var bookingResponse = await _bookingService.GetBookingById(createDto.BookingId);
            if (!bookingResponse.IsSuccess || bookingResponse.Result is not GetBookingDTO dto)
                return BadRequest();

            StripeConfiguration.ApiKey = StripeSecretKey; // 🔁 Thay bằng key thật

            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(dto.TotalPrice * 100),
                Currency = "usd",
                Metadata = new Dictionary<string, string>
        {
            { "bookingId", dto.BookingId.ToString() }
        }
            };

            // Tạo giao dịch trước (nếu bạn muốn lưu transaction trước khi tạo PaymentIntent)
            var createTransactionDTO = new CreateTransactionDTO
            {
                BookingId = dto.BookingId,
                Amount = dto.TotalPrice * 100,
                PaymentMethod = "Stripe",  // Sửa lại nếu dùng Stripe thay vì VNPay
                TransactionReference = "N/A"
            };

            var transactionResponse = await _transactionService.CreateTransactionAsync(createTransactionDTO);
            if (!transactionResponse.IsSuccess || transactionResponse.Result is not TransactionDTO transaction)
            {
                TempData["Error"] = "Failed to create transaction.";
                return RedirectToPage("/Error");
            }

            var service = new PaymentIntentService();
            var intent = await service.CreateAsync(options);

            return new JsonResult(new
            {
                clientSecret = intent.ClientSecret,
                bookingId = dto.BookingId
            });
        }

        //SEPay
        public async Task<IActionResult> OnPostPayWithSEPayAsync()
        {
            var bookingResponse = await _bookingService.GetBookingById(BookingId);
            if (!bookingResponse.IsSuccess || bookingResponse.Result is not GetBookingDTO dto)
            {
                TempData["Error"] = "Booking not found.";
                return RedirectToPage("/Error");
            }

            var amount = dto.TotalPrice;

            var createTransactionDTO = new CreateTransactionDTO
            {
                BookingId = BookingId,
                Amount = amount,
                PaymentMethod = "VNPay",
                TransactionReference = "N/A"
            };

            var transactionResponse = await _transactionService.CreateTransactionAsync(createTransactionDTO);
            if (!transactionResponse.IsSuccess || transactionResponse.Result is not TransactionDTO transaction)
            {
                TempData["Error"] = "Failed to create transaction.";
                return RedirectToPage("/Error");
            }

            var qrUrl = await _sepayService.CreateSepayPaymentUrlAsync(transaction);
            TempData["Message"] = "Vui lòng quét mã QR để thanh toán.";
            return RedirectToPage("/Payment/SEPayQRCode", new
            {
                qrImageUrl = qrUrl,
                amount = dto.TotalPrice,
                transactionId = transaction.TransactionId
            });


        }

        public async Task<IActionResult> OnGetChatCustomersAsync()
        {
            var users = await LoadChatCustomersAsync();
            return new JsonResult(new { success = true, result = users });
        }


        private async Task<List<UserChatDTO>> LoadChatCustomersAsync()
        {
            var response = await _chatService.GetChatUsersAsync(CurrentUserId);

            if (response.IsSuccess && response.Result is List<UserChatDTO> users)
            {
                return users;
            }

            return new List<UserChatDTO>();
        }



        // ✅ Lấy tin nhắn giữa bạn và khách hàng
        public async Task<IActionResult> OnGetChatAsync(Guid customerId)
        {
            var sellerId = _userUtility.GetUserIDFromToken();

            var response = await _chatService.GetConversationAsync(sellerId, customerId);

            if (response.IsSuccess && response.Result is List<MessageDTO> messages)
            {
                ChatHistory = messages;
                return new JsonResult(new
                {
                    success = true,
                    result = ChatHistory
                });
            }

            return new JsonResult(new
            {
                success = false,
                message = response.Message
            });
        }
        public async Task<IActionResult> OnPostSendChatAsync()
        {
            var message = Request.Form["message"];
            var customerId = Request.Form["customerId"];
            var file = Request.Form.Files["image"];
            string imageUrl = null;

            if (string.IsNullOrEmpty(message) && (file == null || file.Length == 0))
            {
                TempData["Error"] = "Message or image is required.";
                return new JsonResult(new { success = false, error = "Message or image is required." });
            }

            if (string.IsNullOrEmpty(customerId))
            {
                TempData["Error"] = "Customer ID is missing.";
                return new JsonResult(new { success = false, error = "Customer ID is missing." });
            }

            if (file != null && file.Length > 0)
            {
                imageUrl = await _fileUploadService.UploadImageToFirebaseAsync(file);
            }

            var senderId = _userUtility.GetUserIDFromToken();
            var receiverId = Guid.Parse(customerId);

            var response = await _chatService.SendMessageAsync(senderId, receiverId, message, imageUrl);

            if (response.IsSuccess)
            {
                TempData["Message"] = "Message sent successfully.";
                return new JsonResult(new { success = true, imageUrl });
            }

            TempData["Error"] = "Failed to send message: " + response.Message;
            return new JsonResult(new { success = false, error = response.Message });
        }


        public class CreateIntentDTO
        {
            public Guid BookingId { get; set; }
        }
    }



}
