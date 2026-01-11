using BLL.Services.Interface;
using BLL.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using static Common.DTO.AuthDTO;

namespace VNTourLandProject.Pages.Auth
{
    public class LoginModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly UserUtility _userUtility;

        public LoginModel(IAuthService authService, UserUtility userUtility)
        {
            _authService = authService;
            _userUtility = userUtility;
        }

        [BindProperty]
        public LoginDTO loginDTO { get; set; } = new LoginDTO();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPost()
        {
            var response = await _authService.Login(loginDTO);
            if (response.IsSuccess)
            {
                // ✅ Parse token
                dynamic tokenResult = response.Result;
                string accessToken = tokenResult?.AccessToken;

                if (!string.IsNullOrEmpty(accessToken))
                {
                    HttpContext.Response.Cookies.Append("AccessToken", accessToken, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTimeOffset.UtcNow.AddHours(1)
                    });
                }

                var role = tokenResult?.Role;

                // ✅ Redirect based on role with English messages
                switch (role)
                {
                    case "MANAGER":
                        TempData["Message"] = "Login successful as Manager.";
                        return RedirectToPage("/Manage/ManageTour");
                    case "USER":
                        TempData["Message"] = "Login successful.";
                        return RedirectToPage("/Home/Index");
                    case "SELLER":
                        TempData["Message"] = "Login successful as Seller.";
                        return RedirectToPage("/Seller/Bookings");
                    case "ADMIN":
                        TempData["Message"] = "Login successful as Admin.";
                        return RedirectToPage("/Admin/DashboardOfAdmin");
                    default:
                        TempData["Error"] = "Invalid role. Please contact the system administrator.";
                        return RedirectToPage("/Login");
                }
            }
            else
            {
                TempData["Error"] = "Login failed. Please check your credentials.";
                return Page();
            }
        }
    }

}
