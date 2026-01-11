using BLL.Services.Implement;
using BLL.Services.Interface;
using BLL.Utilities;
using Common.DTO;
using DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VNTourLandProject.Pages.Tour_Service
{
    public class IndexModel : PageModel
    {
        private readonly UserUtility _userUtility;
        private readonly ITourService _tourService;
        private readonly IChatService _chatService;
        private readonly IFileUploadService _fileUploadService;
        private readonly IStoryService _storyService;

        public IndexModel(UserUtility userUtility, ITourService tourService, IChatService chatService, IFileUploadService fileUploadService, IStoryService storyService)
        {
            _userUtility = userUtility;
            _tourService = tourService;
            _chatService = chatService;
            _fileUploadService = fileUploadService;
            _storyService = storyService;
        }

        public bool isAuth => _userUtility.IsAuthenticated();
        public Guid CurrentUserId => isAuth ? _userUtility.GetUserIDFromToken() : Guid.Empty;
        public string Role => isAuth ? _userUtility.GetRoleFromToken() : string.Empty;
        public string UserName => isAuth ? _userUtility.GetFullNameFromToken() : "Guest";

        public List<TourWithStoryDTO> TourWithStories { get; set; } = new();


        public List<MessageDTO> ChatHistory { get; set; } = new();
        public List<UserChatDTO> ChatCustomers { get; set; } = new();
        public List<TourDTO> AllTour { get; set; } = new();

      


        public async Task OnGetAsync()
        {
            try
            {
                ChatCustomers = await LoadChatCustomersAsync();

                var response = await _tourService.GetAllTour();
                if (response.IsSuccess && response.Result is List<TourDTO> allTours)
                {
                    foreach (var tour in allTours)
                    {
                        var stories = new List<StoryDTO>();

                        var locationNames = tour.Location.ToList();



                        foreach (var locationName in locationNames)
                        {
                            var storyResponse = await _storyService.GetStoriesByLocationNameAsync(locationName);
                            if (storyResponse.IsSuccess)
                            {
                                var locationStories = storyResponse.Result as List<StoryDTO>;
                                if (locationStories != null)
                                {
                                    stories.AddRange(locationStories);
                                }
                            }
                        }

                        TourWithStories.Add(new TourWithStoryDTO
                        {
                            Tour = tour,
                            Stories = stories.Any() ? stories.DistinctBy(s => s.StoryId).ToList() : new List<StoryDTO>()
                        });
                    }
                }

                else
                {
                    TempData["Message"] = "Failed to load tour list.";
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = "An error occurred while loading data: " + ex.Message;
            }
        }

        public async Task<IActionResult> OnGetChatCustomersAsync()
        {
            try
            {
                var users = await LoadChatCustomersAsync();
                if (users.Any())
                    return new JsonResult(new { success = true, result = users });

                return new JsonResult(new { success = false, error = "No chat users found." });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, error = "Failed to load chat users: " + ex.Message });
            }
        }

        private async Task<List<UserChatDTO>> LoadChatCustomersAsync()
        {
            var response = await _chatService.GetChatUsersAsync(CurrentUserId);

            if (response.IsSuccess && response.Result is List<UserChatDTO> users)
            {
                return users;
            }

            TempData["Message"] = "Failed to load chat users.";
            return new List<UserChatDTO>();
        }

        public async Task<IActionResult> OnGetChatAsync(Guid customerId)
        {
            try
            {
                var sellerId = _userUtility.GetUserIDFromToken();
                var response = await _chatService.GetConversationAsync(sellerId, customerId);

                if (response.IsSuccess && response.Result is List<MessageDTO> messages)
                {
                    ChatHistory = messages;
                    return new JsonResult(new { success = true, result = ChatHistory });
                }

                return new JsonResult(new { success = false, error = response.Message ?? "Failed to load chat history." });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, error = "An error occurred: " + ex.Message });
            }
        }

        public async Task<IActionResult> OnPostSendChatAsync()
        {
            var message = Request.Form["message"];
            var customerId = Request.Form["customerId"];
            var file = Request.Form.Files["image"];
            string imageUrl = null;

            if (string.IsNullOrEmpty(message) && (file == null || file.Length == 0))
                return new JsonResult(new { success = false, error = "Message content or image is required." });

            if (string.IsNullOrEmpty(customerId))
                return new JsonResult(new { success = false, error = "Customer ID is required." });

            try
            {
                if (file != null && file.Length > 0)
                {
                    imageUrl = await _fileUploadService.UploadImageToFirebaseAsync(file);
                }

                var senderId = _userUtility.GetUserIDFromToken();
                var receiverId = Guid.Parse(customerId);

                var response = await _chatService.SendMessageAsync(senderId, receiverId, message, imageUrl);

                if (response.IsSuccess)
                    return new JsonResult(new { success = true, imageUrl });

                return new JsonResult(new { success = false, error = response.Message ?? "Failed to send message." });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, error = "An unexpected error occurred: " + ex.Message });
            }
        }
        
    }

}
