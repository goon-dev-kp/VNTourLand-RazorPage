using BLL.Services.Interface;
using BLL.Utilities;
using Common.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static Common.DTO.AuthDTO;

namespace VNTourLandProject.Pages.Manage
{
    public class CreateUserModel : PageModel
    {
        private readonly UserUtility _userUtility;
        private readonly IChatService _chatService;
        private readonly IFileUploadService _fileUploadService;
        private readonly IIUserService _userService;
        private readonly IAuthService _authService;

        public CreateUserModel(UserUtility userUtility, IChatService chatService, IFileUploadService fileUploadService, IIUserService userService, IAuthService authService)
        {
            _userUtility = userUtility;
            _chatService = chatService;
            _fileUploadService = fileUploadService;
            _userService = userService;
            _authService = authService;
        }

        [BindProperty]
        public RegisterForBoss RegisterForBoss { get; set; }

        public bool isAuth => _userUtility.IsAuthenticated();
        public Guid CurrentUserId => isAuth ? _userUtility.GetUserIDFromToken() : Guid.Empty;
        public string Role => isAuth ? _userUtility.GetRoleFromToken() : string.Empty;
        public string UserName => isAuth ? _userUtility.GetFullNameFromToken() : "Guest";


        public List<MessageDTO> ChatHistory { get; set; } = new();
        public List<UserChatDTO> ChatCustomers { get; set; }



        public async Task<IActionResult> OnGet()
        {
            if (!isAuth || Role != "MANAGER")
            {
                TempData["Error"] = "You must be logged in as a manager to access this page.";
                return RedirectToPage("/Auth/Login");
            }

            ChatCustomers = await LoadChatCustomersAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostCreateSellerAsync()
        {
            var response = await _authService.RegisterSeller(RegisterForBoss);

            if (!response.IsSuccess)
            {
                TempData["Error"] = "Failed to create seller: " + response.Message;
                ModelState.AddModelError(string.Empty, response.Message);
                return Page();
            }

            TempData["Message"] = "Seller created successfully.";
            return RedirectToPage("/Manage/ManageUser");
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

            TempData["Error"] = "Failed to load chat users.";
            return new List<UserChatDTO>();
        }

        public async Task<IActionResult> OnGetChatAsync(Guid customerId)
        {
            var sellerId = _userUtility.GetUserIDFromToken();
            var response = await _chatService.GetConversationAsync(sellerId, customerId);

            if (response.IsSuccess && response.Result is List<MessageDTO> messages)
            {
                ChatHistory = messages;
                return new JsonResult(new { success = true, result = ChatHistory });
            }

            TempData["Error"] = "Failed to load chat messages.";
            return new JsonResult(new { success = false, message = response.Message });
        }

        public async Task<IActionResult> OnPostSendChatAsync()
        {
            var message = Request.Form["message"];
            var customerId = Request.Form["customerId"];
            var file = Request.Form.Files["image"];
            string imageUrl = null;

            if (string.IsNullOrEmpty(message) && (file == null || file.Length == 0))
                return new JsonResult(new { success = false, error = "Message or image is required." });

            if (string.IsNullOrEmpty(customerId))
                return new JsonResult(new { success = false, error = "Customer ID is missing." });

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

            TempData["Error"] = "Failed to send message.";
            return new JsonResult(new { success = false, error = response.Message });
        }
    }

}
