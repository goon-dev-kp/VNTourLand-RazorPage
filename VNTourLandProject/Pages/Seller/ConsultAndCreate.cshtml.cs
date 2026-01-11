using BLL.Services.Implement;
using BLL.Services.Interface;
using BLL.Utilities;
using Common.DTO;
using Common.Settings;
using Common.Validates;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Stripe.Forwarding;

namespace VNTourLandProject.Pages.Seller
{
    public class ConsultAndCreateModel : PageModel
    {
        private readonly IChatService _chatService;
        private readonly UserUtility _userUtility;
        private readonly ITourService _tourService;
        private readonly IRequestOfCustomerService _requestOfCustomerService;
        private readonly IFileUploadService _fileUploadService;
        private readonly IOptions<MapboxOptions> _mapboxOptions;

        public ConsultAndCreateModel(
            IChatService chatService,
            UserUtility userUtility,
            ITourService tourService,
            IRequestOfCustomerService requestOfCustomerService,
            IFileUploadService fileUploadService,
            IOptions<MapboxOptions> options)
        {
            _chatService = chatService;
            _userUtility = userUtility;
            _tourService = tourService;
            _requestOfCustomerService = requestOfCustomerService;
            _fileUploadService = fileUploadService;
            _mapboxOptions = options;
        }

        // ✅ Auth
        public bool isAuth => _userUtility.IsAuthenticated();
        public Guid CurrentUserId => isAuth ? _userUtility.GetUserIDFromToken() : Guid.Empty;
        public string Role => isAuth ? _userUtility.GetRoleFromToken() : string.Empty;
        public string UserName => isAuth ? _userUtility.GetFullNameFromToken() : "Guest";


        // ✅ Tour Creation
        [BindProperty] public CreateTourSellerRequest TourRequest { get; set; }

        // ✅ Chat
        public List<UserChatDTO> ChatCustomers { get; set; } = new();
        public List<MessageDTO> ChatHistory { get; set; } = new();

        // ✅ Mapbox
        public string MapboxToken => _mapboxOptions.Value.AccessToken;

        // ✅ Bind từ URL
        [BindProperty(SupportsGet = true)] public string CustomerId { get; set; }
        [BindProperty(SupportsGet = true)] public Guid RequestId { get; set; }

        // ✅ OnGet
        public async Task<IActionResult> OnGetAsync()
        {
            if (!isAuth || Role != "SELLER")
            {
                TempData["Message"] = "You must log in first.";
                return RedirectToPage("/Auth/Login");
            }

            ChatCustomers = await LoadChatCustomersAsync();
            return Page();
        }

        // ✅ POST tạo tour
        public async Task<IActionResult> OnPostAsync()
        {
            //// ✅ Validate start and end dates
            //var validationMessage = DateValidator.ValidateStartEnd(TourRequest.StartDate, TourRequest.EndDate);
            //if (!string.IsNullOrEmpty(validationMessage))
            //{
            //    ModelState.AddModelError(string.Empty, validationMessage);
            //    TempData["Error"] = validationMessage;
            //    return Page();
            //}

            var response = await _tourService.CreateTourSellerAsync(TourRequest);

            if (response.IsSuccess)
            {
                TempData["Message"] = "Tour created successfully!";

                var changeStatusResponse = await _requestOfCustomerService.ChangeStatusAsync(
                    TourRequest.RequestId,
                    Common.Enums.RequestStatus.WAITING_PAYMENT
                );

                if (!changeStatusResponse.IsSuccess)
                {
                    TempData["Message"] += " However, failed to update request status: " + changeStatusResponse.Message;
                }

                return RedirectToPage("/Seller/CustomerRequests");
            }

            ModelState.AddModelError(string.Empty, response.Message);
            return Page();
        }



        // ✅ Chat Users
        public async Task<IActionResult> OnGetChatCustomersAsync()
        {
            var users = await LoadChatCustomersAsync();
            return new JsonResult(new { success = true, result = users });
        }

        private async Task<List<UserChatDTO>> LoadChatCustomersAsync()
        {
            var response = await _chatService.GetChatUsersAsync(CurrentUserId);
            return response.IsSuccess && response.Result is List<UserChatDTO> users ? users : new();
        }

        // ✅ Lấy tin nhắn giữa bạn và khách
        public async Task<IActionResult> OnGetChatAsync(Guid customerId)
        {
            var response = await _chatService.GetConversationAsync(CurrentUserId, customerId);

            return response.IsSuccess && response.Result is List<MessageDTO> messages
                ? new JsonResult(new { success = true, result = messages })
                : new JsonResult(new { success = false, message = response.Message });
        }

        // ✅ Gửi tin nhắn
        public async Task<IActionResult> OnPostSendChatAsync()
        {
            var message = Request.Form["message"];
            var customerId = Request.Form["customerId"];
            var file = Request.Form.Files["image"];
            string imageUrl = null;

            Console.WriteLine("📨 OnPostSendChatAsync");
            Console.WriteLine($"Message: {message}");
            Console.WriteLine($"CustomerId: {customerId}");
            Console.WriteLine($"Image: {file?.FileName}");

            if (string.IsNullOrEmpty(message) && (file == null || file.Length == 0))
                return new JsonResult(new { success = false, error = "Thiếu nội dung tin nhắn hoặc ảnh!" });

            if (string.IsNullOrEmpty(customerId))
                return new JsonResult(new { success = false, error = "Thiếu customerId!" });

            if (file != null && file.Length > 0)
            {
                imageUrl = await _fileUploadService.UploadImageToFirebaseAsync(file);
            }

            var receiverId = Guid.Parse(customerId);
            var response = await _chatService.SendMessageAsync(CurrentUserId, receiverId, message, imageUrl);

            return response.IsSuccess
                ? new JsonResult(new { success = true, imageUrl })
                : new JsonResult(new { success = false, error = response.Message });
        }
    }



}
