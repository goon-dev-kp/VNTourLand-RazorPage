using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class Story
    {
        public Guid StoryId { get; set; }
        public Guid LocationOfStoryId { get; set; } // Foreign key

        // Navigation property: Một Story thuộc một Location
        public LocationOfStory LocationOfStory { get; set; }

        public string Title { get; set; }
        public string Content { get; set; }
        public string ImageUrl { get; set; }
        public string AuthorName { get; set; }
        public DateTime StoryDate { get; set; }
        public virtual ICollection<Reviewer> Reviewers { get; set; } = new HashSet<Reviewer>();

    }
}
