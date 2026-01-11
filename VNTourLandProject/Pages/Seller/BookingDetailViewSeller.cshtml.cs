using BLL.Services.Implement;
using BLL.Services.Interface;
using BLL.Utilities;
using Common.DTO;
using DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VNTourLandProject.Pages.Seller
{
    public class BookingDetailViewSellerModel : PageModel
    {
        private readonly IChatService _chatService;
        private readonly UserUtility _userUtility;
        private readonly IFileUploadService _fileUploadService;
        private readonly IBookingService _bookingService;

        public BookingDetailViewSellerModel(
            IChatService chatService,
            UserUtility userUtility,
            IFileUploadService fileUploadService,
            IBookingService bookingService)
        {
            _chatService = chatService;
            _userUtility = userUtility;
            _fileUploadService = fileUploadService;
            _bookingService = bookingService;
        }

        // ✅ Auth
        public bool isAuth => _userUtility.IsAuthenticated();
        public Guid CurrentUserId => isAuth ? _userUtility.GetUserIDFromToken() : Guid.Empty;
        public string Role => isAuth ? _userUtility.GetRoleFromToken() : string.Empty;
        public string UserName => isAuth ? _userUtility.GetFullNameFromToken() : "Guest";


        // ✅ Chat
        public List<MessageDTO> ChatHistory { get; set; } = new();
        public List<UserChatDTO> ChatCustomers { get; set; } = new();

        //
        [BindProperty(SupportsGet = true)]
        public Guid BookingId { get; set; } 

        public BookingDetailsDTO Booking { get; set; } = new();

        // ✅ GET page
        public async Task<IActionResult> OnGetAsync()
        {
            if (!isAuth || Role != "SELLER")
            {
                TempData["Message"] = "You must log in first.";
                return RedirectToPage("/Auth/Login");
            }



            ChatCustomers = await LoadChatCustomersAsync();

            var response = await _bookingService.GetBookingDetailsForSellerAsync(BookingId);

            if (!response.IsSuccess)
            {
                TempData["Message"] = response.Message;
                return RedirectToPage("/Error"); // hoặc xử lý khác
            }

            Booking = response.Result as BookingDetailsDTO;

            if (Booking == null)
            {
                TempData["Message"] = "Invalid booking data";
                return RedirectToPage("/Error");
            }

  

            return Page();
        }

        // ✅ GET danh sách người đã chat
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

        // ✅ GET tin nhắn giữa Seller và Customer
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

            Console.WriteLine("📩 OnPostSendChatAsync called");
            Console.WriteLine($"Message: {message}");
            Console.WriteLine($"CustomerId: {customerId}");
            Console.WriteLine($"Image: {file?.FileName}");

            if (string.IsNullOrEmpty(message) && (file == null || file.Length == 0))
                return new JsonResult(new { success = false, error = "Missing message or image!" });

            if (string.IsNullOrEmpty(customerId))
                return new JsonResult(new { success = false, error = "Missing customerId!" });

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
