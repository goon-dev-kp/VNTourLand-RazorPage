using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VNTourLandProject.Pages.Payment
{
    public class SEPayQRCodeModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string QrImageUrl { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal Amount { get; set; }

        [BindProperty(SupportsGet = true)]
        public Guid TransactionId { get; set; }

        public void OnGet()
        {
            // Dữ liệu đã được truyền từ controller hoặc redirect với query string
        }
    }
}
