using BLL.Services.Implement;
using BLL.Services.Interface;
using BLL.Utilities;
using Common.DTO;
using Common.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace VNTourLandProject.Pages.TourAndService
{
    public class TourDetailModel : PageModel
    {
        private readonly UserUtility _userUtility;
        private readonly ITourService _tourService;
        private readonly IIncludeService _includeService;
        private readonly INotIncludeService _notIncludeService;
        private readonly IItineraryService _itineraryService;
        //private readonly IAddOnOptionService _addOnOptionService;
        private readonly IBookingService _bookingService;
        private readonly IReviewerService _reviewerService;
        private readonly IChatService _chatService;
        private readonly IFileUploadService _fileUploadService;
        private readonly IOptions<MapboxOptions> _mapboxOptions;

        public TourDetailModel(UserUtility userUtility, ITourService tourService, IIncludeService includeService, INotIncludeService notIncludeService, IItineraryService itineraryService, IBookingService bookingService, IReviewerService reviewerService, IChatService chatService, IFileUploadService fileUploadService, IOptions<MapboxOptions> mapboxOptions)
        {
            _userUtility = userUtility;
            _tourService = tourService;
            _includeService = includeService;
            _notIncludeService = notIncludeService;
            _itineraryService = itineraryService;
            //_addOnOptionService = addOnOptionService;
            _bookingService = bookingService;
            _reviewerService = reviewerService;
            _chatService = chatService;
            _fileUploadService = fileUploadService;
            _mapboxOptions = mapboxOptions;
        }

        public bool isAuth => _userUtility.IsAuthenticated();
        public Guid CurrentUserId => isAuth ? _userUtility.GetUserIDFromToken() : Guid.Empty;
        public string Role => isAuth ? _userUtility.GetRoleFromToken() : string.Empty;
        public string UserName => isAuth ? _userUtility.GetFullNameFromToken() : "Guest";


        [BindProperty(SupportsGet = true)]
        public Guid TourId { get; set; }

        public TourDetailDTO TourDetail { get; set; }
        public List<IncludedDTO> IncludedItems { get; set; } = new();
        public List<NotIncludedDTO> NotIncludedItems { get; set; } = new();
        public List<ItineraryDTO> Itineraries { get; set; } = new();
        //public List<AddOptionDTO> AddOptions { get; set; } = new();

        [BindProperty]
        public CreateBookingDTO createBookingDTO { get; set; }

        [BindProperty]
        public List<Guid> SelectedAddOns { get; set; } = new();

        [BindProperty]
        public CreateReviewerDTO CreateReviewerDTO { get; set; } = new CreateReviewerDTO();
        public List<ReviewerDTO> ReviewerList { get; set; } = new();

        public List<MessageDTO> ChatHistory { get; set; } = new();
        public List<UserChatDTO> ChatCustomers { get; set; } = new();


        public string MapboxToken => _mapboxOptions.Value.AccessToken;


        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                ChatCustomers = await LoadChatCustomersAsync();

                var tourResponse = await _tourService.GetTourDetailByIdAsync(TourId);
                var includeResponse = await _includeService.GetAllByTourId(TourId);
                var notIncludeResponse = await _notIncludeService.GetAllByTourId(TourId);
                var itineraryResponse = await _itineraryService.GetAllItinerariesByTourId(TourId);
                //var addOptionResponse = await _addOnOptionService.GetAllAsync();
                var reviewResponse = await _reviewerService.GetAllByTourId(TourId);

                if (reviewResponse.IsSuccess && reviewResponse.Result is List<ReviewerDTO> reviews)
                    ReviewerList = reviews;

                if (!tourResponse.IsSuccess || tourResponse.Result == null)
                {
                    TempData["Message"] = tourResponse.Message ?? "Failed to load tour information.";
                    return RedirectToPage("/TourManager/ManageTour");
                }

                TourDetail = tourResponse.Result as TourDetailDTO;
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

                if (includeResponse.IsSuccess && includeResponse.Result is List<IncludedDTO> includes)
                    IncludedItems = includes;

                if (notIncludeResponse.IsSuccess && notIncludeResponse.Result is List<NotIncludedDTO> notIncludes)
                    NotIncludedItems = notIncludes;

                if (itineraryResponse.IsSuccess && itineraryResponse.Result is List<ItineraryDTO> itineraries)
                    Itineraries = itineraries;

                //if (addOptionResponse.IsSuccess && addOptionResponse.Result is List<AddOptionDTO> addons)
                //    AddOptions = addons;

                return Page();
            }
            catch (Exception ex)
            {
                TempData["Message"] = "An unexpected error occurred: " + ex.Message;
                return RedirectToPage("/TourManager/ManageTour");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                //createBookingDTO.SelectedAddOns = SelectedAddOns;
                createBookingDTO.TourId = TourId;

                var bookingResponse = await _bookingService.CreateBooking(createBookingDTO);
                if (bookingResponse.IsSuccess && bookingResponse.Result is Guid bookingId)
                {
                    TempData["Message"] = bookingResponse.Message;
                    return RedirectToPage("/Booking/BookingDetail", new { bookingId });
                }

                TempData["Error"] = bookingResponse.Message ?? "Failed to create booking.";
                return RedirectToPage("/TourAndService/TourDetail", new { TourId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An unexpected error occurred: " + ex.Message;
                return RedirectToPage("/TourAndService/TourDetail", new { TourId });
            }
        }

        public async Task<IActionResult> OnPostSubmitReviewAsync()
        {
            try
            {
                var response = await _reviewerService.CreateReviewerByTourIdAsync(CreateReviewerDTO);

                if (!response.IsSuccess)
                {
                    ModelState.AddModelError(string.Empty, response.Message ?? "Failed to submit review.");
                    return Page();
                }

                return RedirectToPage("/TourAndService/TourDetail", new { TourId = CreateReviewerDTO.TourId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Unexpected error: " + ex.Message);
                return Page();
            }
        }

        public async Task<IActionResult> OnGetChatCustomersAsync()
        {
            try
            {
                var users = await LoadChatCustomersAsync();
                return new JsonResult(new { success = true, result = users });
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
                return users;

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

                return new JsonResult(new { success = false, error = response.Message ?? "Failed to load chat." });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, error = "Unexpected error: " + ex.Message });
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
                return new JsonResult(new { success = false, error = "Unexpected error: " + ex.Message });
            }
        }
    }

}
