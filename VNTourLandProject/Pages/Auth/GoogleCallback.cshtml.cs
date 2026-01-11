using BLL.Services.Interface;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VNTourLandProject.Pages.Auth
{
    public class GoogleCallbackModel : PageModel
    {
        private readonly IAuthService _authService;

        public GoogleCallbackModel(IAuthService authService)
        {
            _authService = authService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Không lấy được email từ Google.";
                return RedirectToPage("/Login");
            }

            var response = await _authService.LoginWithGoogle(email);

            if (response.IsSuccess)
            {
                dynamic tokenResult = response.Result;
                string accessToken = tokenResult?.AccessToken;
                string role = tokenResult?.Role;

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
                        return Page();
                }
            }
            else
            {
                TempData["Error"] = "Login failed. Please check your credentials.";
                return RedirectToPage("/Login");
            }
        }
    }
}
