using BLL.Services.Implement;
using BLL.Services.Interface;
using BLL.Utilities;
using Common.DTO;
using DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace VNTourLandProject.Pages.Manage
{
    public class CreateBlogModel : PageModel
    {
        private readonly UserUtility _userUtility;
        private readonly IBlogCategoryService _blogCategoryService;
        private readonly IBlogService _blogService;
        private readonly IChatService _chatService;
        private readonly IFileUploadService _fileUploadService;

        public CreateBlogModel(UserUtility userUtility, IBlogCategoryService blogCategoryService, IBlogService blogService, IChatService chatService, IFileUploadService fileUploadService)
        {
            _userUtility = userUtility;
            _blogCategoryService = blogCategoryService;
            _blogService = blogService;
            _chatService = chatService;
            _fileUploadService = fileUploadService;
        }

        [BindProperty]
        public CreateBlogDTO CreateBlogDTO { get; set; }
        public List<CategoryDTO> Categories { get; set; }

        public bool isAuth => _userUtility.IsAuthenticated();
        public Guid CurrentUserId => isAuth ? _userUtility.GetUserIDFromToken() : Guid.Empty;
        public string Role => isAuth ? _userUtility.GetRoleFromToken() : string.Empty;
        public string UserName => isAuth ? _userUtility.GetFullNameFromToken() : "Guest";


        public List<MessageDTO> ChatHistory { get; set; } = new();
        public List<UserChatDTO> ChatCustomers { get; set; }



        public async Task<IActionResult> OnGetAsync()
        {
            if (!isAuth || Role != "MANAGER")
            {
                TempData["Error"] = "You must be logged in as manager to access this page.";
                return RedirectToPage("/Auth/Login");
            }

            var listCategory = await _blogCategoryService.GetAllCategoryAsync();
            if (listCategory.IsSuccess && listCategory.Result != null)
            {
                Categories = listCategory.Result as List<CategoryDTO>;
                ChatCustomers = await LoadChatCustomersAsync();
            }
            else
            {
                TempData["Error"] = "Failed to retrieve the list of categories.";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var response = await _blogService.CreateBlog(CreateBlogDTO);

            if (response.IsSuccess)
            {
                TempData["Message"] = "Blog created successfully!";
                return RedirectToPage("/Manage/ManageBlog");
            }

            TempData["Error"] = "Failed to create blog: " + response.Message;
            ModelState.AddModelError(string.Empty, response.Message);
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

            TempData["Error"] = "Failed to load chat customers.";
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
    }

}
