using BLL.Services.Implement;
using BLL.Services.Interface;
using BLL.Utilities;
using Common.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Stripe.Terminal;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VNTourLandProject.Pages.Manage
{
    public class ManageStoryModel : PageModel
    {
        private readonly ILocationOfStoryService _locationOfStoryService;
        private readonly IStoryService _storyService;
        private readonly UserUtility _userUtility;
        private readonly IChatService _chatService;
        private readonly IFileUploadService _fileUploadService;

        public ManageStoryModel(ILocationOfStoryService locationOfStoryService, IStoryService storyService, UserUtility userUtility, IChatService chatService, IFileUploadService fileUploadService)
        {
            _locationOfStoryService = locationOfStoryService;
            _storyService = storyService;
            _userUtility = userUtility;
            _chatService = chatService;
            _fileUploadService = fileUploadService;
        }

        // Auth Info
        public bool isAuth => _userUtility.IsAuthenticated();
        public Guid CurrentUserId => isAuth ? _userUtility.GetUserIDFromToken() : Guid.Empty;
        public string Role => isAuth ? _userUtility.GetRoleFromToken() : string.Empty;
        public string UserName => isAuth ? _userUtility.GetFullNameFromToken() : "Guest";


        // Properties
        public List<LocationOfStoryDTO> Locations { get; set; } = new();
        public List<MessageDTO> ChatHistory { get; set; } = new();
        public List<UserChatDTO> ChatCustomers { get; set; } = new();

        [BindProperty] public StoryCreateDTO StoryCreate { get; set; }
        [BindProperty]
        public LocationOfStoryCreateDTO LocationCreate { get; set; }
        [BindProperty]
        public LocationOfStoryUpdateDTO LocationEdit { get; set; }

        [BindProperty]
        public StoryEditDTO StoryEdit { get; set; }


        // GET: Load Locations with Stories
        public async Task<IActionResult> OnGetAsync()
        {
            if (!isAuth || Role != "MANAGER")
            {
                TempData["Message"] = "You must log in first.";
                return RedirectToPage("/Auth/Login");
            }

            var response = await _locationOfStoryService.GetAllLocationsWithStoriesAsync();
            Locations = response.IsSuccess && response.Result is List<LocationOfStoryDTO> list ? list : new();

            if (!response.IsSuccess)
                TempData["Message"] = "Failed to retrieve locations.";

            ChatCustomers = await LoadChatCustomersAsync();
            return Page();
        }

        // POST: Create Story
        public async Task<IActionResult> OnPostCreateStoryAsync()
        {
            var response = await _storyService.CreateStoryAsync(StoryCreate);

            if (response.IsSuccess)
                return RedirectToPage("/Manage/ManageStory");

            ModelState.AddModelError(string.Empty, response.Message);
            return await ReloadPageWithDataAsync();
        }

        // POST: Create Location
        public async Task<IActionResult> OnPostCreateLocationAsync()
        {
            var response = await _locationOfStoryService.CreateLocationAsync(LocationCreate);

            if (response.IsSuccess)
                return RedirectToPage("/Manage/ManageStory");

            ModelState.AddModelError(string.Empty, response.Message);
            return await ReloadPageWithDataAsync();
        }

        // POST: Delete Location
        public async Task<IActionResult> OnPostDeleteLocationAsync(Guid locationId)
        {
            var response = await _locationOfStoryService.DeleteLocationAsync(locationId);

            if (response.IsSuccess)
                return RedirectToPage();

            ModelState.AddModelError(string.Empty, response.Message);
            return await ReloadPageWithDataAsync();
        }

        // POST: Delete Story
        public async Task<IActionResult> OnPostDeleteStoryAsync(Guid storyId)
        {
            var response = await _storyService.DeleteStoryAsync(storyId);

            if (response.IsSuccess)
                return RedirectToPage();

            ModelState.AddModelError(string.Empty, response.Message);
            return await ReloadPageWithDataAsync();
        }

        // Reload page and chat data
        private async Task<IActionResult> ReloadPageWithDataAsync()
        {
            await OnGetAsync();
            return Page();
        }

        // GET: Chat users
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

        // GET: Load conversation
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

        // POST: Send chat message
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

        public async Task<IActionResult> OnPostEditLocationAsync()
        {
         

            var response = await _locationOfStoryService.UpdateLocationAsync(LocationEdit);
            if (response.IsSuccess)
            {
                TempData["Message"] = "Location updated successfully.";
            }
            else
            {
                TempData["Message"] = "Failed to update location.";
            }

            return RedirectToPage(); // or RedirectToPage("CurrentPage");
        }

        public async Task<IActionResult> OnPostUpdateStoryAsync()
        {
            if (!ModelState.IsValid) return Page();

            var result = await _storyService.UpdateStoryAsync(StoryEdit);
            if (!result.IsSuccess) ModelState.AddModelError("", result.Message);

            return RedirectToPage(); // or stay on page
        }

    }

}
