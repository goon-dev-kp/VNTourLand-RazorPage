using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Common.DTO
{
    public class BlogDTO
    {
        public Guid BlogId { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CountComment { get; set; }
        public string ImageUrl { get; set; }
    }
    public class CreateBlogDTO
    {
        public string Title { get; set; }
        public string Content { get; set; }
        
        public IFormFile ImageFile { get; set; } // ← Dùng để upload hình
        public Guid BlogCategoryId { get; set; } // ← ID thể loại
    }

    public class BlogDetailDTO
    {
        public Guid BlogId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }         // ← Chi tiết bài viết

        public DateTime CreatedDate { get; set; }
        public string ImageUrl { get; set; }

        public Guid BlogCategoryId { get; set; }
    }
}

