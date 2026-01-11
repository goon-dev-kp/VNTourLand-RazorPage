using BLL.Hubs;
using BLL.Services.Interface;
using BLL.Utilities;
using Common.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;

namespace VNTourLandProject.Pages.Admin
{
    public class ManageUserOfAdminModel : PageModel
    {
        private readonly UserUtility _userUtility;
        private readonly IIUserService _userService;
        private readonly IChatService _chatService;
        private readonly IFileUploadService _fileUploadService;
        private readonly IHubContext<ChatHub> _hubContext;

        public List<UserDTO> Users { get; set; }
        [BindProperty]
        public UserDTO EditUser { get; set; }

        public ManageUserOfAdminModel(IIUserService userService, UserUtility userUtility, IChatService chatService, IFileUploadService fileUploadService, IHubContext<ChatHub> hubContext)
        {
            _userService = userService;
            _userUtility = userUtility;
            _chatService = chatService;
            _fileUploadService = fileUploadService;
            _hubContext = hubContext;
        }

        public bool isAuth => _userUtility.IsAuthenticated();
        public Guid CurrentUserId => isAuth ? _userUtility.GetUserIDFromToken() : Guid.Empty;
        public string Role => isAuth ? _userUtility.GetRoleFromToken() : string.Empty;
        public string UserName => isAuth ? _userUtility.GetFullNameFromToken() : "Guest";


        public List<MessageDTO> ChatHistory { get; set; } = new();
        public List<UserChatDTO> ChatCustomers { get; set; }

        

        public async Task<IActionResult> OnGetAsync(Guid? editId)
        {
            if (!isAuth || Role != "ADMIN")
            {
                TempData["Error"] = "You must log in as admin to access this page.";
                return RedirectToPage("/Auth/Login");
            }

            var response = await _userService.GetAllManagerAndSeller();
            if (!response.IsSuccess)
            {
                TempData["Error"] = "Failed to load user list: " + response.Message;
                ModelState.AddModelError(string.Empty, response.Message);
                return Page();
            }

            Users = response.Result as List<UserDTO>;

            if (editId.HasValue)
            {
                EditUser = Users.Find(u => u.UserId == editId.Value);
            }

            ChatCustomers = await LoadChatCustomersAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostChangeStatusAsync(Guid userId, bool isActive)
        {
            await _userService.ChangeUserStatusAsync(userId, isActive);
            TempData["Message"] = isActive ? "User activated successfully." : "User deactivated.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAsync()
        {
            if (!ModelState.IsValid)
            {
                Users = await _userService.GetAllUserAsync();
                TempData["Error"] = "Please correct the highlighted errors before submitting.";
                return Page();
            }

            var result = await _userService.UpdateUserAsync(EditUser);
            if (!result.Success)
            {
                TempData["Error"] = "Failed to update user: " + result.Message;
                ModelState.AddModelError(string.Empty, result.Message);
                Users = await _userService.GetAllUserAsync();
                return Page();
            }

            TempData["Message"] = "User updated successfully.";
            return RedirectToPage();
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

            TempData["Error"] = "Failed to load chat history.";
            return new JsonResult(new { success = false, message = response.Message });
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

            TempData["Error"] = "Failed to send message.";
            return new JsonResult(new { success = false, error = response.Message });
        }

        public async Task<IActionResult> OnPostCreateChatAsync(Guid customerId, Guid requestId)
        {
            var response = await _chatService.SendMessageAsync(CurrentUserId, customerId, "Hello! 👋");

            if (response.IsSuccess)
            {
                TempData["Message"] = "Chat session created successfully.";
                await _hubContext.Clients.User(customerId.ToString())
                    .SendAsync("ReceiveChatUser", CurrentUserId.ToString());
            }
            else
            {
                TempData["Error"] = "Failed to create chat: " + response.Message;
            }

            return RedirectToPage("/Admin/ManageUserOfAdmin");
        }
    }

}
