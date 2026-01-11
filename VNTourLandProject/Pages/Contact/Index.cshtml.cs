using BLL.Services.Implement;
using BLL.Services.Interface;
using BLL.Utilities;
using Common.DTO;
using Common.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace VNTourLandProject.Pages.Contact
{
    public class IndexModel : PageModel
    {
        private readonly UserUtility _userUtility;
        private readonly IChatService _chatService;
        private readonly IFileUploadService _fileUploadService;
        private readonly IContactService _contactService;
        private readonly IOptions<MapboxOptions> _mapboxOptions;
        public IndexModel(UserUtility userUtility, IChatService chatService, IFileUploadService fileUploadService, IContactService contactService, IOptions<MapboxOptions>options)
        {
            _userUtility = userUtility;
            _chatService = chatService;
            _fileUploadService = fileUploadService;
            _contactService = contactService;
            _mapboxOptions = options;
        }

        public bool isAuth => _userUtility.IsAuthenticated();
        public Guid CurrentUserId => isAuth ? _userUtility.GetUserIDFromToken() : Guid.Empty;
        public string Role => isAuth ? _userUtility.GetRoleFromToken() : string.Empty;
        public string UserName => isAuth ? _userUtility.GetFullNameFromToken() : "Guest";
        public string MapboxToken => _mapboxOptions.Value.AccessToken;

        //chat
        public List<MessageDTO> ChatHistory { get; set; } = new();
        public List<UserChatDTO> ChatCustomers { get; set; }

        [BindProperty]
        public CreateContactDTO ContactInput { get; set; }




        public async Task OnGetAsync()
        {
            ChatCustomers = await LoadChatCustomersAsync();
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

            Console.WriteLine("Gọi OnPostSendChatAsync");
            Console.WriteLine("message = " + message);
            Console.WriteLine("customerId = " + customerId);
            Console.WriteLine("image = " + file?.FileName);

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

        public async Task<IActionResult> OnPostSendContactAsync()
        {
            if (!ModelState.IsValid)
            {
                TempData["Message"] = "Please fill all required fields correctly.";
                return Page();
            }

            var result = await _contactService.CreateContact(ContactInput);
            TempData["Message"] = result.Message;

            if (result.IsSuccess)
            {
                ModelState.Clear();
            }

            return Page();
        }
    }
}
