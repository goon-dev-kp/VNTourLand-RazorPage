using BLL.Services.Implement;
using BLL.Services.Interface;
using BLL.Utilities;
using Common.DTO;
using Common.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace VNTourLandProject.Pages.TravelInfo
{
    public class IndexModel : PageModel
    {
        private readonly UserUtility _userUtility;
        private readonly ITourService _tourService;
        private readonly IOptions<MapboxOptions> _mapboxOptions;
        private readonly ILocationOfStoryService _locationOfStoryService;
        private readonly IChatService _chatService;
        private readonly IFileUploadService _fileUploadService;

        public IndexModel(
            UserUtility userUtility,
            ITourService tourService,
            IOptions<MapboxOptions> mapboxOptions,
            ILocationOfStoryService locationOfStoryService,
            IChatService chatService,
            IFileUploadService fileUploadService)
        {
            _userUtility = userUtility;
            _tourService = tourService;
            _mapboxOptions = mapboxOptions;
            _locationOfStoryService = locationOfStoryService;
            _chatService = chatService;
            _fileUploadService = fileUploadService;
        }

        public bool isAuth => _userUtility.IsAuthenticated();
        public Guid CurrentUserId => isAuth ? _userUtility.GetUserIDFromToken() : Guid.Empty;
        public string Role => isAuth ? _userUtility.GetRoleFromToken() : string.Empty;
        public string UserName => isAuth ? _userUtility.GetFullNameFromToken() : "Guest";

        public string MapboxToken => _mapboxOptions.Value.AccessToken;

        public List<LocationOfStoryDTO> Locations { get; set; } = new();
        public List<StoryDTO> LatestStories { get; set; } = new();

        public List<MessageDTO> ChatHistory { get; set; } = new();
        public List<UserChatDTO> ChatCustomers { get; set; } = new();



        public async Task OnGetAsync()
        {
            ChatCustomers = await LoadChatCustomersAsync();

            var response = await _locationOfStoryService.GetAllLocationsWithStoriesAsync();
            if (response.IsSuccess && response.Result is List<LocationOfStoryDTO> locations)
            {
                Locations = locations;
                //LatestStories = locations
                //    .SelectMany(loc => loc.Stories)
                //    .OrderByDescending(s => s.StoryDate)
                //    .DistinctBy(s => s.StoryId)
                //    .Take(3)
                //    .ToList();
            }
            else
            {
                Locations = new List<LocationOfStoryDTO>();
                //LatestStories = new List<StoryDTO>();
            }
        }

        public async Task<IActionResult> OnPostNearbyTours([FromBody] LocationInputModel input)
        {
            var tours = await _tourService.GetToursNearAsync(input.Latitude, input.Longitude, 50);
            return new JsonResult(tours);
        }

        public async Task<IActionResult> OnGetChatCustomersAsync()
        {
            var users = await LoadChatCustomersAsync();
            return new JsonResult(new { success = true, result = users });
        }

        private async Task<List<UserChatDTO>> LoadChatCustomersAsync()
        {
            var response = await _chatService.GetChatUsersAsync(CurrentUserId);
            return response.IsSuccess && response.Result is List<UserChatDTO> users ? users : new List<UserChatDTO>();
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

            return new JsonResult(new { success = false, message = response.Message });
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

        public class LocationInputModel
        {
            public double Latitude { get; set; }
            public double Longitude { get; set; }
        }
    }
}
