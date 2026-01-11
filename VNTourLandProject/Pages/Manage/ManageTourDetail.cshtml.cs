using BLL.Services.Implement;
using BLL.Services.Interface;
using BLL.Utilities;
using Common.DTO;
using DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VNTourLandProject.Pages.Manage
{
    public class ManageTourDetailModel : PageModel
    {
        private readonly ITourService _tourService;
        private readonly IIncludeService _includeService;
        private readonly INotIncludeService _notIncludeService;
        private readonly IItineraryService _itineraryService;
        private readonly UserUtility _userUtility;
        private readonly IChatService _chatService;
        private readonly IFileUploadService _fileUploadService;

        public ManageTourDetailModel(
            ITourService tourService,
            IIncludeService includeService,
            INotIncludeService notIncludeService,
            IItineraryService itineraryService,
            UserUtility userUtility,
            IChatService chatService,
            IFileUploadService fileUploadService)
        {
            _tourService = tourService;
            _includeService = includeService;
            _notIncludeService = notIncludeService;
            _itineraryService = itineraryService;
            _userUtility = userUtility;
            _chatService = chatService;
            _fileUploadService = fileUploadService;
        }

        // Auth
        public bool isAuth => _userUtility.IsAuthenticated();
        public Guid CurrentUserId => isAuth ? _userUtility.GetUserIDFromToken() : Guid.Empty;
        public string Role => isAuth ? _userUtility.GetRoleFromToken() : string.Empty;
        public string UserName => isAuth ? _userUtility.GetFullNameFromToken() : "Guest";


        // Properties
        [BindProperty]
        public Guid TourId { get; set; }

        public TourDetailDTO TourDetail { get; set; }
        public List<IncludedDTO> IncludedItems { get; set; } = new();
        public List<NotIncludedDTO> NotIncludedItems { get; set; } = new();
        public List<ItineraryDTO> Itineraries { get; set; } = new();

        // Chat
        public List<MessageDTO> ChatHistory { get; set; } = new();
        public List<UserChatDTO> ChatCustomers { get; set; } = new();

        // ✅ Load thông tin chi tiết tour sau khi post ID
        public async Task<IActionResult> OnPostAsync()
        {
            if (!isAuth || Role != "MANAGER")
            {
                TempData["Message"] = "You must log in first.";
                return RedirectToPage("/Auth/Login");
            }

            var tourResponse = await _tourService.GetTourDetailByIdAsync(TourId);
            if (!tourResponse.IsSuccess || tourResponse.Result == null)
            {
                TempData["Message"] = tourResponse.Message ?? "Không thể lấy thông tin tour.";
                return RedirectToPage("/TourManager/ManageTour");
            }

            TourDetail = tourResponse.Result as TourDetailDTO;
            ProcessImageUrl();

            // Load các thành phần liên quan
            IncludedItems = await LoadListAsync<IncludedDTO>(_includeService.GetAllByTourId, TourId);
            NotIncludedItems = await LoadListAsync<NotIncludedDTO>(_notIncludeService.GetAllByTourId, TourId);
            Itineraries = await LoadListAsync<ItineraryDTO>(_itineraryService.GetAllItinerariesByTourId, TourId);

            ChatCustomers = await LoadChatCustomersAsync();

            return Page();
        }

        // ✅ Tách xử lý ảnh (nếu là JSON array, lấy ảnh đầu tiên)
        private void ProcessImageUrl()
        {
            if (!string.IsNullOrEmpty(TourDetail.ImageUrl) && TourDetail.ImageUrl.Trim().StartsWith("["))
            {
                try
                {
                    var list = System.Text.Json.JsonSerializer.Deserialize<List<string>>(TourDetail.ImageUrl);
                    TourDetail.ImageUrl = list?.FirstOrDefault() ?? "";
                }
                catch
                {
                    TourDetail.ImageUrl = "";
                }
            }
        }

        // ✅ Helper: Gọi service chung để load list
        private async Task<List<T>> LoadListAsync<T>(Func<Guid, Task<ResponseDTO>> serviceFunc, Guid tourId)
        {
            var response = await serviceFunc(tourId);
            return response.IsSuccess && response.Result is List<T> result ? result : new List<T>();
        }

        // ✅ Chat section
        public async Task<IActionResult> OnGetAsync()
        {
            ChatCustomers = await LoadChatCustomersAsync();
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
            return response.IsSuccess && response.Result is List<UserChatDTO> users ? users : new();
        }

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

            var senderId = _userUtility.GetUserIDFromToken();
            var receiverId = Guid.Parse(customerId);

            var response = await _chatService.SendMessageAsync(senderId, receiverId, message, imageUrl);

            return response.IsSuccess
                ? new JsonResult(new { success = true, imageUrl })
                : new JsonResult(new { success = false, error = response.Message });
        }
    }

}
