using BLL.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VNTourLandProject.Pages.SpecialService
{
    public class ServiceDetailModel : PageModel
    {
        private readonly UserUtility _userUtility;
        public ServiceDetailModel(UserUtility userUtility)
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
