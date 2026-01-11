using BLL.Services.Interface;
using BLL.Utilities;
using Common.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VNTourLandProject.Pages.NewsAndBlog
{
    public class BlogDetailModel : PageModel
    {
        private readonly IReviewerService _reviewerService;
        private readonly UserUtility _userUtility;
        private readonly IBlogService _blogService;
        private readonly IBlogCategoryService _blogCategoryService;
        public BlogDetailModel (IReviewerService reviewerService, UserUtility userUtility, IBlogService blogService, IBlogCategoryService blogCategoryService)
        {
            _reviewerService = reviewerService;
            _userUtility = userUtility;
            _blogService = blogService;
            _blogCategoryService = blogCategoryService;
        }
        [BindProperty(SupportsGet = true)]
        public Guid BlogId { get; set; }

        [BindProperty]
        public CreateReviewerDTO NewReview { get; set; } = new CreateReviewerDTO();

        public List<ReviewerDTO> BlogReviews { get; set; } = new List<ReviewerDTO>();

        public List<BlogDTO> Top3LatestPosts { get; set; } = new();
        public List<CategoryDTO> Categories { get; set; } = new();

        public BlogDetailDTO BlogDetail { get; set; }


        // ✅ Public property để Razor view dùng Model.IsAuth
        public bool IsAuth => _userUtility.IsAuthenticated();
        public string Role => _userUtility.GetRoleFromToken();
        
        public string UserName => _userUtility.GetFullNameFromToken();
        public async Task OnGet()
        {
            // GET BLOG BY ID 
            // GET ALL CATEGORY ( NAME )
            // GET TOP 3 CREATE DATE NEW ( NAME, CREATEDATE)
            // GET COMENT ( REVIEWER ) CỦA BLOG
            // CREATE REVIEWER ( COMMENT - TYPE = BLOG )

            // GET BLOG BY CATEGORY ( ID )
            var blogResponse = await _blogService.GetBlogByIdAsync(BlogId);
            if (blogResponse.IsSuccess && blogResponse.Result is BlogDetailDTO blog)
            {
                BlogDetail = blog;
            }
            else
            {
                // Nếu không tìm thấy blog, có thể redirect hoặc báo lỗi
                TempData["Message"] = "Bài viết không tồn tại hoặc đã bị xoá.";
                RedirectToPage("/NewsAndBlog/Index"); // hoặc return NotFound();
            }

            var allPostResponse = await _blogService.GetAllBlog();
            if (allPostResponse.IsSuccess && allPostResponse.Result is List<BlogDTO> all)
            {


                // Lấy top 3 bài mới nhất (nếu chưa có hàm riêng)
                Top3LatestPosts = all
                    .OrderByDescending(p => p.CreatedDate)
                    .Take(3)
                    .ToList();
            }

            var catResponse = await _blogCategoryService.GetAllCategoryAsync();
            if (catResponse.IsSuccess && catResponse.Result is List<CategoryDTO> catList)
            {
                Categories = catList;
            }

            var response = await _reviewerService.GetReviewerByBlogId(BlogId);

            if (response != null && response.IsSuccess && response.Result is List<ReviewerDTO> list)
            {
                BlogReviews = list;
            }
        }

        public async Task<IActionResult> OnPost()
        {
            NewReview.BlogId = BlogId;

            var result = await _reviewerService.CreateReviewerByBlogIdAsync(NewReview);

            if (result.IsSuccess)
            {
                TempData["Message"] = "Bình luận đã được gửi thành công!";
                return RedirectToPage(new { BlogId }); 
            }

            TempData["Message"] = result.Message;
            await OnGet(); 
            return Page();
        }
    }
}
