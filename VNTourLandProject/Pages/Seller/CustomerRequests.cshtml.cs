using BLL.Hubs;
using BLL.Services.Implement;
using BLL.Services.Interface;
using BLL.Utilities;
using Common.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;

namespace VNTourLandProject.Pages.Seller
{
    public class CustomerRequestsModel : PageModel
    {
        private readonly IRequestOfCustomerService _requestService;
        private readonly UserUtility _userUtility;
        private readonly IChatService _chatService;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IFileUploadService _fileUploadService;

        public CustomerRequestsModel(IRequestOfCustomerService requestService, UserUtility userUtility, IChatService chatService, IHubContext<ChatHub> hubContext, IFileUploadService fileUploadService)
        {
            _requestService = requestService;
            _userUtility = userUtility;
            _chatService = chatService;
            _hubContext = hubContext;
            _fileUploadService = fileUploadService;
        }

        public bool isAuth => _userUtility.IsAuthenticated();
        public Guid CurrentUserId => isAuth ? _userUtility.GetUserIDFromToken() : Guid.Empty;
        public string Role => isAuth ? _userUtility.GetRoleFromToken() : string.Empty;
        public string UserName => isAuth ? _userUtility.GetFullNameFromToken() : "Guest";

        public List<MessageDTO> ChatHistory { get; set; } = new();
        public List<UserChatDTO> ChatCustomers { get; set; }

        [BindProperty]
        public Guid RequestId { get; set; }




        public List<RequestOfCustomerWithNameDTO> Requests { get; set; } = new();

        private async Task LoadAllRequestsAsync()
        {
            var response = await _requestService.GetAllRequestsAsync();

            if (response.IsSuccess && response.Result is List<RequestOfCustomerWithNameDTO> list)
            {
                Requests = list;
            }
            else
            {
                Requests = new List<RequestOfCustomerWithNameDTO>();
                TempData["Message"] = "Failed to retrieve customer requests.";
            }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!isAuth || Role != "SELLER")
            {
                TempData["Message"] = "Please log in as a seller to continue.";
                return RedirectToPage("/Auth/Login");
            }

            ChatCustomers = await LoadChatCustomersAsync();

            await LoadAllRequestsAsync();
            return Page();
        }



        public async Task<IActionResult> OnPostCreateChatAsync(Guid customerId, Guid requestId)
        {
            var response = await _chatService.SendMessageAsync(CurrentUserId, customerId,
                "Hello! 👋 I saw your request for a custom tour. I'm here to help you plan it. May I ask a few questions to better understand your preferences?");

            if (response.IsSuccess)
            {
                TempData["Message"] = "Chat conversation successfully created.";

                await _hubContext.Clients.User(customerId.ToString())
                    .SendAsync("ReceiveChatUser", CurrentUserId.ToString());

                var changeStatusResponse = await _requestService.ChangeStatusAsync(requestId, Common.Enums.RequestStatus.IN_PROGRESS);

                if (!changeStatusResponse.IsSuccess)
                {
                    TempData["Message"] += " However, failed to update the request status: " + changeStatusResponse.Message;
                }

                Console.WriteLine("📤 [SignalR] Sent ReceiveChatUser to customerId: " + customerId);
            }
            else
            {
                TempData["Message"] = "Failed to create chat conversation: " + response.Message;
            }

            await LoadAllRequestsAsync();
            return Page();
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

            Console.WriteLine("Call OnPostSendChatAsync");
            Console.WriteLine("message = " + message);
            Console.WriteLine("customerId = " + customerId);
            Console.WriteLine("image = " + file?.FileName);

            if (string.IsNullOrEmpty(message) && (file == null || file.Length == 0))
                return new JsonResult(new { success = false, error = "Message content or image is required." });

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
                return new JsonResult(new { success = true, imageUrl });

            return new JsonResult(new { success = false, error = response.Message });
        }

        public async Task<IActionResult> OnPostChangeStatus()
        {
            var response = await _requestService.ChangeStatusAsync(Guid.Parse(Request.Form["requestId"]), Common.Enums.RequestStatus.COMPLETED);
            if (response.IsSuccess)
            {
                TempData["Message"] = "Request status updated successfully.";
            }
            else
            {
                TempData["Message"] = "Failed to update request status: " + response.Message;
            }

            await LoadAllRequestsAsync();
            return RedirectToPage();
        }
    }
}
