using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class BlogCategory
    {
        public Guid BlogCategoryId { get; set; }
        public string CategoryName { get; set; }

        // Một Category có nhiều Blog
        public virtual ICollection<Blog> Blogs { get; set; } = new HashSet<Blog>();
    }
}
