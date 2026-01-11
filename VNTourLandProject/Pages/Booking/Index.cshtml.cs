using BLL.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VNTourLandProject.Pages.Booking
{
    public class IndexModel : PageModel
    {
        private readonly UserUtility _userUtility;
        public IndexModel(UserUtility userUtility)
        {
            _userUtility = userUtility;
        }

        public string Role => _userUtility.GetRoleFromToken();
        public bool isAuth => _userUtility.IsAuthenticated();
        public string UserName => _userUtility.GetFullNameFromToken();
        public void OnGet()
        {
        }
    }
}
