using System.Text.Json;
using BLL.Services.Interface;
using BLL.Utilities;
using Common.DTO;
using Common.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;


namespace VNTourLandProject.Pages.NewsAndBlog
{
    public class IndexModel : PageModel
    {
        private readonly IBlogService _blogService;
        private readonly IBlogCategoryService _categoryService;
        private readonly UserUtility _userUtility;
        private readonly IOptions<MapboxOptions> _mapboxOptions;
        private readonly ITourService _tourService;

        public IndexModel(IBlogService blogService, IBlogCategoryService categoryService, UserUtility userUtility, IOptions<MapboxOptions> mapboxOptions, ITourService tourService)
        {
            _blogService = blogService;
            _categoryService = categoryService;
            _userUtility = userUtility;
            _mapboxOptions = mapboxOptions;
            _tourService = tourService;
        }

        public List<BlogDTO> AllBlogs { get; set; } = new();
        public List<BlogDTO> Top3LatestPosts { get; set; } = new();
        public List<CategoryDTO> Categories { get; set; } = new();
        public bool IsAuth => _userUtility.IsAuthenticated();
        public string Role => _userUtility.GetRoleFromToken();

        public string UserName => _userUtility.GetFullNameFromToken();

        //MapboxToken
        public string MapboxToken => _mapboxOptions.Value.AccessToken;

        public async void OnGet()
        {
            var allPostResponse = await _blogService.GetAllBlog();
            if (allPostResponse.IsSuccess && allPostResponse.Result is List<BlogDTO> all)
            {
                AllBlogs = all;

                // Lấy top 3 bài mới nhất (nếu chưa có hàm riêng)
                Top3LatestPosts = all
                    .OrderByDescending(p => p.CreatedDate)
                    .Take(3)
                    .ToList();
            }

            var catResponse = await _categoryService.GetAllCategoryAsync();
            if (catResponse.IsSuccess && catResponse.Result is List<CategoryDTO> catList)
            {
                Categories = catList;
            }
        }


        public async Task<IActionResult> OnPostNearbyTours([FromBody] LocationInputModel input)
        {


            var tours = await _tourService.GetToursNearAsync(input.Latitude, input.Longitude, 50);
            return new JsonResult(tours);
        }


        public class LocationInputModel
        {
            public double Latitude { get; set; }
            public double Longitude { get; set; }
        }

    }
}
