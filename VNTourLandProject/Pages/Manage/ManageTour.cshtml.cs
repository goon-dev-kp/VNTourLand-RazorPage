using BLL.Services.Implement;
using BLL.Services.Interface;
using BLL.Utilities;
using Common.DTO;
using Common.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace VNTourLandProject.Pages.Manage
{
    public class ManageTourModel : PageModel
    {
        private readonly ITourService _tourService;
        private readonly UserUtility _userUtility;
        private readonly IChatService _chatService;
        private readonly IFileUploadService _fileUploadService;
        private readonly IOptions<MapboxOptions> _mapboxOptions;


        public ManageTourModel(ITourService tourService, UserUtility userUtility, IChatService chatService, IFileUploadService fileUploadService, IOptions<MapboxOptions> mapboxOptions)
        {
            _tourService = tourService;
            _userUtility = userUtility;
            _chatService = chatService;
            _fileUploadService = fileUploadService;
            _mapboxOptions = mapboxOptions;
        }

        // Auth info
        public bool isAuth => _userUtility.IsAuthenticated();
        public Guid CurrentUserId => isAuth ? _userUtility.GetUserIDFromToken() : Guid.Empty;
        public string Role => isAuth ? _userUtility.GetRoleFromToken() : string.Empty;
        public string UserName => isAuth ? _userUtility.GetFullNameFromToken() : "Guest";
        public string MapboxToken => _mapboxOptions.Value.AccessToken;



        // Tour & Chat data
        public List<TourDTO> Tours { get; set; } = new();
        public List<UserChatDTO> ChatCustomers { get; set; } = new();
        public List<MessageDTO> ChatHistory { get; set; } = new();

        [BindProperty]
        public UpdateTourRequest EditTour { get; set; }

        [BindProperty]
        public Guid DeleteTourId { get; set; }

        // ✅ Load all tours & edit tour if ID is passed
        public async Task<IActionResult> OnGetAsync(Guid? editId = null)
        {
            if (!isAuth || Role != "MANAGER")
            {
                TempData["Message"] = "You must log in first.";
                return RedirectToPage("/Auth/Login");
            }

            // Load Tours
            var response = await _tourService.GetAllTour();
            Tours = response.IsSuccess && response.Result is List<TourDTO> list ? list : new();

            if (editId.HasValue)
            {
                var tour = Tours.FirstOrDefault(t => t.TourId == editId.Value);
                if (tour != null)
                {
                    EditTour = new UpdateTourRequest
                    {
                        TourId = tour.TourId,
                        TourName = tour.TourName,
                        TourDescription = tour.TourDescription,
                        StartDate = tour.StartDate,
                        EndDate = tour.EndDate,
                        PriceOfAdult = tour.PriceOfAdult,
                        PriceOfChild = tour.PriceOfChild,
                        
                        Type = tour.Type,
                        Status = tour.Status,
                        IsActive = tour.IsActive,
                        ImageUrl = tour.ImageUrl,
                        // ✅ Chuyển từ List<string> sang List<LocationRequest>
                        Locations = tour.Location?.Select(l => new LocationRequest
                        {
                            LocationName = l
                        }).ToList()
                    };
                }
            }

            ChatCustomers = await LoadChatCustomersAsync();
            return Page();
        }

        // ✅ Toggle tour status
        public async Task<IActionResult> OnPostChangeStatusAsync(Guid tourId, bool isActive)
        {
            if (isActive)
                await _tourService.EnableTourAsync(tourId);
            else
                await _tourService.DisableTourAsync(tourId);

            return RedirectToPage();
        }

        // ✅ Edit/update tour
        public async Task<IActionResult> OnPostEditAsync()
        {
            if (!ModelState.IsValid)
            {
                if (EditTour.NewImage == null || EditTour.NewImage.Length == 0)
                    ModelState.Remove(nameof(EditTour.NewImage));

                if (!ModelState.IsValid)
                    return Page();
            }

            //// Upload new image if exists
            //if (EditTour.NewImage != null && EditTour.NewImage.Length > 0)
            //{
            //    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(EditTour.NewImage.FileName)}";
            //    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            //    Directory.CreateDirectory(uploadPath);
            //    var filePath = Path.Combine(uploadPath, fileName);

            //    using var stream = new FileStream(filePath, FileMode.Create);
            //    await EditTour.NewImage.CopyToAsync(stream);

            //    EditTour.ImageUrl = $"/uploads/{fileName}";
            //}

            var result = await _tourService.UpdateTourByIdAsync(EditTour);
            if (result.IsSuccess)
            {
                TempData["Success"] = "Tour updated successfully.";
                return RedirectToPage();
            }

            ModelState.AddModelError(string.Empty, result.Message ?? "Error updating tour.");
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteTourAsync()
        {
            var response = await _tourService.DeleteTourAsync(DeleteTourId);
            if (response.IsSuccess)
            {
                TempData["Message"] = "Tour delete successfully.";
                return RedirectToPage();
            }


            TempData["Error"] = response.Message ?? "Failed to delete tour.";
            return RedirectToPage();
        }

        // ✅ Get chat customers
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

        // ✅ Get conversation with a customer
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

        // ✅ Send a chat message
        public async Task<IActionResult> OnPostSendChatAsync()
        {
            var message = Request.Form["message"];
            var customerId = Request.Form["customerId"];
            var file = Request.Form.Files["image"];
            string imageUrl = null;

            if (string.IsNullOrEmpty(message) && (file == null || file.Length == 0))
                return new JsonResult(new { success = false, error = "Message or image is required!" });

            if (string.IsNullOrEmpty(customerId))
                return new JsonResult(new { success = false, error = "Customer ID is missing!" });

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
    }

}
