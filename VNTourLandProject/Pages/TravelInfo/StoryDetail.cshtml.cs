using BLL.Services.Implement;
using BLL.Services.Interface;
using BLL.Utilities;
using Common.DTO;
using Common.Settings;
using DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace VNTourLandProject.Pages.TravelInfo
{
    public class StoryDetailModel : PageModel
    {
        private readonly UserUtility _userUtility;
        private readonly ITourService _tourService;
        private readonly IRequestOfCustomerService _requestService;
        private readonly IStoryService _storyService;
        private readonly IReviewerService _reviewerService;
        private readonly IChatService _chatService;
        private readonly IFileUploadService _fileUploadService;

        public StoryDetailModel(UserUtility userUtility, ITourService tourService, IRequestOfCustomerService requestOfCustomerService, IStoryService storyService, IReviewerService reviewerService, IChatService chatService, IFileUploadService fileUploadService)
        {
            _userUtility = userUtility;
            _tourService = tourService;
            _requestService = requestOfCustomerService;
            _storyService = storyService;
            _reviewerService = reviewerService;
            _chatService = chatService;
            _fileUploadService = fileUploadService;
        }

        public bool isAuth => _userUtility.IsAuthenticated();
        public Guid CurrentUserId => isAuth ? _userUtility.GetUserIDFromToken() : Guid.Empty;
        public string Role => isAuth ? _userUtility.GetRoleFromToken() : string.Empty;
        public string UserName => isAuth ? _userUtility.GetFullNameFromToken() : "Guest";

        [BindProperty(SupportsGet = true)]
        public Guid TourId { get; set; } = Guid.Empty;


        [BindProperty]
        public RequestOfCustomerDTO RequestOfCustomer { get; set; }

        [BindProperty(SupportsGet = true)]
        public Guid StoryId { get; set; }

        public StoryDTO Story { get; set; }

        [BindProperty]
        public CreateReviewerDTO CreateReviewerDTO { get; set; } = new CreateReviewerDTO();
        public List<ReviewerDTO> ReviewerList { get; set; } = new();

        public List<MessageDTO> ChatHistory { get; set; } = new();
        public List<UserChatDTO> ChatCustomers { get; set; }


        public async Task OnGetAsync()
        {
            if (StoryId == Guid.Empty)
            {
                Story = null;
                return;
            }

            var response = await _storyService.GetStoryByIdAsync(StoryId);

            if (response.IsSuccess)
            {
                Story = (StoryDTO)response.Result;
            }
            else
            {
                Story = null;
            }

            var reviewResponse = await _reviewerService.GetAllByStoryId(StoryId);
            if (reviewResponse.IsSuccess && reviewResponse.Result is List<ReviewerDTO> reviews)
            {
                ReviewerList = reviews;
            }
            else
            {
                ReviewerList = new List<ReviewerDTO>();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (RequestOfCustomer.StartDate.HasValue && RequestOfCustomer.StartDate.Value.Date < DateTime.UtcNow.Date)
            {
                ModelState.AddModelError("Request.StartDate", "The tour start date must be today or later.");
            }

            if (RequestOfCustomer.EndDate.HasValue)
            {
                if (!RequestOfCustomer.StartDate.HasValue)
                {
                    ModelState.AddModelError("Request.EndDate", "Please enter a start date before entering an end date.");
                }
                else
                {
                    if (RequestOfCustomer.EndDate.Value.Date <= RequestOfCustomer.StartDate.Value.Date)
                    {
                        ModelState.AddModelError("Request.EndDate", "The tour end date must be after the start date.");
                    }
                    if (RequestOfCustomer.EndDate.Value.Date <= DateTime.UtcNow.Date)
                    {
                        ModelState.AddModelError("Request.EndDate", "The tour end date must be in the future.");
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                foreach (var state in ModelState)
                {
                    var key = state.Key;
                    var errors = state.Value.Errors;
                    foreach (var error in errors)
                    {
                        Console.WriteLine($"Field: {key}, Message: {error.ErrorMessage}");
                        ModelState.AddModelError(string.Empty, $"Error in field '{key}': {error.ErrorMessage}");
                    }
                }
                return Page();
            }

            var userId = _userUtility.GetUserIDFromToken();
            if (userId == Guid.Empty)
            {
                ModelState.AddModelError(string.Empty, "User is not logged in.");
                return Page();
            }

            await _requestService.CreateRequestOfCustomerAsync(RequestOfCustomer, userId);
            return RedirectToPage("/TravelInfo/Index");
        }

        public async Task<IActionResult> OnPostSubmitReviewAsync()
        {
            Console.WriteLine($"StoryId: {CreateReviewerDTO.StoryId}");

            var response = await _reviewerService.CreateReviewerByStoryIdAsync(CreateReviewerDTO);

            if (!response.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, response.Message ?? "Failed to submit review.");
                return Page();
            }

            return RedirectToPage("/TourAndService/TourDetail", new { TourId = CreateReviewerDTO.TourId });
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

            Console.WriteLine("Calling OnPostSendChatAsync");
            Console.WriteLine("message = " + message);
            Console.WriteLine("customerId = " + customerId);
            Console.WriteLine("image = " + file?.FileName);

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
    }

}
