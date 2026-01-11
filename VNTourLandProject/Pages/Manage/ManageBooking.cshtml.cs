using BLL.Services.Implement;
using BLL.Services.Interface;
using BLL.Utilities;
using Common.DTO;
using Common.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace VNTourLandProject.Pages.Manage
{
    public class ManageBookingModel : PageModel
    {
        private readonly UserUtility _userUtility;
        private readonly IBookingService _bookingService;
        private readonly IChatService _chatService;
        private readonly IFileUploadService _fileUploadService;

        public ManageBookingModel(UserUtility userUtility, IBookingService bookingService, IChatService chatService, IFileUploadService fileUploadService)
        {
            _userUtility = userUtility;
            _bookingService = bookingService;
            _chatService = chatService;
            _fileUploadService = fileUploadService;
        }

        // Properties
        public bool isAuth => _userUtility.IsAuthenticated();
        public Guid CurrentUserId => isAuth ? _userUtility.GetUserIDFromToken() : Guid.Empty;
        public string Role => isAuth ? _userUtility.GetRoleFromToken() : string.Empty;
        public string UserName => isAuth ? _userUtility.GetFullNameFromToken() : "Guest";


        public List<BookingManage> Bookings { get; set; } = new();
        public List<MessageDTO> ChatHistory { get; set; } = new();
        public List<UserChatDTO> ChatCustomers { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchCode { get; set; }

        [BindProperty(SupportsGet = true)]
        public string StatusFilter { get; set; }


        [BindProperty]
        public Guid BookingId { get; set; }
        // OnGet
        public async Task<IActionResult> OnGetAsync()
        {
            if (!isAuth || Role != "MANAGER")
            {
                TempData["Message"] = "You must log in first.";
                return RedirectToPage("/Auth/Login");
            }

            var response = await _bookingService.GetAllBookingsAsync();
            if (response.IsSuccess && response.Result is List<BookingManage> list)
            {
                Bookings = list;
            }
            else
            {
                TempData["Error"] = "Failed to load bookings.";
            }

            ChatCustomers = await LoadChatCustomersAsync();
            return Page();
        }

        // Chat customers
        public async Task<IActionResult> OnGetChatCustomersAsync()
        {
            var users = await LoadChatCustomersAsync();
            return new JsonResult(new { success = true, result = users });
        }

        private async Task<List<UserChatDTO>> LoadChatCustomersAsync()
        {
            var response = await _chatService.GetChatUsersAsync(CurrentUserId);
            return (response.IsSuccess && response.Result is List<UserChatDTO> users)
                ? users
                : new List<UserChatDTO>();
        }

        // Get Chat history
        public async Task<IActionResult> OnGetChatAsync(Guid customerId)
        {
            var sellerId = _userUtility.GetUserIDFromToken();
            var response = await _chatService.GetConversationAsync(sellerId, customerId);

            if (response.IsSuccess && response.Result is List<MessageDTO> messages)
            {
                ChatHistory = messages;
                return new JsonResult(new { success = true, result = ChatHistory });
            }

            return new JsonResult(new { success = false, message = response.Message });
        }

        // Send message
        public async Task<IActionResult> OnPostSendChatAsync()
        {
            var message = Request.Form["message"];
            var customerId = Request.Form["customerId"];
            var file = Request.Form.Files["image"];
            string imageUrl = null;

            if (string.IsNullOrEmpty(message) && (file == null || file.Length == 0))
                return new JsonResult(new { success = false, error = "Thiếu nội dung tin nhắn hoặc ảnh!" });

            if (string.IsNullOrEmpty(customerId))
                return new JsonResult(new { success = false, error = "Thiếu customerId!" });

            if (file != null && file.Length > 0)
            {
                imageUrl = await _fileUploadService.UploadImageToFirebaseAsync(file);
            }

            var senderId = _userUtility.GetUserIDFromToken();
            var receiverId = Guid.Parse(customerId);

            var response = await _chatService.SendMessageAsync(senderId, receiverId, message, imageUrl);

            if (response.IsSuccess)
                return new JsonResult(new { success = true, imageUrl });

            return new JsonResult(new { success = false, error = response.Message });
        }

        public async Task<IActionResult> OnPostChangeStatus()
        {
            var response = await _bookingService.ChangeStatusForManager(BookingId, BookingStatus.CONFIRMED);
           if (!response.IsSuccess)
            {
                TempData["Error"] = response.Message ?? "Failed to change booking status.";
                return RedirectToPage();
            }
            TempData["Message"] = "Booking status changed successfully.";

            // Có thể redirect lại chính trang đó
            return RedirectToPage();
        }

    }

}
