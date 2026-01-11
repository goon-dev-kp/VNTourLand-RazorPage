using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Common.DTO
{
    public class LocationOfStoryDTO
    {
        public Guid LocationOfStoryId { get; set; }
        public string LocationOfStoryName { get; set; }
        public string Description { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public List<string> BannerImageUrl { get; set; }
        public List<StoryDTO> Stories { get; set; }
    }

    public class LocationOfStoryCreateDTO
    {
        public string LocationOfStoryName { get; set; }
        public string Description { get; set; }


        // Thay vì string, dùng IFormFile để upload ảnh banner
        public List<IFormFile> BannerImageFile { get; set; }
    }

    public class LocationOfStoryUpdateDTO
    {
        public Guid LocationOfStoryId { get; set; }

        public string LocationOfStoryName { get; set; }

        public string Description { get; set; }

        public List<IFormFile> BannerImageFile { get; set; } // Có thể null => giữ lại ảnh cũ

        
    }

}
