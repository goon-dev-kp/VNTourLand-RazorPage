using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Enums;

namespace Common.DTO
{
    public class ReviewerDTO
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public int? Rating { get; set; }
        public string Comment { get; set; }
        public DateTime DateTime { get; set; }
    }

    public class CreateReviewerDTO
    {
        public Guid? BlogId { get; set; }
        public Guid? TourId { get; set; }
        public Guid? StoryId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }

    }
}
