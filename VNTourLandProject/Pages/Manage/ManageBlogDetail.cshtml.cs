using BLL.Services.Interface;
using BLL.Utilities;
using Common.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VNTourLandProject.Pages.Manage
{
    public class ManageBlogDetailModel : PageModel
    {
        private readonly UserUtility _userUtility;
        private readonly IBlogService _blogService;
        public ManageBlogDetailModel(UserUtility userUtility, IBlogService blogService)
        {
            _userUtility = userUtility;
            _blogService = blogService;
        }

        [BindProperty(SupportsGet = true)]
        public Guid BlogId { get; set; }
        public BlogDetailDTO Blog { get; set; } = new();
        public string Role => _userUtility.GetRoleFromToken();
        public bool isAuth => _userUtility.IsAuthenticated();
        public string UserName => _userUtility.GetFullNameFromToken();
        public async Task<IActionResult> OnGetAsync()
        {

            if (BlogId == Guid.Empty)
            {
                TempData["Message"] = "Không tìm thấy bài viết.";
                return RedirectToPage("/Manage/ManageBlog");
            }

            var response = await _blogService.GetBlogByIdAsync(BlogId);

            if (!response.IsSuccess || response.Result == null)
            {
                TempData["Message"] = response.Message ?? "Không tìm thấy bài viết.";
                return RedirectToPage("/Manage/ManageBlog");
            }

            Blog = response.Result as BlogDetailDTO;
            return Page();
        }
    }
}
