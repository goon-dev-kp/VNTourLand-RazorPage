using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Common.DTO
{
    public class StoryDTO
    {
        public Guid StoryId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string ImageUrl { get; set; }
        public string AuthorName { get; set; }
        public DateTime StoryDate { get; set; }
    }
    public class StoryCreateDTO
    {
        public Guid LocationOfStoryId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string AuthorName { get; set; }
        public DateTime StoryDate { get; set; }

        // File ảnh upload lên Firebase
        public IFormFile ImageFile { get; set; }
    }

    public class StoryEditDTO
    {
        public Guid StoryId { get; set; }
        public Guid LocationOfStoryId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string AuthorName { get; set; }
        public DateTime StoryDate { get; set; }
        public IFormFile? ImageFile { get; set; }  // optional
    }

}
