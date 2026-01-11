using Google.Cloud.Storage.V1;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IO;

namespace VNTourLandProject.Pages.Demo
{
    public class UploadModel : PageModel
    {
        [BindProperty]
        public IFormFile FileUpload { get; set; }

        public string UploadUrl { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (FileUpload != null)
            {
                var bucketName = "fir-upload-6b2e0.firebasestorage.app";

                // Đường dẫn file JSON service account key
                var credentialPath = Path.Combine(Directory.GetCurrentDirectory(), "firebase", "fir-upload-6b2e0-firebase-adminsdk-fbsvc-80df89414e.json");

                // Tạo credential từ file json
                var credential = GoogleCredential.FromFile(credentialPath);

                // Tạo StorageClient với credential rõ ràng
                var storage = StorageClient.Create(credential);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(FileUpload.FileName);
                using (var stream = FileUpload.OpenReadStream())
                {
                    await storage.UploadObjectAsync(bucketName, fileName, null, stream);
                }

                UploadUrl = $"https://storage.googleapis.com/{bucketName}/{fileName}";
            }

            return Page();
        }
    }
}
