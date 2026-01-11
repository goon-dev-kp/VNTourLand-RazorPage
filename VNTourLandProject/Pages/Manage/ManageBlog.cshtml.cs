using BLL.Services.Interface;
using BLL.Utilities;
using Common.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VNTourLandProject.Pages.Manage
{
    public class ManageBlogModel : PageModel
    {
        private readonly UserUtility _userUtility;
        private readonly IBlogService _blogService;
        public ManageBlogModel(UserUtility userUtility, IBlogService blogService)
        {
            _userUtility = userUtility;
            _blogService = blogService;
        }
        public List<BlogDTO> AllBlogs { get; set; } = new();
        public string Role => _userUtility.GetRoleFromToken();
        public bool isAuth => _userUtility.IsAuthenticated();
        public string UserName => _userUtility.GetFullNameFromToken();

        //chat
        public List<MessageDTO> ChatHistory { get; set; } = new();
        public List<UserChatDTO> ChatCustomers { get; set; }

        public Guid CurrentUserId => _userUtility.GetUserIDFromToken();
        public async void OnGet()
        {

            var allPostResponse = await _blogService.GetAllBlog();
            if (allPostResponse.IsSuccess && allPostResponse.Result is List<BlogDTO> all)
            {
                AllBlogs = all;

            }
        }
    }
}
