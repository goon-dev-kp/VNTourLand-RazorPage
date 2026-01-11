using BLL.Services.Implement;
using BLL.Services.Interface;
using BLL.Utilities;
using Common.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VNTourLandProject.Pages.Manage
{
    public class ManageTransactionModel : PageModel
    {
        private readonly UserUtility _userUtility;
        private readonly ITransactionService _transactionService;
        private readonly IChatService _chatService;
        private readonly IFileUploadService _fileUploadService;

        public ManageTransactionModel(
            UserUtility userUtility,
            ITransactionService transactionService,
            IChatService chatService,
            IFileUploadService fileUploadService)
        {
            _userUtility = userUtility;
            _transactionService = transactionService;
            _chatService = chatService;
            _fileUploadService = fileUploadService;
        }

        // Auth
        public bool isAuth => _userUtility.IsAuthenticated();
        public Guid CurrentUserId => isAuth ? _userUtility.GetUserIDFromToken() : Guid.Empty;
        public string Role => isAuth ? _userUtility.GetRoleFromToken() : string.Empty;
        public string UserName => isAuth ? _userUtility.GetFullNameFromToken() : "Guest";


        // Data
        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        public List<TransactionManage> Transactions { get; set; } = new();

        // Chat
        public List<MessageDTO> ChatHistory { get; set; } = new();
        public List<UserChatDTO> ChatCustomers { get; set; } = new();

        // ✅ Main GET
        public async Task<IActionResult> OnGetAsync()
        {
            if (!isAuth || Role != "MANAGER")
            {
                TempData["Message"] = "You must log in first.";
                return RedirectToPage("/Auth/Login");
            }

            var response = await _transactionService.GetAllTransactionsAsync();
            ChatCustomers = await LoadChatCustomersAsync();

            if (response.IsSuccess && response.Result is List<TransactionManage> allTransactions)
            {
                Transactions = FilterTransactions(allTransactions);
            }
            else
            {
                TempData["Message"] = "Failed to retrieve transactions.";
                Transactions = new();
            }

            return Page();
        }

        // ✅ Search filter logic
        private List<TransactionManage> FilterTransactions(List<TransactionManage> allTransactions)
        {
            if (string.IsNullOrWhiteSpace(SearchTerm))
                return allTransactions;

            return allTransactions
                .Where(t =>
                    (!string.IsNullOrEmpty(t.FullName) && t.FullName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(t.Email) && t.Email.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        // ✅ Chat: Get chat users
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

        // ✅ Get chat messages between manager & customer
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

        // ✅ Send message with optional image
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

            var senderId = CurrentUserId;
            var receiverId = Guid.Parse(customerId);

            var response = await _chatService.SendMessageAsync(senderId, receiverId, message, imageUrl);

            return response.IsSuccess
                ? new JsonResult(new { success = true, imageUrl })
                : new JsonResult(new { success = false, error = response.Message });
        }
    }

}
