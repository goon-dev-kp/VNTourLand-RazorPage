using System.Security.Claims;
using BLL.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VNTourLandProject.Pages
{
    public class SigninGoogleModel : PageModel
    {
        private readonly IAuthService _authService;

        public SigninGoogleModel(IAuthService authService)
        {
            _authService = authService;
        }
        public async Task<IActionResult> OnGetAsync()
        {
            // Lấy thông tin user Google từ HttpContext.User
            var email = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(email))
            {
                return RedirectToPage("/Login", new { error = "Không lấy được email từ Google" });
            }

            // Gọi hàm login Google trong service của bạn
            var response = await _authService.LoginWithGoogle(email);

            if (response.IsSuccess)
            {
                // ✅ Ép kiểu response.Result về dynamic để lấy token
                dynamic tokenResult = response.Result;
                string accessToken = tokenResult?.AccessToken;



                if (!string.IsNullOrEmpty(accessToken))
                {
                    // ✅ Lưu AccessToken vào Cookie
                    HttpContext.Response.Cookies.Append("AccessToken", accessToken, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTimeOffset.UtcNow.AddHours(1)
                    });
                }

                // ✅ Lấy Role từ token đã lưu trong cookie
                var role = tokenResult?.Role;

                // ✅ Điều hướng theo role
                if (role == "MANAGER")
                {
                    TempData["Message"] = "Đăng nhập thành công Manager";
                    return RedirectToPage("/Manage/ManageTour");
                }
                else if (role == "USER")
                {
                    TempData["Message"] = "Đăng nhập thành công User";
                    return RedirectToPage("/Home/Index");
                }
                else if (role == "SELLER")
                {
                    TempData["Message"] = "Đăng nhập thành công Seller";
                    return RedirectToPage("/Seller/Bookings");
                }
                else if (role == "ADMIN")
                {
                    TempData["Message"] = "Đăng nhập thành công Admin";
                    return RedirectToPage("/Admin/DashboardOfAdmin");
                }

                else
                {
                    TempData["Message"] = "Quyền không hợp lệ";
                    return Page();
                }
            }
            else
            {
                TempData["Message"] = "Đăng nhập thất bại";
                return Page();
            }
        }
    }
}
