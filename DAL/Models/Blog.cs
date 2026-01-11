using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class Blog
    {
        public Guid BlogId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Image { get; set; } 

        public virtual ICollection<Reviewer> Reviewers { get; set; } = new HashSet<Reviewer>();

        // Khóa ngoại đến Category
        public Guid BlogCategoryId { get; set; }

        // Navigation property
        public virtual BlogCategory BlogCategory { get; set; }
    }
}
