using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class LocationOfStory
    {
        public Guid LocationOfStoryId { get; set; }
        public string LocationOfStoryName { get; set; }
        public string Description { get; set; }
        public List<string> BannerImageUrl { get; set; }
        public ICollection<Story> Stories { get; set; } = new List<Story>();
    }
}
