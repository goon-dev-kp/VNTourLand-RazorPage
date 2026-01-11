using BLL.Services.Interface;
using BLL.Utilities;
using Common.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VNTourLandProject.Pages.TourAndService
{
    public class YourByTypeModel : PageModel
    {
        private readonly UserUtility _userUtility;
        private readonly ITourService _tourService;
        private readonly IChatService _chatService;
        private readonly IFileUploadService _fileUploadService;
        private readonly IStoryService _storyService;

        public YourByTypeModel(UserUtility userUtility, ITourService tourService, IChatService chatService, IFileUploadService fileUploadService, IStoryService storyService)
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

        [BindProperty(SupportsGet = true)]
        public string TourType { get; set; }



        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                // Nếu chọn MICE, chuyển hướng tới trang SpecialService/Index
                if (TourType == "MICE")
                {
                    return RedirectToPage("/SpecialService/Index");
                }

                // Lấy danh sách khách hàng chat (nếu cần thiết)
                ChatCustomers = await LoadChatCustomersAsync();

                var allTours = new List<TourDTO>(); // Khởi tạo danh sách tour

                // Xử lý với INBOUND
                if (TourType == "INBOUND")
                {
                    var inboundTypes = new List<string> { "INBOUND", "INBOUND_MIEN_BAC", "INBOUND_MIEN_TRUNG", "INBOUND_MIEN_TAY", "INBOUND_MIEN_BIEN", "INBOUND_MIEN_NUI" };
                    foreach (var type in inboundTypes)
                    {
                        var response = await _tourService.GetAllTourByType(type);
                        if (response.IsSuccess && response.Result is List<TourDTO> tours)
                        {
                            allTours.AddRange(tours);
                        }
                    }
                }
                // Xử lý với THEME
                else if (TourType == "THEME")
                {
                    var themeTypes = new List<string> { "THEME", "THEME_HONEYMOON", "THEME_CHU_DE", "THEME_LICH_SU", "THEME_XUYEN_VIET" };
                    foreach (var type in themeTypes)
                    {
                        var response = await _tourService.GetAllTourByType(type);
                        if (response.IsSuccess && response.Result is List<TourDTO> tours)
                        {
                            allTours.AddRange(tours);
                        }
                    }
                }
                // Xử lý với OUTBOUND, chỉ lấy 1 tour
                else if (TourType == "OUTBOUND")
                {
                    var response = await _tourService.GetAllTourByType("OUTBOUND");
                    if (response.IsSuccess && response.Result is List<TourDTO> tours && tours.Any())
                    {
                        allTours.AddRange(tours);
                    
                    }
                }
                else
                {
                    // Lấy các tour khác (nếu có)
                    var response = await _tourService.GetAllTourByType(TourType);
                    if (response.IsSuccess && response.Result is List<TourDTO> tours)
                    {
                        allTours.AddRange(tours);
                    }
                }

                if (allTours.Any())
                {
                    foreach (var tour in allTours)
                    {
                        var stories = new List<StoryDTO>();

                        // Nếu tour.Location là List<string>
                        var locationNames = tour.Location ?? new List<string>();

                        foreach (var locationName in locationNames)
                        {
                            var storyResponse = await _storyService.GetStoriesByLocationNameAsync(locationName);
                            if (storyResponse.IsSuccess && storyResponse.Result is List<StoryDTO> locationStories)
                            {
                                stories.AddRange(locationStories);
                            }
                        }

                        TourWithStories.Add(new TourWithStoryDTO
                        {
                            Tour = tour,
                            Stories = stories.Any() ? stories.GroupBy(s => s.StoryId).Select(g => g.First()).ToList() : new List<StoryDTO>()
                        });
                    }
                }




            }
            catch (Exception ex)
            {
                TempData["Message"] = "An error occurred while loading data: " + ex.Message;
            }

            return Page(); // Trả về trang hiện tại
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
