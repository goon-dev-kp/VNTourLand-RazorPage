using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class Location
    {
        public Guid LocationId { get; set; }
        public string LocationName { get; set; }  // Tên địa điểm


        // Nếu muốn, có thể thêm tọa độ, ví dụ:
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public virtual ICollection<TourLocation> TourLocations { get; set; } = new HashSet<TourLocation>();

    }
}
