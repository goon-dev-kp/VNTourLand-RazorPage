using BLL.Services.Implement;
using BLL.Services.Interface;
using BLL.Utilities;
using Common.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VNTourLandProject.Pages.RequestOfCustomer
{
    public class CreateRequestModel : PageModel
    {
        private readonly IRequestOfCustomerService _requestService;
        private readonly UserUtility _userUtility;
        private readonly IEmailService _emailService;
        private readonly IChatService _chatService;
        private readonly IFileUploadService _fileUploadService;

        public CreateRequestModel(
            IRequestOfCustomerService requestService,
            UserUtility userUtility,
            IEmailService emailService,
            IChatService chatService,
            IFileUploadService fileUploadService)
        {
            _requestService = requestService;
            _userUtility = userUtility;
            _emailService = emailService;
            _chatService = chatService;
            _fileUploadService = fileUploadService;
        }

        // ✅ Auth
        public bool isAuth => _userUtility.IsAuthenticated();
        public Guid CurrentUserId => isAuth ? _userUtility.GetUserIDFromToken() : Guid.Empty;
        public string Role => isAuth ? _userUtility.GetRoleFromToken() : string.Empty;
        public string UserName => isAuth ? _userUtility.GetFullNameFromToken() : "Guest";


        // ✅ Dữ liệu chính
        [BindProperty]
        public RequestOfCustomerDTO RequestOfCustomer { get; set; }

        // ✅ Chat
        public List<MessageDTO> ChatHistory { get; set; } = new();
        public List<UserChatDTO> ChatCustomers { get; set; } = new();

        // ✅ GET hiển thị trang
        public async Task<IActionResult> OnGet()
        {
            if (!isAuth)
            {
                TempData["Message"] = "You must log in first.";
                return RedirectToPage("/Auth/Login");
            }

            ChatCustomers = await LoadChatCustomersAsync();
            return Page();
        }

        // ✅ POST tạo request
        public async Task<IActionResult> OnPostAsync()
        {
            var errorMessage = Common.Validates.DateValidator.ValidateStartEnd(RequestOfCustomer.StartDate, RequestOfCustomer.EndDate);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                TempData["Error"] = errorMessage;
                return Page();
            }

            if (CurrentUserId == Guid.Empty)
            {
                TempData["Message"] = "User is not logged in.";
                return Page();
            }

            var response = await _requestService.CreateRequestOfCustomerAsync(RequestOfCustomer, CurrentUserId);

            if (response.IsSuccess && response.Result is string userName)
            {
                await _emailService.SendEmailRequestReceivedAsync(RequestOfCustomer.Email, userName);
                TempData["Message"] = "Request created successfully!";
                return RedirectToPage("/TravelInfo/Index");
            }

            TempData["Error"] = "Failed to create request.";
            return Page();
        }

        // ✅ Load danh sách người đã chat
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

        // ✅ GET lịch sử chat giữa bạn và khách
        public async Task<IActionResult> OnGetChatAsync(Guid customerId)
        {
            var response = await _chatService.GetConversationAsync(CurrentUserId, customerId);

            return response.IsSuccess && response.Result is List<MessageDTO> messages
                ? new JsonResult(new { success = true, result = messages })
                : new JsonResult(new { success = false, message = response.Message });
        }

        // ✅ POST gửi tin nhắn
        public async Task<IActionResult> OnPostSendChatAsync()
        {
            var message = Request.Form["message"];
            var customerId = Request.Form["customerId"];
            var file = Request.Form.Files["image"];
            string imageUrl = null;

            if (string.IsNullOrEmpty(message) && (file == null || file.Length == 0))
                return new JsonResult(new { success = false, error = "Missing message or image!" });

            if (string.IsNullOrEmpty(customerId))
                return new JsonResult(new { success = false, error = "Missing customerId!" });

            if (file != null && file.Length > 0)
                imageUrl = await _fileUploadService.UploadImageToFirebaseAsync(file);

            var receiverId = Guid.Parse(customerId);
            var response = await _chatService.SendMessageAsync(CurrentUserId, receiverId, message, imageUrl);

            return response.IsSuccess
                ? new JsonResult(new { success = true, imageUrl })
                : new JsonResult(new { success = false, error = response.Message });
        }
    }

}