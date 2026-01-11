using BLL.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VNTourLandProject.Pages.Auth
{
    public class LogoutModel : PageModel
    {
        private readonly IAuthService _authService;

        public LogoutModel(IAuthService authService)
        {
            _authService = authService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var result = await _authService.Logout();

            if (!result.IsSuccess)
            {
                TempData["Error"] = "Logout false !";
                return RedirectToPage("/Home/Index");
            }

            TempData["Message"] = "Logout successful !";

            return RedirectToPage("/Auth/Login"); // chuyển hướng sau khi logout thành công
        }
    }
}
