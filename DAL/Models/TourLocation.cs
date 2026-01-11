using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public  class TourLocation
    {
        public Guid TourLocationId { get; set; } // Khóa chính
        public Guid TourId { get; set; } // Khóa ngoại đến Tour
        public Guid LocationId { get; set; } // Khóa ngoại đến Location
        public virtual Tour Tour { get; set; }
        public virtual Location Location { get; set; }
    }
}
