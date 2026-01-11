using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLL.Services.Interface;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Http;

namespace BLL.Services.Implement
{
    public class FileUploadService : IFileUploadService
    {
        public async Task<string> UploadImageToFirebaseAsync(IFormFile file)
        {
            // ✅ Đúng tên bucket Firebase dùng để hiển thị ảnh public
            var bucketName = "vntourland-551b5.firebasestorage.app";



            // ✅ Đường dẫn tới file json chứa service account key
            var credentialPath = Path.Combine(Directory.GetCurrentDirectory(), "firebase", "vntourland-551b5-firebase-adminsdk-fbsvc-3cf27b8437.json");
            var credential = GoogleCredential.FromFile(credentialPath);
            var storage = StorageClient.Create(credential);

            // ✅ Tạo fileName mới
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

            using (var stream = file.OpenReadStream())
            {
                // Upload file
                await storage.UploadObjectAsync(bucketName, fileName, null, stream, new UploadObjectOptions
                {
                    PredefinedAcl = PredefinedObjectAcl.PublicRead  // 👈 Cho phép truy cập công khai
                });
            }

            // ✅ Trả về URL đúng format public (có alt=media để hiển thị)
            return $"https://firebasestorage.googleapis.com/v0/b/{bucketName}/o/{Uri.EscapeDataString(fileName)}?alt=media";
        }
    }
}
