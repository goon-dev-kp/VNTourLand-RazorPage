using BLL.Hubs;
using BLL.Services.Implement;
using BLL.Services.Interface;
using BLL.Utilities;
using Common.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Stripe.Forwarding;

namespace VNTourLandProject.Pages.Manage
{
    public class ManageUserModel : PageModel
    {
        private readonly UserUtility _userUtility;
        private readonly IIUserService _userService;
        private readonly IChatService _chatService;
        private readonly IFileUploadService _fileUploadService;
        private readonly IHubContext<ChatHub> _hubContext;

        public ManageUserModel(
            IIUserService userService,
            UserUtility userUtility,
            IChatService chatService,
            IFileUploadService fileUploadService,
            IHubContext<ChatHub> hubContext)
        {
            _userService = userService;
            _userUtility = userUtility;
            _chatService = chatService;
            _fileUploadService = fileUploadService;
            _hubContext = hubContext;
        }

        // Auth
        public bool isAuth => _userUtility.IsAuthenticated();
        public Guid CurrentUserId => isAuth ? _userUtility.GetUserIDFromToken() : Guid.Empty;
        public string Role => isAuth ? _userUtility.GetRoleFromToken() : string.Empty;
        public string UserName => isAuth ? _userUtility.GetFullNameFromToken() : "Guest";


        // Data
        public List<UserDTO> Users { get; set; } = new();

        [BindProperty]
        public UserDTO EditUser { get; set; }

        // Chat
        public List<MessageDTO> ChatHistory { get; set; } = new();
        public List<UserChatDTO> ChatCustomers { get; set; } = new();

        // ✅ Main GET
        public async Task<IActionResult> OnGetAsync(Guid? editId)
        {
            if (!isAuth || Role != "MANAGER")
            {
                TempData["Message"] = "You must log in first.";
                return RedirectToPage("/Auth/Login");
            }

            var response = await _userService.GetAllSeller();
            if (!response.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, response.Message);
                return Page();
            }

            Users = response.Result as List<UserDTO>;

            if (editId.HasValue)
            {
                EditUser = Users.FirstOrDefault(u => u.UserId == editId.Value);
            }

            ChatCustomers = await LoadChatCustomersAsync();
            return Page();
        }

        // ✅ Cập nhật trạng thái
        public async Task<IActionResult> OnPostChangeStatusAsync(Guid userId, bool isActive)
        {
            await _userService.ChangeUserStatusAsync(userId, isActive);
            return RedirectToPage();
        }

        // ✅ Sửa thông tin người dùng
        public async Task<IActionResult> OnPostEditAsync()
        {
            if (!ModelState.IsValid)
            {
                Users = await _userService.GetAllUserAsync();
                return Page();
            }

            var result = await _userService.UpdateUserAsync(EditUser);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                Users = await _userService.GetAllUserAsync();
                return Page();
            }

            return RedirectToPage("/Manage/ManageUser");
        }

        // ✅ Xóa người dùng
        public async Task<IActionResult> OnPostDeleteAsync(Guid userId)
        {
            var result = await _userService.DeleteUserAsync(userId);

            TempData["Message"] = result.IsSuccess
                ? "User deleted successfully."
                : "Failed to delete user: " + result.Message;

            return RedirectToPage("/Manage/ManageUser");
        }

        // ✅ Tạo cuộc trò chuyện
        public async Task<IActionResult> OnPostCreateChatAsync(Guid customerId)
        {
            var response = await _chatService.SendMessageAsync(CurrentUserId, customerId, "Hello! 👋");

            if (response.IsSuccess)
            {
                TempData["Message"] = "Send chat successfully.";
                await _hubContext.Clients.User(customerId.ToString())
                    .SendAsync("ReceiveChatUser", CurrentUserId.ToString());
            }
            else
            {
                TempData["Message"] = "Can not create chat: " + response.Message;
            }

            return RedirectToPage("/Manage/ManageUser");
        }

        // ✅ Lấy danh sách khách hàng đã chat
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

        // ✅ Lấy tin nhắn giữa bạn và khách hàng
        public async Task<IActionResult> OnGetChatAsync(Guid customerId)
        {
            var response = await _chatService.GetConversationAsync(CurrentUserId, customerId);

            if (response.IsSuccess && response.Result is List<MessageDTO> messages)
            {
                ChatHistory = messages;
                return new JsonResult(new { success = true, result = ChatHistory });
            }

            return new JsonResult(new { success = false, message = response.Message });
        }

        // ✅ Gửi tin nhắn + ảnh (nếu có)
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

            var receiverId = Guid.Parse(customerId);
            var response = await _chatService.SendMessageAsync(CurrentUserId, receiverId, message, imageUrl);

            return response.IsSuccess
                ? new JsonResult(new { success = true, imageUrl })
                : new JsonResult(new { success = false, error = response.Message });
        }
    }

}
