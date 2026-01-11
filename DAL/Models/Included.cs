using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class Included
    {
        public Guid IncludedId { get; set; }
        public string Description { get; set; }
        public Guid TourId { get; set; }
        public virtual Tour Tour { get; set; }
    }
}
