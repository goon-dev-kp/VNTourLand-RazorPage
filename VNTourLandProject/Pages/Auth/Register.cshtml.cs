using BLL.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static Common.DTO.AuthDTO;

namespace VNTourLandProject.Pages.Auth
{
    public class RegisterModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly IEmailService _emailService;

        public RegisterModel(IAuthService authService, IEmailService emailService)
        {
            _authService = authService;
            _emailService = emailService;
        }

        [BindProperty]
        public RegisterDTO registerDTO { get; set; } = new RegisterDTO();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPost()
        {


            var response = await _authService.Register(registerDTO);
            if (response.IsSuccess)
            {
                TempData["Message"] = "Register successfully. Please login !.";
                await _emailService.SendEmailRegisterSuccessAsync(registerDTO.UserName, registerDTO.Email);
                return RedirectToPage("/Auth/Login");
            }
            else
            {
                TempData["Error"] = "Register false: " + response.Message;
                return Page();
            }
        }
    }
}
